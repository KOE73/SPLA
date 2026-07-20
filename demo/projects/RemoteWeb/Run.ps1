$ErrorActionPreference = 'Stop'

$utf8 = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..'))
$cliProject = Join-Path $repoRoot 'src\apps\SPLA.CLI\SPLA.CLI.csproj'
$browserPluginProject = Join-Path $repoRoot 'src\plugins\SPLA.Plugins.Browser.Screencast\SPLA.Plugins.Browser.Screencast.csproj'
$serviceUrl = 'http://127.0.0.1:5050'
$splaUrl = "$serviceUrl/?panel=browserScreencast"
$initialMessage = 'Use the embedded Browser Lab. Open https://en.wikipedia.org, take a snapshot, find the site search field, visibly type OpenAI and submit the search, open the OpenAI article if needed, read its first paragraph, and summarize it for the user.'

function Test-TcpPort {
    param([int]$Port)

    $client = [System.Net.Sockets.TcpClient]::new()
    try {
        $connection = $client.ConnectAsync('127.0.0.1', $Port)
        return $connection.Wait(250) -and $client.Connected
    }
    catch {
        return $false
    }
    finally {
        $client.Dispose()
    }
}

if (Test-TcpPort 5050) {
    throw 'Port 5050 is already in use. Stop the existing service and run this demo again.'
}

Write-Host '=== SPLA Remote Web screencast demo ==='
Write-Host "Local:  $splaUrl"
Write-Host 'Remote: http://<this-computer-name>:5050/?panel=browserScreencast'
Write-Host

Push-Location $PSScriptRoot
$browserJob = $null
try {
    Write-Host 'Building the embedded screencast browser plugin...'
    & dotnet build $browserPluginProject -c Debug --nologo -v q
    if ($LASTEXITCODE -ne 0) { throw 'Browser plugin build failed.' }

    $browserJob = Start-Job -ScriptBlock {
        param($Url)

        $deadline = (Get-Date).AddMinutes(2)
        do {
            Start-Sleep -Milliseconds 500
            try {
                $ready = (Invoke-WebRequest -UseBasicParsing $Url -TimeoutSec 1).StatusCode -eq 200
            }
            catch {
                $ready = $false
            }
        } while (-not $ready -and (Get-Date) -lt $deadline)

        if ($ready) { Start-Process $Url }
    } -ArgumentList $splaUrl

    Write-Host 'Starting the service. Press Ctrl+C to stop.'
    & dotnet run --project $cliProject -c Debug -- serve --bind 0.0.0.0 --port 5050 --new-chat $initialMessage
    if ($LASTEXITCODE -ne 0) { throw "SPLA CLI exited with code $LASTEXITCODE." }
}
finally {
    if ($browserJob) {
        Stop-Job $browserJob -ErrorAction SilentlyContinue
        Remove-Job $browserJob -Force -ErrorAction SilentlyContinue
    }
    Pop-Location
}
