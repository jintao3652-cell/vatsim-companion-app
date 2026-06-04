#!/bin/bash

echo "============================================"
echo "VATSIM Companion App - Setup Script"
echo "============================================"
echo ""

# Check prerequisites
echo "[1/5] Checking prerequisites..."

if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not found. Please install .NET 7.0 SDK first."
    echo "Download from: https://dotnet.microsoft.com/download"
    exit 1
fi

if ! command -v flutter &> /dev/null; then
    echo "WARNING: Flutter not found. You'll need it for the mobile app."
    echo "Download from: https://flutter.dev/docs/get-started/install"
fi

echo ".NET SDK: OK"
echo ""

# Setup Bridge Service
echo "[2/5] Setting up Bridge Service..."
cd bridge-service/windows/VatsimBridge || cd bridge-service/windows

echo "Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to restore Bridge packages"
    exit 1
fi

echo "Building Bridge Service..."
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to build Bridge Service"
    exit 1
fi

cd ../../..
echo "Bridge Service: OK"
echo ""

# Setup vPilot Plugin
echo "[3/5] Setting up vPilot Plugin..."
cd vpilot-plugin

echo "NOTE: The vPilot plugin requires Visual Studio (Windows) to build."
echo "On non-Windows systems, you can only restore packages."
echo ""

cd VatsimCompanionPlugin
dotnet restore
cd ../..

echo "vPilot Plugin: Packages restored"
echo ""

# Setup Flutter App
echo "[4/5] Setting up Flutter App..."
cd mobile-app

if [ -f "pubspec.yaml" ]; then
    if command -v flutter &> /dev/null; then
        echo "Getting Flutter packages..."
        flutter pub get
        if [ $? -eq 0 ]; then
            echo "Flutter App: OK"
        else
            echo "WARNING: Flutter pub get failed"
        fi
    else
        echo "SKIPPED: Flutter not installed"
    fi
else
    echo "ERROR: pubspec.yaml not found"
fi

cd ..
echo ""

# Generate configuration template
echo "[5/5] Generating configuration files..."

cat > .env.example << 'EOF'
# VATSIM Companion Configuration

# Bridge Service
BRIDGE_PORT=5000
JWT_SECRET=CHANGE_THIS_TO_A_RANDOM_32_CHAR_STRING

# Firebase (Optional - for push notifications)
FCM_SERVER_KEY=YOUR_FCM_SERVER_KEY_HERE

# vPilot Plugin
PLUGIN_PORT=8765
EOF

echo ""
echo "============================================"
echo "Setup Complete!"
echo "============================================"
echo ""
echo "NEXT STEPS:"
echo ""
echo "1. vPilot Plugin (Windows only):"
echo "   - Open vpilot-plugin/VatsimCompanionPlugin.sln in Visual Studio"
echo "   - Check if vPilot SDK reference path is correct"
echo "   - Build the solution"
echo "   - The DLL will auto-copy to vPilot Plugins folder"
echo ""
echo "2. Bridge Service:"
echo "   - Edit bridge-service/windows/VatsimBridge/appsettings.json"
echo "   - Change JWT SecretKey to a random string"
echo "   - Run: cd bridge-service/windows/VatsimBridge"
echo "   - Run: dotnet run"
echo ""
echo "3. Flutter App (Optional - if no Firebase):"
echo "   - Firebase initialization is wrapped in try-catch"
echo "   - App will work without push notifications"
echo ""
echo "4. Firebase Setup (for push notifications):"
echo "   - Create Firebase project at console.firebase.google.com"
echo "   - Add Android app, download google-services.json"
echo "   - Place in mobile-app/android/app/"
echo "   - Add iOS app, download GoogleService-Info.plist"
echo "   - Place in mobile-app/ios/Runner/"
echo ""
echo "See READINESS_CHECKLIST.md for detailed instructions"
echo ""
