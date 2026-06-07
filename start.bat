@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul
title VATSIM Companion - Launcher

REM ============================================================
REM  One-click start: Bridge Service + cloudflared tunnel
REM  (vPilot and plugin need to be started manually via vPilot)
REM ============================================================

REM ---- Configuration ----
REM Bridge listening port (must match Port in appsettings.json)
set BRIDGE_PORT=5000
REM cloudflared.exe path. Leave empty if already in system PATH.
REM Otherwise fill in full path, e.g.: set CLOUDFLARED=C:\Tools\cloudflared.exe
set CLOUDFLARED=

set SCRIPT_DIR=%~dp0
set BRIDGE_DIR=%SCRIPT_DIR%bridge-service\windows\VatsimBridge

echo ============================================
echo   VATSIM Companion - Starting
echo ============================================
echo.

REM ---- Check dotnet ----
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] .NET SDK not found. Please install: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

REM ---- Detect cloudflared ----
if "%CLOUDFLARED%"=="" (
    where cloudflared >nul 2>nul
    if !ERRORLEVEL! EQU 0 (
        set CLOUDFLARED=cloudflared
    )
)
if "%CLOUDFLARED%"=="" (
    echo [WARNING] cloudflared not found. Will only start Bridge (LAN access only).
    echo           For internet access, edit CLOUDFLARED variable at the top of this script.
    echo.
    set SKIP_TUNNEL=1
)

REM ---- Start Bridge (new window) ----
echo [1/2] Starting Bridge Service (port %BRIDGE_PORT%)...
start "VATSIM Bridge" cmd /k "cd /d "%BRIDGE_DIR%" && dotnet run"

REM Wait for Bridge to start
timeout /t 6 /nobreak >nul

REM ---- Start cloudflared tunnel (new window) ----
if not defined SKIP_TUNNEL (
    echo [2/2] Starting cloudflared tunnel -^> http://localhost:%BRIDGE_PORT% ...
    start "Cloudflared Tunnel" cmd /k ""%CLOUDFLARED%" tunnel --url http://localhost:%BRIDGE_PORT%"
    echo.
    echo Tunnel URL will be shown in the "Cloudflared Tunnel" window
    echo   (like https://xxxx-xxxx.trycloudflare.com). Enter it in mobile app.
) else (
    echo [2/2] Tunnel skipped. Use LAN IP:%BRIDGE_PORT% to connect from mobile.
)

echo.
echo ============================================
echo   Startup Complete
echo ============================================
echo.
echo   Reminder: Please manually start vPilot (plugin loads automatically).
echo.
echo You can close this window. Bridge and tunnel are running in separate windows.
pause
endlocal
