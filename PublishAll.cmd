@echo off
echo === SPLA Publish All Script ===
echo Stopping running SPLA processes...
taskkill /IM SPLA.UI.Avalonia.exe /F >nul 2>nul
taskkill /IM SPLA.CLI.exe /F >nul 2>nul

echo Cleaning publish folder...
if exist .publish\work rmdir /s /q .publish\work
mkdir .publish\work

echo Rebuilding web client (Vue) so both UI and CLI embed the fresh bundle...
if exist web\dist rmdir /s /q web\dist
if not exist web\node_modules call npm --prefix web install
if %ERRORLEVEL% neq 0 (
    echo Error installing web client dependencies.
    exit /b %ERRORLEVEL%
)
call npm --prefix web run build
if %ERRORLEVEL% neq 0 (
    echo Error building web client.
    exit /b %ERRORLEVEL%
)

echo Publishing SPLA.UI.Avalonia (UI)...
dotnet publish src/apps/SPLA.UI.Avalonia/SPLA.UI.Avalonia.csproj -p:PublishProfile=SingleFile -c Release -o .publish/work
if %ERRORLEVEL% neq 0 (
    echo Error publishing UI.
    exit /b %ERRORLEVEL%
)

echo Publishing SPLA.CLI (CLI)...
dotnet publish src/apps/SPLA.CLI/SPLA.CLI.csproj -c Release -o .publish/work
if %ERRORLEVEL% neq 0 (
    echo Error publishing CLI.
    exit /b %ERRORLEVEL%
)

echo Building plugins...
echo 1. Network Plugin...
dotnet publish src/plugins/SPLA.Plugins.Network/SPLA.Plugins.Network.csproj -c Release -o .publish/work/plugins/network
copy /y src\plugins\SPLA.Plugins.Network\meta.yaml .publish\work\plugins\network\
xcopy /s /y src\plugins\SPLA.Skills.Network\skills .publish\work\plugins\network\skills\

echo 2. Test Plugin...
dotnet publish src/plugins/SPLA.Plugins.Test/SPLA.Plugins.Test.csproj -c Release -o .publish/work/plugins/test
copy /y src\plugins\SPLA.Plugins.Test\meta.yaml .publish\work\plugins\test\

echo 3. OneC Plugin...
dotnet publish src/plugins/SPLA.Plugins.OneC/SPLA.Plugins.OneC.csproj -c Release -o .publish/work/plugins/onec
copy /y src\plugins\SPLA.Plugins.OneC\meta.yaml .publish\work\plugins\onec\
if exist src\plugins\SPLA.Plugins.OneC\Assets xcopy /s /y src\plugins\SPLA.Plugins.OneC\Assets .publish\work\plugins\onec\Assets\

echo 4. OneC Avalonia UI Plugin...
dotnet publish src/plugins/SPLA.Plugins.OneC.Avalonia/SPLA.Plugins.OneC.Avalonia.csproj -c Release -o .publish/work/plugins/onec_avalonia
copy /y src\plugins\SPLA.Plugins.OneC.Avalonia\meta.yaml .publish\work\plugins\onec_avalonia\

echo 5. SQL Plugin...
dotnet publish src/plugins/SPLA.Plugins.Sql/SPLA.Plugins.Sql.csproj -c Release -o .publish/work/plugins/sql
copy /y src\plugins\SPLA.Plugins.Sql\meta.yaml .publish\work\plugins\sql\

echo 5b. SQL Avalonia UI Plugin...
dotnet publish src/plugins/SPLA.Plugins.Sql.Avalonia/SPLA.Plugins.Sql.Avalonia.csproj -c Release -o .publish/work/plugins/sql_avalonia
copy /y src\plugins\SPLA.Plugins.Sql.Avalonia\meta.yaml .publish\work\plugins\sql_avalonia\

echo 6. Roslyn Plugin...
dotnet publish src/plugins/SPLA.Plugins.Roslyn/SPLA.Plugins.Roslyn.csproj -c Release -o .publish/work/plugins/roslyn
copy /y src\plugins\SPLA.Plugins.Roslyn\meta.yaml .publish\work\plugins\roslyn\

echo 7. Browser Plugin...
dotnet publish src/plugins/SPLA.Plugins.Browser/SPLA.Plugins.Browser.csproj -c Release -o .publish/work/plugins/browser
copy /y src\plugins\SPLA.Plugins.Browser\meta.yaml .publish\work\plugins\browser\

echo 8. SSH Plugin...
dotnet publish src/plugins/SPLA.Plugins.Ssh/SPLA.Plugins.Ssh.csproj -c Release -o .publish/work/plugins/ssh
copy /y src\plugins\SPLA.Plugins.Ssh\meta.yaml .publish\work\plugins\ssh\

echo Cleaning debug and documentation artifacts from publish work folder...
del /s /q .publish\work\*.pdb >nul 2>nul
del /s /q .publish\work\*.xml >nul 2>nul

echo Creating ZIP package...
if exist .publish\zip rmdir /s /q .publish\zip
mkdir .publish\zip
mkdir .publish\zip\stage

robocopy .publish\work .publish\zip\stage /E /XF *.pdb *.xml *.ilk *.iobj *.ipdb *.tmp *.log /XD .git obj bin >nul
if %ERRORLEVEL% GEQ 8 (
    echo Error staging files for ZIP package.
    exit /b %ERRORLEVEL%
)

powershell -NoProfile -ExecutionPolicy Bypass -Command "Compress-Archive -Path '.publish\zip\stage\*' -DestinationPath '.publish\zip\SPLA.zip' -Force"
if %ERRORLEVEL% neq 0 (
    echo Error creating ZIP package.
    exit /b %ERRORLEVEL%
)

rmdir /s /q .publish\zip\stage

echo === Publish Complete! ===
dir .publish\work
echo ZIP package:
dir .publish\zip
