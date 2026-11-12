@echo off
rem Opens the architecture maps with SeMaps (https://github.com/KOE73/SeMaps).
rem The environment lives in spla.semaps; double-clicking that file works too.
where semaps >nul 2>nul || (echo semaps not found on PATH. Install it: https://github.com/KOE73/SeMaps & exit /b 1)
semaps "%~dp0spla.semaps" %*
