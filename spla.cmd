@echo off
rem === spla - launcher for SPLA.CLI.exe ===
rem (ASCII only on purpose: .cmd is read in the console OEM codepage, not UTF-8)
rem
rem Put this file's folder (or a copy of this file) on PATH and call `spla ...` from anywhere.
rem A shim rather than a renamed/copied exe on purpose: SPLA.CLI.exe is a .NET apphost that loads
rem its managed assembly from its own directory, so a copy elsewhere does not start, and a rename
rem in place is undone by the next build.
rem
rem It deliberately does NOT change directory: the CLI finds its project by searching upward for a
rem *.spla from wherever it was invoked, so `cd` here would silently pick the wrong project.
rem
rem Lookup order: SPLA_CLI env var, then published build, then Release, then Debug.
rem Override for a build that lives elsewhere:  set SPLA_CLI=D:\somewhere\SPLA.CLI.exe

setlocal
set "SPLA_ROOT=%~dp0"
set "SPLA_EXE=%SPLA_CLI%"

if not defined SPLA_EXE if exist "%SPLA_ROOT%.publish\work\SPLA.CLI.exe" set "SPLA_EXE=%SPLA_ROOT%.publish\work\SPLA.CLI.exe"
if not defined SPLA_EXE if exist "%SPLA_ROOT%src\apps\SPLA.CLI\bin\Release\net10.0\SPLA.CLI.exe" set "SPLA_EXE=%SPLA_ROOT%src\apps\SPLA.CLI\bin\Release\net10.0\SPLA.CLI.exe"
if not defined SPLA_EXE if exist "%SPLA_ROOT%src\apps\SPLA.CLI\bin\Debug\net10.0\SPLA.CLI.exe" set "SPLA_EXE=%SPLA_ROOT%src\apps\SPLA.CLI\bin\Debug\net10.0\SPLA.CLI.exe"

if not defined SPLA_EXE (
    echo spla: SPLA.CLI.exe not found.
    echo   looked in: .publish\work, src\apps\SPLA.CLI\bin\{Release,Debug}\net10.0
    echo   build it:  dotnet build src\apps\SPLA.CLI\SPLA.CLI.csproj
    echo   or publish: PublishAll.cmd
    echo   or point at one: set SPLA_CLI=C:\path\to\SPLA.CLI.exe
    exit /b 9009
)

"%SPLA_EXE%" %*
exit /b %ERRORLEVEL%
