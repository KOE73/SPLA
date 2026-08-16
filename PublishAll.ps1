# === SPLA Publish All ===
# PowerShell 5.1 compatible. Strategy:
#   1. one solution build (MSBuild parallelizes internally, no obj/ races),
#   2. plugin publishes run in parallel jobs (each to its own folder); projects
#      outside the solution build themselves, the rest reuse the solution build,
#   3. UI/CLI publish sequentially into the shared .publish/work. Their single-file
#      publishes require RID-specific outputs that the solution build does not produce.
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$sw = [System.Diagnostics.Stopwatch]::StartNew()

function Fail($msg) { Write-Host "ERROR: $msg" -ForegroundColor Red; exit 1 }

Write-Host '=== SPLA Publish All ===' -ForegroundColor Cyan

. "$PSScriptRoot\EnsureNodeJs.ps1"
. "$PSScriptRoot\CopySkills.ps1"

Write-Host 'Stopping running SPLA processes...'
foreach ($name in 'SPLA.UI.Avalonia', 'SPLA.CLI') {
    Get-Process -Name $name -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
}

Write-Host 'Cleaning publish folder...'
if (Test-Path .publish\work) { Remove-Item .publish\work -Recurse -Force }
New-Item -ItemType Directory -Force .publish\work | Out-Null

Write-Host 'Rebuilding web client (Vue) so UI and CLI embed the fresh bundle...'
if (Test-Path web\dist) { Remove-Item web\dist -Recurse -Force }
# Unconditionally, not "only when node_modules is missing". That condition is true exactly once per
# checkout, so a dependency added after someone's first install never reaches them: the build then
# fails naming a package that IS in package.json, and the only cure is knowing to run npm install by
# hand. A publish is not the place to save the couple of seconds this costs when nothing changed.
npm --prefix web install
if ($LASTEXITCODE -ne 0) { Fail 'npm install failed.' }

npm --prefix web run build
if ($LASTEXITCODE -ne 0) { Fail 'web client build failed.' }

Write-Host 'Building solution (Release, single pass)...'
dotnet build SPLA.slnx -c Release --nologo
if ($LASTEXITCODE -ne 0) { Fail 'solution build failed.' }

# Plugin name -> project path + extra payload copied next to the dll.
$plugins = @(
    @{ Name = 'network';            Proj = 'src/plugins/SPLA.Plugins.Network/SPLA.Plugins.Network.csproj';
       Extras = @(@{ From = 'src\plugins\SPLA.Skills.Network\skills'; To = 'skills' }) }
    @{ Name = 'test';               Proj = 'src/plugins/SPLA.Plugins.Test/SPLA.Plugins.Test.csproj'; Build = $true }
    @{ Name = 'onec';               Proj = 'src/plugins/SPLA.Plugins.OneC/SPLA.Plugins.OneC.csproj' }
    @{ Name = 'sql';                Proj = 'src/plugins/SPLA.Plugins.Sql/SPLA.Plugins.Sql.csproj' }
    @{ Name = 'roslyn';             Proj = 'src/plugins/SPLA.Plugins.Roslyn/SPLA.Plugins.Roslyn.csproj' }
    @{ Name = 'browser';            Proj = 'src/plugins/SPLA.Plugins.Browser/SPLA.Plugins.Browser.csproj' }
    @{ Name = 'ssh';                Proj = 'src/plugins/SPLA.Plugins.Ssh/SPLA.Plugins.Ssh.csproj' }
    @{ Name = 'browser_screencast'; Proj = 'src/plugins/SPLA.Plugins.Browser.Screencast/SPLA.Plugins.Browser.Screencast.csproj' }
)

Write-Host "Publishing $($plugins.Count) plugins in parallel..."
$jobs = foreach ($p in $plugins) {
    Start-Job -Name $p.Name -ArgumentList $PSScriptRoot, $p -ScriptBlock {
        param($root, $p)
        Set-Location $root
        $out = ".publish/work/plugins/$($p.Name)"
        $publishArguments = @('publish', $p.Proj, '-c', 'Release', '-o', $out, '--nologo', '-v', 'q')
        if (-not $p.Build) { $publishArguments += '--no-build' }
        dotnet @publishArguments 2>&1 | Out-String | Write-Output
        if ($LASTEXITCODE -ne 0) { throw "publish failed for $($p.Name)" }
        $metaDir = Split-Path $p.Proj -Parent
        Copy-Item (Join-Path $metaDir 'meta.yaml') $out -Force

        # Skill trees go through the shared helper — one definition of what a skills folder is.
        # Jobs run in their own runspace, so it has to be dot-sourced again here.
        . (Join-Path $root 'CopySkills.ps1')
        foreach ($e in @($p.Extras)) {
            if ($e) { Copy-Skills -From $e.From -To (Join-Path $out $e.To) | Out-Null }
        }
        "plugin $($p.Name): OK"
    }
}

# Apps publish while plugin jobs run: they write to the shared root, plugins to their own subfolders.
Write-Host 'Publishing SPLA.UI.Avalonia (SingleFile profile)...'
dotnet publish src/apps/SPLA.UI.Avalonia/SPLA.UI.Avalonia.csproj -p:PublishProfile=SingleFile -c Release -o .publish/work --nologo
if ($LASTEXITCODE -ne 0) { Fail 'UI publish failed.' }

Write-Host 'Publishing SPLA.CLI...'
dotnet publish src/apps/SPLA.CLI/SPLA.CLI.csproj -c Release -o .publish/work --nologo
if ($LASTEXITCODE -ne 0) { Fail 'CLI publish failed.' }

Write-Host 'Waiting for plugin jobs...'
$failed = @()
foreach ($j in $jobs) {
    try {
        Receive-Job -Job $j -Wait -ErrorAction Stop | ForEach-Object { Write-Host "  $_" }
    } catch {
        Write-Host "  plugin $($j.Name): FAILED -- $_" -ForegroundColor Red
        $failed += $j.Name
    }
    Remove-Job -Job $j -Force
}
if ($failed.Count -gt 0) { Fail ("plugin publish failed: " + ($failed -join ', ')) }

# Skills that ship with the product, laid out beside plugins/ where the runtime looks for them
# (SkillSourceRegistry appends <install>/skills as the lowest-priority source, so a project's own
# skills/ folder overrides a shipped skill by reusing its id).
Write-Host 'Copying shipped skills...'
$skillCount = Copy-Skills -From 'skills' -To '.publish\work\skills' -Clean
Write-Host "  $skillCount skill file(s)"

Write-Host 'Cleaning debug and documentation artifacts...'
Get-ChildItem .publish\work -Recurse -Include *.pdb, *.xml | Remove-Item -Force

# Stamped so the running build can tell the client which branch it was published from (ClientConnection
# reads this next to the exe and puts it on the welcome frame) — and so registration below always points
# Explorer at whichever folder was built last, not whichever was built first.
# Best-effort, and it has to stay that way: git is not needed to BUILD anything here, so a publish
# must not die because it is missing from PATH or because this tree is not a repository (an extracted
# zip, a copied folder). The reader already treats a missing branch.txt as a dev build and shows no
# banner — see SystemOps.ReadBuildBranch — so saying nothing is a state it understands.
$branch = ''
if (Get-Command git -ErrorAction SilentlyContinue) {
    $branch = (& git rev-parse --abbrev-ref HEAD 2>$null | Out-String).Trim()
    if ($LASTEXITCODE -ne 0) { $branch = '' }
}

if ($branch) {
    Set-Content -Path '.publish\work\branch.txt' -Value $branch -NoNewline
    Write-Host "Branch stamp: $branch"
}
else {
    Write-Host '  (no branch stamp: git unavailable or not a repository)' -ForegroundColor Yellow
    # Clear git's exit code so a failure we deliberately ignored cannot be read as the next step's.
    $global:LASTEXITCODE = 0
}

Write-Host 'Registering .spla file association to this build...'
& '.publish\work\SPLA.CLI.exe' system register-association
if ($LASTEXITCODE -ne 0) { Write-Host '  (non-fatal: association not registered)' -ForegroundColor Yellow }

Write-Host 'Creating ZIP package...'
if (Test-Path .publish\zip) { Remove-Item .publish\zip -Recurse -Force }
New-Item -ItemType Directory -Force .publish\zip | Out-Null
Compress-Archive -Path '.publish\work\*' -DestinationPath '.publish\zip\SPLA.zip' -Force

$sw.Stop()
Write-Host ("=== Publish Complete in {0:mm\:ss} ===" -f $sw.Elapsed) -ForegroundColor Green
Get-ChildItem .publish\work | Format-Table Name, Length -AutoSize
Write-Host 'ZIP package:'
Get-ChildItem .publish\zip
