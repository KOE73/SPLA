@echo off
rem Thin shim — real visualizer setup and server launch live in ViewArchitecture.ps1.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0ViewArchitecture.ps1" %*
exit /b %ERRORLEVEL%
