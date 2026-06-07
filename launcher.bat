@echo off
cd /d "%~dp0"

:: 检查是否在桌面安装目录
if not exist "bridge-service" (
    echo ERROR: Please run from installation directory
    pause
    exit /b 1
)

echo ============================================
echo VATSIM Companion - Launcher
echo ============================================
echo.

:: 选择启动模式
echo Select mode:
echo [1] Start Bridge Service
echo [2] Stop Bridge Service
echo [3] Configure Settings
echo [4] View Logs
echo [5] Exit
echo.
set /p choice="Enter choice (1-5): "

if "%choice%"=="1" goto start_bridge
if "%choice%"=="2" goto stop_bridge
if "%choice%"=="3" goto configure
if "%choice%"=="4" goto logs
if "%choice%"=="5" exit /b 0

:start_bridge
echo.
echo Starting Bridge Service...
cd bridge-service\windows\VatsimBridge\bin\Release\net7.0
start "VATSIM Bridge" VatsimBridge.exe
echo.
echo Bridge started in new window
echo Access at: http://localhost:5000
echo.
pause
goto menu

:stop_bridge
echo.
echo Stopping Bridge Service...
taskkill /IM VatsimBridge.exe /F >nul 2>&1
echo Bridge stopped
pause
goto menu

:configure
echo.
notepad bridge-service\windows\VatsimBridge\appsettings.json
goto menu

:logs
echo.
echo Opening logs directory...
explorer bridge-service\windows\VatsimBridge\logs
goto menu

:menu
cls
goto :start_bridge
