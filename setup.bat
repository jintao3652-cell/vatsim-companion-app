@echo off
echo ============================================
echo VATSIM Companion App - Setup Script
echo ============================================
echo.

REM Check prerequisites
echo [1/5] Checking prerequisites...
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET SDK not found. Please install .NET 7.0 SDK first.
    echo Download from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

where flutter >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo WARNING: Flutter not found. You'll need it for the mobile app.
    echo Download from: https://flutter.dev/docs/get-started/install
)

echo .NET SDK: OK
echo.

REM Setup Bridge Service
echo [2/5] Setting up Bridge Service...
cd bridge-service\windows\VatsimBridge
if not exist "VatsimBridge.sln" (
    cd ..
)

echo Restoring NuGet packages...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to restore Bridge packages
    pause
    exit /b 1
)

echo Building Bridge Service...
dotnet build --configuration Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to build Bridge Service
    pause
    exit /b 1
)

cd ..\..\..
echo Bridge Service: OK
echo.

REM Setup vPilot Plugin
echo [3/5] Setting up vPilot Plugin...
cd vpilot-plugin

echo NOTE: The vPilot plugin requires Visual Studio to build.
echo This script will restore packages, but you need to build it in Visual Studio.
echo.

cd VatsimCompanionPlugin
dotnet restore
cd ..\..

echo vPilot Plugin: Packages restored (build in Visual Studio)
echo.

REM Setup Flutter App
echo [4/5] Setting up Flutter App...
cd mobile-app

if exist "pubspec.yaml" (
    where flutter >nul 2>nul
    if %ERRORLEVEL% EQU 0 (
        echo Getting Flutter packages...
        flutter pub get
        if %ERRORLEVEL% EQU 0 (
            echo Flutter App: OK
        ) else (
            echo WARNING: Flutter pub get failed
        )
    ) else (
        echo SKIPPED: Flutter not installed
    )
) else (
    echo ERROR: pubspec.yaml not found
)

cd ..
echo.

REM Generate configuration template
echo [5/5] Generating configuration files...

echo Creating .env.example...
(
echo # VATSIM Companion Configuration
echo.
echo # Bridge Service
echo BRIDGE_PORT=5000
echo JWT_SECRET=CHANGE_THIS_TO_A_RANDOM_32_CHAR_STRING
echo.
echo # Firebase (Optional - for push notifications^)
echo FCM_SERVER_KEY=YOUR_FCM_SERVER_KEY_HERE
echo.
echo # vPilot Plugin
echo PLUGIN_PORT=8765
) > .env.example

echo.
echo ============================================
echo Setup Complete!
echo ============================================
echo.
echo NEXT STEPS:
echo.
echo 1. vPilot Plugin:
echo    - Open vpilot-plugin\VatsimCompanionPlugin.sln in Visual Studio
echo    - Check if vPilot SDK reference path is correct
echo    - Build the solution (F6^)
echo    - The DLL will auto-copy to vPilot Plugins folder
echo.
echo 2. Bridge Service:
echo    - Edit bridge-service\windows\VatsimBridge\appsettings.json
echo    - Change JWT SecretKey to a random string
echo    - Run: cd bridge-service\windows\VatsimBridge
echo    - Run: dotnet run
echo.
echo 3. Flutter App (Optional - if no Firebase^):
echo    - Comment out Firebase in lib\main.dart
echo    - Or setup Firebase project and add config files
echo.
echo 4. Firebase Setup (for push notifications^):
echo    - Create Firebase project at console.firebase.google.com
echo    - Add Android app, download google-services.json
echo    - Place in mobile-app\android\app\
echo    - Add iOS app, download GoogleService-Info.plist
echo    - Place in mobile-app\ios\Runner\
echo.
echo See READINESS_CHECKLIST.md for detailed instructions
echo.
pause
