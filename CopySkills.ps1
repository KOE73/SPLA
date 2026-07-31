# === Copy authored skill files from one folder tree into another ===
#
# One place that knows how skills are laid out on disk, dot-sourced by whoever needs to move them:
# PublishAll.ps1 for the release layout, and available by hand for a dev deploy. Before this, the
# layout was encoded separately in the publish script and in SPLA.Skills.Network.csproj — they drifted,
# and the drift is what let a disabled plugin keep injecting its skills into the system prompt.
#
# PowerShell 5.1 compatible.

<#
.SYNOPSIS
    Mirrors a skills tree (*.md plus each skill folder's resources) from -From into -To.

.DESCRIPTION
    Copies the directory structure as-is: subfolders are meaningful, because they both group skills
    and carry a folder-skill's resources (see agents/skills.md). Excluded on the way:
      * README.md at any level — documentation about the folder, not a skill;
      * .git / .vs and any other dot-folder, bin, obj, node_modules — tooling noise.

    Copying nothing is not an error: most projects and plugins ship no skills, and the caller should
    not have to check first. Returns the number of files copied.

.PARAMETER From
    Source folder. A missing folder yields 0, silently.

.PARAMETER To
    Destination folder. Created if absent.

.PARAMETER Clean
    Remove the destination first, so a deleted skill does not survive in an incremental publish.

.EXAMPLE
    . .\CopySkills.ps1
    Copy-Skills -From 'skills' -To '.publish\work\skills' -Clean
#>
function Copy-Skills {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)] [string] $From,
        [Parameter(Mandatory = $true)] [string] $To,
        [switch] $Clean
    )

    if (-not (Test-Path -LiteralPath $From)) { return 0 }

    $sourceRoot = (Resolve-Path -LiteralPath $From).Path

    if ($Clean -and (Test-Path -LiteralPath $To)) {
        Remove-Item -LiteralPath $To -Recurse -Force
    }
    if (-not (Test-Path -LiteralPath $To)) {
        New-Item -ItemType Directory -Force -Path $To | Out-Null
    }
    $destRoot = (Resolve-Path -LiteralPath $To).Path

    $excludedDirs = @('bin', 'obj', 'node_modules')
    $copied = 0

    foreach ($file in Get-ChildItem -LiteralPath $sourceRoot -Recurse -File) {
        $relative = $file.FullName.Substring($sourceRoot.Length).TrimStart('\', '/')

        if ($file.Name -ieq 'README.md') { continue }

        # Reject the whole path if any segment is noise, so skills/.git/x.md never travels.
        $segments = $relative -split '[\\/]'
        $parents = $segments[0..($segments.Length - 2)]
        $skip = $false
        foreach ($segment in $parents) {
            if ($segment.StartsWith('.') -or ($excludedDirs -contains $segment.ToLowerInvariant())) { $skip = $true; break }
        }
        if ($skip) { continue }

        $target = Join-Path $destRoot $relative
        $targetDir = Split-Path -Parent $target
        if (-not (Test-Path -LiteralPath $targetDir)) {
            New-Item -ItemType Directory -Force -Path $targetDir | Out-Null
        }
        Copy-Item -LiteralPath $file.FullName -Destination $target -Force
        $copied++
    }

    return $copied
}
