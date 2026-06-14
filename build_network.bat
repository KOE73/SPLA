@echo off
cd /d C:\Git\SPLA
dotnet build SPLA.Plugins.Network\SPLA.Plugins.Network.csproj 2>&1 > C:\Git\SPLA\build_output.txt
echo Exit code: %ERRORLEVEL% >> C:\Git\SPLA\build_output.txt
pause
