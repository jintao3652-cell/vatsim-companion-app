@echo off
chcp 65001 >nul
title AetherLink
setlocal

rem 安装目录(bat 所在目录)
set "ROOT=%~dp0"
set "CFLOG=%TEMP%\aetherlink-cf.log"
del "%CFLOG%" 2>nul

echo ===========================================
echo   AetherLink - starting tunnel...
echo ===========================================

rem 启动 cloudflared 临时隧道, 输出写入日志文件供解析
start "" /b "%ROOT%tunnel\cloudflared-windows-amd64.exe" tunnel --url http://localhost:5000 --logfile "%CFLOG%"

rem 轮询日志抓取 trycloudflare 公网 URL (最多 ~30 秒)
set "PUBURL="
for /l %%i in (1,1,30) do (
    if not defined PUBURL (
        for /f "tokens=*" %%u in ('powershell -NoProfile -Command "if(Test-Path '%CFLOG%'){(Select-String -Path '%CFLOG%' -Pattern 'https://[a-z0-9-]+\.trycloudflare\.com' | Select-Object -First 1).Matches.Value}"') do set "PUBURL=%%u"
        if not defined PUBURL ping -n 2 127.0.0.1 >nul
    )
)

if not defined PUBURL (
    echo   [WARN] Tunnel URL not detected. Falling back to LAN only.
) else (
    echo   Public URL: %PUBURL%
)

echo ===========================================
echo   Starting Bridge...
echo ===========================================

rem 把公网地址通过 PublicUrl 传给 Bridge, 配对二维码即用此地址
set "PublicUrl=%PUBURL%"
"%ROOT%bridge\VatsimBridge.exe"
