@echo off
rem Opens the architecture diagrams in SeMaps. Needs the semaps binary on PATH.
where semaps >nul 2>nul || (echo semaps not found on PATH. Install SeMaps first. & exit /b 1)
semaps --workspace "%~dp0docs\diagrams" --source-root "%~dp0." %*
