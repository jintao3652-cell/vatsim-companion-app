# VATSIM Companion App

Mobile companion app for VATSIM pilots using vPilot on Windows. Enables real-time text messaging and aircraft monitoring from your phone while flying.

## 🚀 Features

- 📱 **Mobile App** (Android/iOS) - Stay connected while flying
- 💬 **Private Messaging** - Text chat with ATC and other pilots
- 📻 **Radio Messages** - View radio communications
- ✈️ **Aircraft Status** - Monitor your flight data in real-time
- 🎮 **Controller List** - See nearby ATC controllers with frequencies
- 🔔 **Push Notifications** - Never miss important messages
- 🔒 **Secure Pairing** - QR code or manual pairing with JWT authentication
- 🌐 **Cloudflare Tunnel** - Access from anywhere via HTTPS

## 📋 Requirements

### Bridge Service (Windows PC)
- Windows 10/11
- .NET 8.0 SDK or Runtime
- vPilot installed and running
- vPilot Plugin API enabled

### Mobile App
- Android 6.0+ or iOS 12.0+
- Internet connection (Wi-Fi or mobile data)

## 🛠️ Quick Start

### 1️⃣ Install Bridge Service

**Desktop Installation (Recommended)**:
```cmd
install-desktop.bat
```
Creates shortcut on Desktop → Double-click to launch GUI

**Manual Installation**:
```cmd
cd bridge-service\windows
dotnet run
```

### 2️⃣ Build Mobile App

**Android APK**:
```cmd
build-apk.bat
```
Output: `mobile-app\build\app\outputs\flutter-apk\app-release.apk`

Install on phone:
- Transfer APK to Android phone
- Enable "Install from Unknown Sources"
- Install APK

**iOS** (Requires Mac):
```bash
cd mobile-app/ios
pod install
open Runner.xcworkspace
# Select device → Run in Xcode
```

📖 **Detailed iOS Guide**: See [IOS_SETUP.md](IOS_SETUP.md) for certificates, signing, App Store submission  
⚡ **Quick iOS Start**: See [IOS_QUICK_START.md](IOS_QUICK_START.md) for 5-minute setup

### 3️⃣ Pairing

1. Start vPilot and connect to VATSIM
2. Launch Bridge Service (runs on http://localhost:5000)
3. Open mobile app → Generate pairing code
4. Enter code on Bridge Service web UI (http://localhost:5000)
5. Connected! ✅

### 4️⃣ External Access (Optional)

**Use Cloudflare Tunnel for external access**:
```cmd
cloudflared tunnel --url http://localhost:5000
```
Copy the `https://*.trycloudflare.com` URL to mobile app

## 📁 Project Structure

```
vatsim-companion-app/
├── bridge-service/
│   └── windows/              # .NET 8.0 Bridge Service
│       ├── VatsimBridge.csproj
│       ├── Program.cs
│       ├── Controllers/      # REST API endpoints
│       ├── Hubs/            # SignalR hub for real-time
│       └── Services/        # vPilot plugin communication
├── mobile-app/              # Flutter mobile app
│   ├── lib/
│   │   ├── screens/        # UI screens
│   │   ├── services/       # API & WebSocket services
│   │   ├── providers/      # State management (Riverpod)
│   │   └── models/         # Data models
│   ├── android/
│   └── ios/
├── vpilot-plugin/          # C++ vPilot plugin
│   └── VatsimCompanionPlugin/
├── installer/              # Desktop installer files
├── build-apk.bat          # Build Android APK
├── install-desktop.bat    # Install to Desktop
├── launcher-gui.bat       # GUI launcher
├── launcher.bat           # CLI launcher
└── README.md
```

## 🔧 Configuration

### Bridge Service
`bridge-service/windows/appsettings.json`:
```json
{
  "Port": "5000",
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-chars-long",
    "ExpiryMinutes": 43200
  },
  "Pairing": {
    "CodeLength": 6,
    "ExpiryMinutes": 10
  }
}
```

### vPilot Plugin
- Place `VatsimCompanionPlugin.dll` in vPilot's `Plugins` folder
- Enable plugin in vPilot settings

## 🌐 API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/status` | GET | Bridge & vPilot status |
| `/api/status/aircraft` | GET | Current aircraft state |
| `/api/pairing/start` | POST | Generate pairing code |
| `/api/pairing/complete` | POST | Complete pairing with code |
| `/api/messages` | GET | Recent messages |
| `/vatsimhub` | WebSocket | SignalR hub for real-time |

## 📱 Mobile App Features

### Screens
- **Pairing** - Connect to Bridge Service
- **Home** - Quick overview & recent messages
- **Chat** - Private & radio messages
- **Status** - Aircraft state & nearby controllers
- **Settings** - Connection management

### State Management
- Uses Riverpod for reactive state
- Automatic reconnection on connection loss
- Message persistence with local storage
- Foreground service for background connectivity

## 🔒 Security

- JWT authentication for mobile → bridge
- Short-lived pairing codes (10 min expiry)
- HTTPS support via Cloudflare Tunnel
- No sensitive data stored in app

## 🐛 Troubleshooting

**Bridge won't start**:
- Check vPilot is running
- Verify .NET 8.0 is installed: `dotnet --version`
- Check port 5000 is available

**Mobile app can't connect**:
- Verify bridge is running (check http://localhost:5000)
- Check firewall allows port 5000
- Use Cloudflare Tunnel for external access

**No messages received**:
- Verify vPilot plugin is loaded
- Check vPilot connected to VATSIM network
- Verify callsign matches in app and vPilot

## 📦 Building from Source

### Bridge Service
```cmd
cd bridge-service\windows
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained
```

### Mobile App
```cmd
cd mobile-app
flutter pub get
flutter build apk --release
```

### vPilot Plugin
Open `vpilot-plugin/VatsimCompanionPlugin.sln` in Visual Studio 2022+
Build → Release → x64

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing`)
5. Open Pull Request

## 📄 License

MIT License - See LICENSE file for details

## 🔗 Links

- vPilot: https://www.vatsim.net/pilots/software
- VATSIM: https://www.vatsim.net
- Cloudflare Tunnel: https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/

## 💡 Tips

- Keep Bridge Service running while flying
- Use Cloudflare Tunnel for stable external access
- Enable notifications for important messages
- Check controller list for nearby ATC frequencies

---

Made for VATSIM pilots ✈️ by the community
