@echo off
setlocal enabledelayedexpansion

echo ============================================
echo vPilot Plugin Auto-Installer
echo ============================================
echo.

REM Find vPilot installation
set "VPILOT_DIR="
for %%D in (
    "%ProgramFiles%\vPilot"
    "%ProgramFiles(x86)%\vPilot"
    "%LOCALAPPDATA%\vPilot"
    "C:\vPilot"
) do (
    if exist "%%~D\vPilot.exe" (
        set "VPILOT_DIR=%%~D"
        goto :found
    )
)

echo ERROR: vPilot not found. Please install vPilot first.
echo Expected locations:
echo   - %ProgramFiles%\vPilot
echo   - %ProgramFiles(x86)%\vPilot
pause
exit /b 1

:found
echo Found vPilot at: %VPILOT_DIR%
echo.

REM Check if plugin DLL exists
set "PLUGIN_DLL=vpilot-plugin\VatsimCompanionPlugin\bin\Release\net48\VatsimCompanionPlugin.dll"
if not exist "%PLUGIN_DLL%" (
    echo ERROR: Plugin not built yet. Building now...
    echo.
    cd vpilot-plugin\VatsimCompanionPlugin
    dotnet build -c Release
    if %ERRORLEVEL% NEQ 0 (
        echo ERROR: Build failed
        pause
        exit /b 1
    )
    cd ..\..
)

REM Create Plugins directory if needed
set "PLUGINS_DIR=%VPILOT_DIR%\Plugins"
if not exist "%PLUGINS_DIR%" (
    echo Creating Plugins directory...
    mkdir "%PLUGINS_DIR%"
)

REM Copy plugin
echo Installing plugin...
copy /Y "%PLUGIN_DLL%" "%PLUGINS_DIR%\"
if %ERRORLEVEL% EQU 0 (
    echo.
    echo ============================================
    echo Installation Complete!
    echo ============================================
    echo.
    echo Plugin installed to: %PLUGINS_DIR%
    echo.
    echo NEXT STEPS:
    echo 1. Start Bridge Service (use start-bridge.bat)
    echo 2. Launch vPilot
    echo 3. The plugin will auto-load on startup
    echo.
) else (
    echo ERROR: Failed to copy plugin
)

pause
