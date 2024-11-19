:: Installs/uninstalls Hourglass and its dependencies in/from the native image cache.
:: Run as an Administrator.

@echo off

set scriptName=%~nx0
set command=%~1
set faq=https://github.com/i2van/hourglass/blob/main/FAQ.md#how-to-speed-up-the-portable-hourglass-startup

:: Check permissions.
net session >nul 2>&1
if not %errorlevel%==0 goto NO_PERMISSIONS

:: Params.
if "%command%"=="install"   goto EXECUTE
if "%command%"=="uninstall" goto EXECUTE

:: Usage.
echo Usage: %scriptName% [install^|uninstall]
echo.
echo ^> %scriptName% install
echo Generates the Hourglass native image and its dependencies and installs in the native image cache.
echo.
echo ^> %scriptName% uninstall
echo Deletes the native images of the Hourglass and its dependencies from the native image cache.
echo.
echo FAQ: %faq%
exit /b 1

:EXECUTE

:: .NET Framework version.
set netVersion=4.0.30319

set nAudio=%~dp0Hourglass.NAudio.dll
set netPath=%WINDIR%\Microsoft.NET\Framework
set ngenPath=%netPath%64

if not exist "%ngenPath%" set ngenPath=%netPath%

echo on

"%ngenPath%\v%netVersion%\ngen.exe" %~1 "%~dp0Hourglass.exe" || goto EXIT
@if not exist "%nAudio%" goto EXIT
"%ngenPath%\v%netVersion%\ngen.exe" %~1 "%nAudio%" || goto EXIT

:EXIT

@exit /b %errorlevel%

:NO_PERMISSIONS

echo Please run %scriptName% as an Administrator: %faq%
exit /b 1
