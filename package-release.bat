@echo off
setlocal enabledelayedexpansion

echo ============================================
echo VATSIM Companion - Release Packager
echo ============================================
echo.

set "RELEASE_DIR=release"
set "VERSION=1.0.0"

:: 创建发布目录
if exist "%RELEASE_DIR%" (
    echo Cleaning old release...
    rmdir /s /q "%RELEASE_DIR%"
)
mkdir "%RELEASE_DIR%"

echo Creating release package...
echo.

:: 1. 复制 Bridge 服务
echo [1/5] Packaging Bridge Service...
mkdir "%RELEASE_DIR%\bridge-service"
xcopy /E /I /Y "bridge-service\windows" "%RELEASE_DIR%\bridge-service\windows" >nul
xcopy /E /I /Y "bridge-service\linux" "%RELEASE_DIR%\bridge-service\linux" >nul 2>nul
echo Bridge Service: OK

:: 2. 复制 vPilot 插件
echo [2/5] Packaging vPilot Plugin...
mkdir "%RELEASE_DIR%\vpilot-plugin"
xcopy /E /I /Y "vpilot-plugin" "%RELEASE_DIR%\vpilot-plugin" >nul
echo vPilot Plugin: OK

:: 3. 复制 APK（如果存在）
echo [3/5] Packaging Mobile APK...
if exist "mobile-app\build\app\outputs\flutter-apk\app-release.apk" (
    mkdir "%RELEASE_DIR%\mobile-app"
    copy /Y "mobile-app\build\app\outputs\flutter-apk\app-release.apk" "%RELEASE_DIR%\mobile-app\vatsim-companion.apk" >nul
    echo Mobile APK: OK
) else (
    echo Mobile APK: NOT FOUND (run build-apk.bat first)
)

:: 4. 复制启动脚本和文档
echo [4/5] Packaging Scripts and Docs...
copy /Y "install-desktop.bat" "%RELEASE_DIR%\" >nul
copy /Y "launcher.bat" "%RELEASE_DIR%\" >nul
copy /Y "start.bat" "%RELEASE_DIR%\" >nul
copy /Y "build-apk.bat" "%RELEASE_DIR%\" >nul
copy /Y "README.md" "%RELEASE_DIR%\" >nul
copy /Y "DESKTOP_QUICK_START.md" "%RELEASE_DIR%\" >nul
copy /Y "QUICK_START.md" "%RELEASE_DIR%\" >nul 2>nul
echo Scripts: OK

:: 5. 创建安装说明
echo [5/5] Creating installation guide...
(
echo ============================================
echo VATSIM Companion v%VERSION%
echo ============================================
echo.
echo QUICK INSTALL:
echo.
echo 1. Desktop Installation:
echo    - Run install-desktop.bat
echo    - Find shortcut on Desktop
echo.
echo 2. Mobile Installation:
echo    - Copy mobile-app\vatsim-companion.apk to phone
echo    - Install APK
echo.
echo 3. Start Using:
echo    - Start Bridge via desktop shortcut
echo    - Open mobile app
echo    - Enter PC IP address
echo    - Enter pairing code: 123456
echo.
echo DOCUMENTATION:
echo - DESKTOP_QUICK_START.md - Desktop setup guide
echo - README.md - Full documentation
echo.
echo SUPPORT:
echo - Issues: Report on GitHub
echo - Docs: See QUICK_START.md
echo.
) > "%RELEASE_DIR%\INSTALL.txt"

echo Installation Guide: OK
echo.

:: 压缩为 ZIP（使用 PowerShell）
echo Creating ZIP archive...
set "ZIP_NAME=vatsim-companion-v%VERSION%.zip"

powershell -Command "Compress-Archive -Path '%RELEASE_DIR%\*' -DestinationPath '%ZIP_NAME%' -Force"

if exist "%ZIP_NAME%" (
    echo.
    echo ============================================
    echo Release Package Created!
    echo ============================================
    echo.
    echo Location: %CD%\%ZIP_NAME%
    echo.
    echo Package Contents:
    echo - Bridge Service ^(Windows/Linux^)
    echo - vPilot Plugin ^(DLL^)
    echo - Mobile APK
    echo - Installation Scripts
    echo - Documentation
    echo.
    echo File size:
    for %%A in (%ZIP_NAME%) do echo %%~zA bytes ^(~%%~zA KB^)
    echo.
    echo Ready for distribution!
    echo.
) else (
    echo ERROR: Failed to create ZIP archive
)

pause
