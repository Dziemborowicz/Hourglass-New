:: Installs/uninstalls Hourglass and its dependencies in/from the native image cache.
:: Run as an Administrator.

@echo off

set command=%~1

if "%command%"=="install"   goto EXECUTE
if "%command%"=="uninstall" goto EXECUTE

set scriptName=%~nx0

echo Usage: %scriptName% [install^|uninstall]
echo.
echo ^> %scriptName% install
echo Generates the Hourglass native image and its dependencies and installs in the native image cache.
echo.
echo ^> %scriptName% uninstall
echo Deletes the native images of the Hourglass and its dependencies from the native image cache.
exit /b 1

:EXECUTE

:: .NET Framework directory under %WINDIR%\Microsoft.NET\Framework or %WINDIR%\Microsoft.NET\Framework64
set netVersion=4.0.30319

set netPath=%WINDIR%\Microsoft.NET\Framework
set ngenPath=%netPath%64

if not exist "%ngenPath%" set ngenPath=%netPath%

echo on

"%ngenPath%\v%netVersion%\ngen.exe" %~1 "%~dp0Hourglass.exe"

@exit /b %errorlevel%
