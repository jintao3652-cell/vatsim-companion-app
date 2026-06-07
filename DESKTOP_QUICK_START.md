# VATSIM Companion - 桌面版快速指南

## 🚀 快速安装

### 方法 1: 自动安装（推荐）

1. 双击运行 `install-desktop.bat`
2. 等待安装完成（约 1-2 分钟）
3. 在桌面找到 "VATSIM Companion" 快捷方式

### 方法 2: 手动安装

1. 复制整个项目文件夹到桌面
2. 重命名为 `VatsimCompanion`
3. 运行 `setup.bat`

## 📁 安装位置

```
C:\Users\你的用户名\Desktop\VatsimCompanion\
├── bridge-service\          # Bridge 服务
├── vpilot-plugin\           # vPilot 插件
├── start.bat                # 快速启动
├── launcher.bat             # 启动器（菜单）
└── README.md
```

## 🎮 使用方法

### 启动 Bridge 服务

**方法 A: 快速启动**
```
双击桌面快捷方式 "VATSIM Companion"
```

**方法 B: 使用启动器**
```
1. 双击 launcher.bat
2. 选择 [1] Start Bridge Service
```

**方法 C: 手动启动**
```
cd Desktop\VatsimCompanion\bridge-service\windows\VatsimBridge
dotnet run
```

### 停止服务

- 关闭 Bridge 窗口
- 或使用启动器选择 [2] Stop Bridge Service

### 查看配置

- 使用启动器选择 [3] Configure Settings
- 或直接编辑 `bridge-service\windows\VatsimBridge\appsettings.json`

## 📱 连接移动应用

### 步骤 1: 获取 APK

APK 文件位置：
```
vatsim-companion-app\mobile-app\build\app\outputs\flutter-apk\app-release.apk
```

### 步骤 2: 安装到手机

- 通过 USB 传输到手机
- 或使用云盘/邮箱发送
- 在手机上安装 APK

### 步骤 3: 配对连接

1. 启动 Bridge 服务
2. 打开手机 App
3. 输入电脑 IP 地址和端口
4. 输入配对码（默认：`123456`）
5. 点击 Connect

## 🔧 配置说明

### appsettings.json

```json
{
  "Bridge": {
    "Port": 5000,              // Bridge 端口
    "PairingCode": "123456",   // 配对码
    "JwtSecret": "your-secret" // JWT 密钥（建议修改）
  }
}
```

### 获取本机 IP 地址

```
1. Win + R
2. 输入 cmd
3. 输入 ipconfig
4. 查找 "IPv4 地址"（通常是 192.168.x.x）
```

## 🔌 vPilot 插件安装

### 自动安装（编译后）

插件会自动复制到：
```
C:\Users\你的用户名\Documents\vPilot Files\Plugins\
```

### 手动安装

1. 找到编译后的 DLL：
   ```
   vpilot-plugin\VatsimCompanionPlugin\bin\Release\VatsimCompanionPlugin.dll
   ```

2. 复制到 vPilot 插件目录：
   ```
   C:\Users\你的用户名\Documents\vPilot Files\Plugins\
   ```

3. 重启 vPilot

## 🧪 测试连接

### 本地测试

1. 启动 Bridge
2. 浏览器访问 `http://localhost:5000`
3. 应该看到 API 响应

### 局域网测试

1. 获取本机 IP（如 192.168.1.100）
2. 手机连接同一 WiFi
3. 手机浏览器访问 `http://192.168.1.100:5000`
4. 应该能访问说明网络通畅

## 📊 启动器功能

```
[1] Start Bridge Service  - 启动 Bridge 服务
[2] Stop Bridge Service   - 停止 Bridge 服务
[3] Configure Settings    - 编辑配置文件
[4] View Logs            - 查看日志目录
[5] Exit                 - 退出
```

## 🆘 常见问题

### Q: 双击快捷方式没反应？
A: 检查安装目录是否正确：`Desktop\VatsimCompanion`

### Q: Bridge 启动失败？
A: 
1. 检查 .NET 是否安装（运行 `dotnet --version`）
2. 检查端口 5000 是否被占用
3. 查看错误日志

### Q: 手机连接不上？
A:
1. 确保手机和电脑在同一 WiFi
2. 检查防火墙是否阻止端口 5000
3. 尝试关闭 Windows 防火墙测试

### Q: vPilot 插件不工作？
A:
1. 检查 DLL 是否在正确位置
2. 重启 vPilot
3. 查看 vPilot 日志

### Q: 配对码错误？
A: 编辑 `appsettings.json`，修改 `PairingCode` 值

## 🔄 更新

### 更新 Bridge 服务

```bash
cd Desktop\VatsimCompanion
git pull
cd bridge-service\windows\VatsimBridge
dotnet build --configuration Release
```

### 更新移动应用

1. 下载新版 APK
2. 直接安装覆盖旧版

## 📝 日志位置

```
bridge-service\windows\VatsimBridge\logs\
```

## 🌐 Cloudflare 隧道（可选）

如果需要外网访问：

```bash
# 下载 cloudflared
# 启动隧道
cloudflared tunnel --url http://localhost:5000

# 使用返回的 URL 在手机 App 中连接
```

## 🎯 完整流程

1. ✅ 运行 `install-desktop.bat` 安装到桌面
2. ✅ 双击桌面快捷方式启动 Bridge
3. ✅ 复制 APK 到手机并安装
4. ✅ 手机 App 输入电脑 IP 和配对码
5. ✅ 启动 vPilot 和 VATSIM 连接
6. ✅ 手机 App 自动接收消息和状态

## 📚 更多帮助

- 详细文档：`README.md`
- 开发指南：`CONTRIBUTING.md`
- 问题反馈：项目 Issues
