# Finds a usable Node.js installation and adds it to PATH for the current process.
# Dot-source this file from PowerShell build scripts so the PATH change persists:
#   . "$PSScriptRoot\EnsureNodeJs.ps1"
#
# CMD build scripts use -PathOnly, capture the returned directory, and prepend it
# to their own PATH. Diagnostics are written to stderr in that mode.
[CmdletBinding()]
param(
    [switch] $PathOnly
)

$ErrorActionPreference = 'Stop'

function Write-Diagnostic {
    param(
        [Parameter(Mandatory)]
        [string] $Message,

        [ConsoleColor] $Color = [ConsoleColor]::Gray
    )

    if ($PathOnly) {
        [Console]::Error.WriteLine($Message)
        return
    }

    Write-Host $Message -ForegroundColor $Color
}

function Add-Candidate {
    param(
        [AllowNull()]
        [string] $Directory,

        [Parameter(Mandatory)]
        [string] $Source
    )

    if ([string]::IsNullOrWhiteSpace($Directory)) {
        return
    }

    $expandedDirectory = [Environment]::ExpandEnvironmentVariables($Directory.Trim('"'))
    if (-not (Test-Path -LiteralPath $expandedDirectory -PathType Container)) {
        return
    }

    try {
        $resolvedDirectory = (Resolve-Path -LiteralPath $expandedDirectory).Path
    } catch {
        return
    }

    if ($candidateDirectories.Add($resolvedDirectory)) {
        $candidates.Add([pscustomobject]@{
            Directory = $resolvedDirectory
            Source = $Source
        })
    }
}

function Get-ExecutableDirectory {
    param(
        [Parameter(Mandatory)]
        [string] $Command
    )

    $resolvedCommand = Get-Command $Command -CommandType Application -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if ($resolvedCommand) {
        Split-Path -Parent $resolvedCommand.Source
    }
}

function Find-UsableCandidate {
    foreach ($candidate in $candidates) {
        $nodeExecutable = Join-Path $candidate.Directory 'node.exe'
        $npmExecutable = Join-Path $candidate.Directory 'npm.cmd'
        if (-not (Test-Path -LiteralPath $nodeExecutable -PathType Leaf) -or
            -not (Test-Path -LiteralPath $npmExecutable -PathType Leaf)) {
            continue
        }

        try {
            $nodeOutput = & $nodeExecutable --version 2>$null
            $nodeExitCode = $LASTEXITCODE
            $npmOutput = & $npmExecutable --version 2>$null
            $npmExitCode = $LASTEXITCODE
            $candidateNodeVersion = ($nodeOutput | Select-Object -First 1).Trim()
            $candidateNpmVersion = ($npmOutput | Select-Object -First 1).Trim()
            if ($nodeExitCode -ne 0 -or $npmExitCode -ne 0 -or
                -not $candidateNodeVersion -or -not $candidateNpmVersion) {
                continue
            }

            return [pscustomobject]@{
                Candidate = $candidate
                NodeVersion = $candidateNodeVersion
                NpmVersion = $candidateNpmVersion
            }
        } catch {
            continue
        }
    }

    return $null
}

Write-Diagnostic 'Checking Node.js required by the web build...' Cyan

$candidateDirectories = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::OrdinalIgnoreCase)
$candidates = [System.Collections.Generic.List[object]]::new()

Add-Candidate (Get-ExecutableDirectory 'node.exe') 'PATH (node.exe)'
Add-Candidate (Get-ExecutableDirectory 'npm.cmd') 'PATH (npm.cmd)'
Add-Candidate $env:NODEJS_HOME 'NODEJS_HOME'
Add-Candidate $env:NODE_HOME 'NODE_HOME'
Add-Candidate $env:NVM_SYMLINK 'NVM_SYMLINK'
Add-Candidate (Join-Path $env:ProgramFiles 'nodejs') 'Program Files'
Add-Candidate $(if (${env:ProgramFiles(x86)}) {
    Join-Path ${env:ProgramFiles(x86)} 'nodejs'
}) 'Program Files (x86)'
Add-Candidate $(if ($env:LOCALAPPDATA) {
    Join-Path $env:LOCALAPPDATA 'Programs\nodejs'
}) 'Local AppData'
Add-Candidate $(if ($env:LOCALAPPDATA) {
    Join-Path $env:LOCALAPPDATA 'Volta\bin'
}) 'Volta'
Add-Candidate $(if ($env:USERPROFILE) {
    Join-Path $env:USERPROFILE '.volta\bin'
}) 'Volta'

$selection = Find-UsableCandidate
if (-not $selection) {
    $vswherePath = if (${env:ProgramFiles(x86)}) {
        Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    }
    if ($vswherePath -and (Test-Path -LiteralPath $vswherePath -PathType Leaf)) {
        Write-Diagnostic "Searching Visual Studio installations via: $vswherePath"
        $visualStudioNodeExecutables = & $vswherePath -products * -find '**\node.exe' 2>$null
        foreach ($nodeExecutable in $visualStudioNodeExecutables) {
            Add-Candidate (Split-Path -Parent $nodeExecutable) 'Visual Studio'
        }
        $selection = Find-UsableCandidate
    } else {
        Write-Diagnostic 'Visual Studio locator: not installed.'
    }
}

if (-not $selection) {
    Write-Diagnostic 'ERROR: No usable Node.js installation was found.' Red
    Write-Diagnostic 'Install Node.js with npm, or add its directory to PATH.' Yellow
    Write-Diagnostic 'Searched PATH, NODE_HOME/NODEJS_HOME, standard Node.js, Volta/NVM, and Visual Studio locations.'
    exit 1
}

$selectedCandidate = $selection.Candidate
$nodeVersion = $selection.NodeVersion
$npmVersion = $selection.NpmVersion
$nodeDirectory = $selectedCandidate.Directory
$env:PATH = "$nodeDirectory;$env:PATH"
Write-Diagnostic "Node.js found via $($selectedCandidate.Source): $nodeDirectory" Green
Write-Diagnostic "  node: $nodeVersion"
Write-Diagnostic "  npm:  $npmVersion"

if ($PathOnly) {
    Write-Output $nodeDirectory
}
