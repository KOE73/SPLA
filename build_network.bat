@echo off
cd /d C:\GitKOE\SPLA
dotnet build src\plugins\SPLA.Plugins.Network\SPLA.Plugins.Network.csproj 2>&1 > C:\GitKOE\SPLA\build_output.txt
echo Exit code: %ERRORLEVEL% >> C:\GitKOE\SPLA\build_output.txt
pause
