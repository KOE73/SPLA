@echo off
setlocal
echo === SPLA Run Server (build web + identity DLL + serve + open browser) ===

echo Stopping running SPLA processes...
taskkill /IM SPLA.UI.Avalonia.exe /F >nul 2>nul
taskkill /IM SPLA.Server.exe /F >nul 2>nul
taskkill /IM SPLA.CLI.exe /F >nul 2>nul

if not exist web\node_modules (
    echo Installing web client dependencies...
    call npm --prefix web install
    if errorlevel 1 (
        echo Error installing web client dependencies.
        exit /b 1
    )
)

echo Rebuilding web client (Vue) so the served bundle is fresh...
call npm --prefix web run build
if errorlevel 1 (
    echo Error building web client.
    exit /b 1
)

echo Building the Windows identity provider DLL (loaded by server.json, not referenced)...
dotnet build src\identity\SPLA.Identity.Windows\SPLA.Identity.Windows.csproj -c Debug --nologo -v q
if errorlevel 1 (
    echo Error building identity provider.
    exit /b 1
)

set SPLA_URL=http://127.0.0.1:5050

echo Opening %SPLA_URL% in the browser once the server is up...
start "" /b powershell -NoProfile -Command "Start-Sleep -Seconds 7; Start-Process '%SPLA_URL%'"

echo Starting SPLA.Server (build + serve) on 0.0.0.0:5050 with Negotiate auth ...
echo (Ctrl+C to stop.)
dotnet run --project src/apps/SPLA.Server -c Debug -- --port 5050

endlocal
