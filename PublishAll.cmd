@echo off
rem Thin shim — the real publish pipeline lives in PublishAll.ps1 (parallel plugin publish).
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0PublishAll.ps1"
exit /b %ERRORLEVEL%
