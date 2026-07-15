# === SPLA Publish All ===
# PowerShell 5.1 compatible. Strategy:
#   1. one solution build (MSBuild parallelizes internally, no obj/ races),
#   2. plugin publishes run --no-build in parallel jobs (each to its own folder),
#   3. UI/CLI publish sequentially into the shared .publish/work (UI's SingleFile
#      profile sets its own RID, so it cannot reuse the solution build).
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$sw = [System.Diagnostics.Stopwatch]::StartNew()

function Fail($msg) { Write-Host "ERROR: $msg" -ForegroundColor Red; exit 1 }

Write-Host '=== SPLA Publish All ===' -ForegroundColor Cyan

Write-Host 'Stopping running SPLA processes...'
foreach ($name in 'SPLA.UI.Avalonia', 'SPLA.CLI') {
    Get-Process -Name $name -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
}

Write-Host 'Cleaning publish folder...'
if (Test-Path .publish\work) { Remove-Item .publish\work -Recurse -Force }
New-Item -ItemType Directory -Force .publish\work | Out-Null

Write-Host 'Rebuilding web client (Vue) so UI and CLI embed the fresh bundle...'
if (Test-Path web\dist) { Remove-Item web\dist -Recurse -Force }
if (-not (Test-Path web\node_modules)) {
    npm --prefix web install
    if ($LASTEXITCODE -ne 0) { Fail 'npm install failed.' }
}
npm --prefix web run build
if ($LASTEXITCODE -ne 0) { Fail 'web client build failed.' }

Write-Host 'Building solution (Release, single pass)...'
dotnet build SPLA.slnx -c Release --nologo
if ($LASTEXITCODE -ne 0) { Fail 'solution build failed.' }

# Plugin name -> project path + extra payload copied next to the dll.
$plugins = @(
    @{ Name = 'network';            Proj = 'src/plugins/SPLA.Plugins.Network/SPLA.Plugins.Network.csproj';
       Extras = @(@{ From = 'src\plugins\SPLA.Skills.Network\skills'; To = 'skills' }) }
    @{ Name = 'test';               Proj = 'src/plugins/SPLA.Plugins.Test/SPLA.Plugins.Test.csproj' }
    @{ Name = 'onec';               Proj = 'src/plugins/SPLA.Plugins.OneC/SPLA.Plugins.OneC.csproj';
       Extras = @(@{ From = 'src\plugins\SPLA.Plugins.OneC\Assets'; To = 'Assets' }) }
    @{ Name = 'onec_avalonia';      Proj = 'src/plugins/SPLA.Plugins.OneC.Avalonia/SPLA.Plugins.OneC.Avalonia.csproj' }
    @{ Name = 'sql';                Proj = 'src/plugins/SPLA.Plugins.Sql/SPLA.Plugins.Sql.csproj' }
    @{ Name = 'sql_avalonia';       Proj = 'src/plugins/SPLA.Plugins.Sql.Avalonia/SPLA.Plugins.Sql.Avalonia.csproj' }
    @{ Name = 'roslyn';             Proj = 'src/plugins/SPLA.Plugins.Roslyn/SPLA.Plugins.Roslyn.csproj' }
    @{ Name = 'browser';            Proj = 'src/plugins/SPLA.Plugins.Browser/SPLA.Plugins.Browser.csproj' }
    @{ Name = 'ssh';                Proj = 'src/plugins/SPLA.Plugins.Ssh/SPLA.Plugins.Ssh.csproj' }
    @{ Name = 'browser_screencast'; Proj = 'src/plugins/SPLA.Plugins.Browser.Screencast/SPLA.Plugins.Browser.Screencast.csproj' }
)

Write-Host "Publishing $($plugins.Count) plugins in parallel (--no-build)..."
$jobs = foreach ($p in $plugins) {
    Start-Job -Name $p.Name -ArgumentList $PSScriptRoot, $p -ScriptBlock {
        param($root, $p)
        Set-Location $root
        $out = ".publish/work/plugins/$($p.Name)"
        dotnet publish $p.Proj -c Release --no-build -o $out --nologo -v q 2>&1 | Out-String | Write-Output
        if ($LASTEXITCODE -ne 0) { throw "publish failed for $($p.Name)" }
        $metaDir = Split-Path $p.Proj -Parent
        Copy-Item (Join-Path $metaDir 'meta.yaml') $out -Force
        foreach ($e in @($p.Extras)) {
            if ($e -and (Test-Path $e.From)) {
                Copy-Item $e.From (Join-Path $out $e.To) -Recurse -Force
            }
        }
        "plugin $($p.Name): OK"
    }
}

# Apps publish while plugin jobs run: they write to the shared root, plugins to their own subfolders.
Write-Host 'Publishing SPLA.UI.Avalonia (SingleFile profile)...'
dotnet publish src/apps/SPLA.UI.Avalonia/SPLA.UI.Avalonia.csproj -p:PublishProfile=SingleFile -c Release -o .publish/work --nologo
if ($LASTEXITCODE -ne 0) { Fail 'UI publish failed.' }

Write-Host 'Publishing SPLA.CLI...'
dotnet publish src/apps/SPLA.CLI/SPLA.CLI.csproj -c Release --no-build -o .publish/work --nologo
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

Write-Host 'Cleaning debug and documentation artifacts...'
Get-ChildItem .publish\work -Recurse -Include *.pdb, *.xml | Remove-Item -Force

Write-Host 'Creating ZIP package...'
if (Test-Path .publish\zip) { Remove-Item .publish\zip -Recurse -Force }
New-Item -ItemType Directory -Force .publish\zip | Out-Null
Compress-Archive -Path '.publish\work\*' -DestinationPath '.publish\zip\SPLA.zip' -Force

$sw.Stop()
Write-Host ("=== Publish Complete in {0:mm\:ss} ===" -f $sw.Elapsed) -ForegroundColor Green
Get-ChildItem .publish\work | Format-Table Name, Length -AutoSize
Write-Host 'ZIP package:'
Get-ChildItem .publish\zip
