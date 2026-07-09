@echo off
setlocal
echo === SPLA Publish Server (full bundle with plugins) ===
echo Stopping running SPLA processes...
taskkill /IM SPLA.Server.exe /F >nul 2>nul
taskkill /IM SPLA.CLI.exe /F >nul 2>nul

set OUT=.publish\server

echo Cleaning %OUT% ...
if exist %OUT% rmdir /s /q %OUT%
mkdir %OUT%

echo Rebuilding web client (Vue) so the server embeds the fresh bundle...
if not exist web\node_modules call npm --prefix web install
if errorlevel 1 ( echo Error installing web deps. & exit /b 1 )
call npm --prefix web run build
if errorlevel 1 ( echo Error building web client. & exit /b 1 )

echo Publishing SPLA.Server (host, platform-neutral)...
dotnet publish src/apps/SPLA.Server/SPLA.Server.csproj -c Release -o %OUT%
if errorlevel 1 ( echo Error publishing server. & exit /b 1 )

echo Publishing Windows identity provider DLL into identity\ (loaded by server.json, not referenced)...
dotnet publish src/identity/SPLA.Identity.Windows/SPLA.Identity.Windows.csproj -c Release -o %OUT%\identity
if errorlevel 1 ( echo Error publishing identity provider. & exit /b 1 )

echo Building plugins into plugins\ (same layout the runtime scans: ^<exe^>\plugins\^<name^>)...
echo 1. Network...
dotnet publish src/plugins/SPLA.Plugins.Network/SPLA.Plugins.Network.csproj -c Release -o %OUT%\plugins\network
copy /y src\plugins\SPLA.Plugins.Network\meta.yaml %OUT%\plugins\network\
if exist src\plugins\SPLA.Skills.Network\skills xcopy /s /y src\plugins\SPLA.Skills.Network\skills %OUT%\plugins\network\skills\

echo 2. OneC...
dotnet publish src/plugins/SPLA.Plugins.OneC/SPLA.Plugins.OneC.csproj -c Release -o %OUT%\plugins\onec
copy /y src\plugins\SPLA.Plugins.OneC\meta.yaml %OUT%\plugins\onec\
if exist src\plugins\SPLA.Plugins.OneC\Assets xcopy /s /y src\plugins\SPLA.Plugins.OneC\Assets %OUT%\plugins\onec\Assets\

echo 3. SQL...
dotnet publish src/plugins/SPLA.Plugins.Sql/SPLA.Plugins.Sql.csproj -c Release -o %OUT%\plugins\sql
copy /y src\plugins\SPLA.Plugins.Sql\meta.yaml %OUT%\plugins\sql\

echo 4. Roslyn...
dotnet publish src/plugins/SPLA.Plugins.Roslyn/SPLA.Plugins.Roslyn.csproj -c Release -o %OUT%\plugins\roslyn
copy /y src\plugins\SPLA.Plugins.Roslyn\meta.yaml %OUT%\plugins\roslyn\

echo 5. Browser...
dotnet publish src/plugins/SPLA.Plugins.Browser/SPLA.Plugins.Browser.csproj -c Release -o %OUT%\plugins\browser
copy /y src\plugins\SPLA.Plugins.Browser\meta.yaml %OUT%\plugins\browser\

echo Cleaning debug artifacts...
del /s /q %OUT%\*.pdb >nul 2>nul

echo.
echo === Server bundle ready: %OUT% ===
echo Run it with:  %OUT%\SPLA.Server.exe --port 5050
echo (server.json in that folder sets serverRoot and the identity provider DLL)
dir %OUT%
endlocal
