@echo off
echo === SPLA Publish All Script ===
echo Stopping running SPLA processes...
taskkill /IM SPLA.UI.Avalonia.exe /F >nul 2>nul
taskkill /IM SPLA.CLI.exe /F >nul 2>nul

echo Cleaning publish folder...
if exist .publish\work rmdir /s /q .publish\work
mkdir .publish\work

echo Publishing SPLA.UI.Avalonia (UI)...
dotnet publish SPLA.UI.Avalonia/SPLA.UI.Avalonia.csproj -p:PublishProfile=SingleFile -c Release -o .publish/work
if %ERRORLEVEL% neq 0 (
    echo Error publishing UI.
    exit /b %ERRORLEVEL%
)

echo Publishing SPLA.CLI (CLI)...
dotnet publish SPLA.CLI/SPLA.CLI.csproj -c Release -o .publish/work
if %ERRORLEVEL% neq 0 (
    echo Error publishing CLI.
    exit /b %ERRORLEVEL%
)

echo Building plugins...
echo 1. Network Plugin...
dotnet publish SPLA.Plugins.Network/SPLA.Plugins.Network.csproj -c Release -o .publish/work/plugins/network
copy /y SPLA.Plugins.Network\meta.yaml .publish\work\plugins\network\
xcopy /s /y SPLA.Skills.Network\skills .publish\work\plugins\network\skills\

echo 2. Test Plugin...
dotnet publish SPLA.Plugins.Test/SPLA.Plugins.Test.csproj -c Release -o .publish/work/plugins/test
copy /y SPLA.Plugins.Test\meta.yaml .publish\work\plugins\test\

echo 3. OneC Plugin...
dotnet publish SPLA.Plugins.OneC/SPLA.Plugins.OneC.csproj -c Release -o .publish/work/plugins/onec
copy /y SPLA.Plugins.OneC\meta.yaml .publish\work\plugins\onec\
if exist SPLA.Plugins.OneC\Assets xcopy /s /y SPLA.Plugins.OneC\Assets .publish\work\plugins\onec\Assets\

echo 4. OneC Avalonia UI Plugin...
dotnet publish SPLA.Plugins.OneC.Avalonia/SPLA.Plugins.OneC.Avalonia.csproj -c Release -o .publish/work/plugins/onec_avalonia
copy /y SPLA.Plugins.OneC.Avalonia\meta.yaml .publish\work\plugins\onec_avalonia\

echo 5. SQL Plugin...
dotnet publish SPLA.Plugins.Sql/SPLA.Plugins.Sql.csproj -c Release -o .publish/work/plugins/sql
copy /y SPLA.Plugins.Sql\meta.yaml .publish\work\plugins\sql\

echo 5b. SQL Avalonia UI Plugin...
dotnet publish SPLA.Plugins.Sql.Avalonia/SPLA.Plugins.Sql.Avalonia.csproj -c Release -o .publish/work/plugins/sql_avalonia
copy /y SPLA.Plugins.Sql.Avalonia\meta.yaml .publish\work\plugins\sql_avalonia\

echo 6. Roslyn Plugin...
dotnet publish SPLA.Plugins.Roslyn/SPLA.Plugins.Roslyn.csproj -c Release -o .publish/work/plugins/roslyn
copy /y SPLA.Plugins.Roslyn\meta.yaml .publish\work\plugins\roslyn\

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
