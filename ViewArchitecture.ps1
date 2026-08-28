# === SPLA Architecture Visualizer Launcher ===
# Bootstraps prerequisites (Node.js, Go, web bundle build) and launches the visualizer server.
param(
    [switch] $SkipBuild
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

function Fail($msg) {
    Write-Host "ERROR: $msg" -ForegroundColor Red
    exit 1
}

Write-Host '=== SPLA Architecture Visualizer ===' -ForegroundColor Cyan

# 1. Ensure Node.js is on PATH
. "$PSScriptRoot\EnsureNodeJs.ps1"

# 2. Check for Go
$goCmd = Get-Command 'go' -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $goCmd) {
    # Check standard Go installation directories
    $standardGoDirs = @(
        "$env:ProgramFiles\Go\bin",
        "${env:ProgramFiles(x86)}\Go\bin",
        "$env:USERPROFILE\go\bin",
        "$env:LOCALAPPDATA\Programs\Go\bin"
    )
    foreach ($dir in $standardGoDirs) {
        if (Test-Path (Join-Path $dir 'go.exe')) {
            $env:PATH = "$dir;$env:PATH"
            $goCmd = Get-Command 'go' -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
            break
        }
    }
}

if (-not $goCmd) {
    Fail "Go compiler (go.exe) was not found in PATH or standard installation paths. Please install Go from https://go.dev/dl/."
}

$goVersion = (& go version 2>$null) | Select-Object -First 1
Write-Host "Go found: $goVersion" -ForegroundColor Green

# 3. Build spla-diagram app if needed
$toolDir = Join-Path $PSScriptRoot 'tools\spla-diagram'
$appIndex = Join-Path $PSScriptRoot 'docs\diagrams\app\index.html'

if (-not $SkipBuild) {
    Write-Host 'Building architecture visualizer web app (spla-diagram)...' -ForegroundColor Cyan
    Push-Location $toolDir
    try {
        if (-not (Test-Path 'node_modules')) {
            Write-Host 'Installing spla-diagram npm dependencies...'
            npm install
            if ($LASTEXITCODE -ne 0) { Fail 'npm install in tools/spla-diagram failed.' }
        }

        Write-Host 'Building app bundle for docs/diagrams/app...'
        npm run build:app
        if ($LASTEXITCODE -ne 0) { Fail 'npm run build:app failed.' }
    }
    finally {
        Pop-Location
    }
}

if (-not (Test-Path $appIndex)) {
    Fail "Web app bundle not found at '$appIndex'. Run without -SkipBuild to build it."
}

# 4. Start Go server
Write-Host 'Starting Go server for SPLA Visualizer...' -ForegroundColor Cyan
$diagramsDir = Join-Path $PSScriptRoot 'docs\diagrams'
Push-Location $diagramsDir
try {
    go run server.go
}
finally {
    Pop-Location
}
