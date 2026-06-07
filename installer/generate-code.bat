@echo off
chcp 65001 >nul
title AetherLink - Pairing Code Generator
setlocal

set "BRIDGE=http://localhost:5000"

:menu
cls
echo ===========================================
echo   AetherLink Pairing
echo ===========================================
echo   [1] Show address and code (type manually)
echo   [2] Show QR code (scan with the app)
echo ===========================================
set /p "choice=Select 1 or 2: "

if "%choice%"=="1" goto show_text
if "%choice%"=="2" goto show_qr
goto menu

:show_text
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "try { $r = Invoke-RestMethod -Method Post -Uri '%BRIDGE%/api/pairing/start' -TimeoutSec 10; Write-Host ''; Write-Host ('  Address : ' + $r.bridgeUrl) -ForegroundColor Cyan; Write-Host ('  Code    : ' + $r.pairingCode) -ForegroundColor Yellow; Write-Host '' } catch { Write-Host '  Bridge not running. Start AetherLink first.' -ForegroundColor Red }"
echo.
pause
goto menu

:show_qr
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "try { $r = Invoke-RestMethod -Method Post -Uri '%BRIDGE%/api/pairing/start' -TimeoutSec 10; $b64 = ($r.qrCode -replace '^data:image/png;base64,',''); $f = Join-Path $env:TEMP 'aetherlink-qr.png'; [IO.File]::WriteAllBytes($f, [Convert]::FromBase64String($b64)); Start-Process $f; Write-Host ('  Code: ' + $r.pairingCode) -ForegroundColor Yellow; Write-Host '  QR image opened. Scan it in the app.' -ForegroundColor Green } catch { Write-Host '  Bridge not running. Start AetherLink first.' -ForegroundColor Red }"
echo.
pause
goto menu
