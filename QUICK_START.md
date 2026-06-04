# VATSIM Companion App - 快速开始指南

这是一个 **5 步快速组装指南**，让你在 30 分钟内运行起来。

---

## 前置要求

在开始之前，确保已安装：

- ✅ **Windows 10/11** (vPilot 插件需要)
- ✅ **.NET 7.0 SDK** - [下载](https://dotnet.microsoft.com/download/dotnet/7.0)
- ✅ **Visual Studio 2022** (Community 免费版即可) - [下载](https://visualstudio.microsoft.com/)
- ✅ **Flutter SDK 3.0+** - [下载](https://flutter.dev/docs/get-started/install)
- ✅ **vPilot** - [下载](https://www.vpilot.net/)

---

## 🚀 5 步快速组装

### 第 1 步：运行自动设置脚本 (5 分钟)

```bash
# Windows
cd vatsim-companion-app
setup.bat

# macOS/Linux (仅 Bridge 和 App)
cd vatsim-companion-app
chmod +x setup.sh
./setup.sh
```

这会自动：
- ✅ 检查环境
- ✅ 恢复 NuGet 包
- ✅ 构建 Bridge 服务
- ✅ 恢复 Flutter 依赖

---

### 第 2 步：编译 vPilot 插件 (5 分钟)

#### 2.1 打开项目

```bash
# 双击打开
vpilot-plugin\VatsimCompanionPlugin.sln
```

或：
1. 打开 Visual Studio 2022
2. 文件 → 打开 → 项目/解决方案
3. 选择 `vpilot-plugin\VatsimCompanionPlugin.sln`

#### 2.2 检查 vPilot SDK 引用

右键 `VatsimCompanionPlugin` 项目 → 属性 → 引用

检查 `RossCarlson.Vatsim.Vpilot.Plugins` 路径是否正确：
```
C:\Program Files\vPilot\RossCarlson.Vatsim.Vpilot.Plugins.dll
```

如果路径不对：
1. 删除旧引用
2. 右键 "引用" → "添加引用" → "浏览"
3. 找到 vPilot 安装目录的 DLL
4. 添加引用

#### 2.3 构建

按 **F6** 或 菜单 → 生成 → 生成解决方案

编译成功后，DLL 会自动复制到：
```
C:\Program Files\vPilot\Plugins\VatsimCompanionPlugin.dll
```

---

### 第 3 步：配置 Bridge 服务 (5 分钟)

#### 3.1 生成安全密钥

打开 PowerShell：
```powershell
# 生成随机 JWT 密钥
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | % {[char]$_})
```

复制输出的随机字符串。

#### 3.2 编辑配置文件

打开：
```
bridge-service\windows\VatsimBridge\appsettings.json
```

修改：
```json
{
  "Jwt": {
    "SecretKey": "粘贴刚才生成的随机字符串",
    ...
  }
}
```

保存文件。

#### 3.3 运行 Bridge

```bash
cd bridge-service\windows\VatsimBridge
dotnet run
```

看到以下输出表示成功：
```
===========================================
VATSIM Companion Bridge
===========================================
Bridge URL: http://localhost:5000
===========================================
```

保持这个窗口运行！

---

### 第 4 步：配置 Flutter App (10 分钟)

#### 选项 A：跳过 Firebase（推荐快速测试）

App 已经处理了 Firebase 初始化失败，可以直接运行（没有推送通知）。

跳到 4.3 运行 App。

#### 选项 B：配置 Firebase（完整功能）

##### 4.1 创建 Firebase 项目

1. 访问 https://console.firebase.google.com/
2. 点击 "添加项目"
3. 输入项目名称，例如 "VATSIM Companion"
4. 按提示完成创建

##### 4.2 添加 Android 应用

1. 在 Firebase 控制台，点击 "添加应用" → Android 图标
2. Android 包名：`com.vatsim.companion`
3. 下载 `google-services.json`
4. 放置到：`mobile-app\android\app\google-services.json`

##### 4.3 添加 iOS 应用（如果需要）

1. 点击 "添加应用" → iOS 图标
2. iOS Bundle ID：`com.vatsim.companion`
3. 下载 `GoogleService-Info.plist`
4. 放置到：`mobile-app\ios\Runner\GoogleService-Info.plist`

##### 4.4 获取 FCM Server Key

1. Firebase 控制台 → 项目设置 → Cloud Messaging
2. 找到 "服务器密钥"
3. 复制密钥

##### 4.5 配置到 Bridge

编辑 `bridge-service\windows\VatsimBridge\appsettings.json`：
```json
{
  "PushNotification": {
    "FcmServerKey": "粘贴你的 FCM Server Key",
    "EnablePush": true
  }
}
```

重启 Bridge 服务。

#### 4.6 运行 Flutter App

```bash
cd mobile-app

# 检查设备
flutter devices

# 运行到 Android 模拟器
flutter run -d emulator-5554

# 或运行到连接的手机
flutter run -d <device-id>
```

---

### 第 5 步：配对和测试 (5 分钟)

#### 5.1 启动所有组件

确保以下都在运行：
1. ✅ vPilot 运行并连接到 VATSIM
2. ✅ vPilot 插件已加载（检查 vPilot 菜单）
3. ✅ Bridge 服务运行（控制台显示 Bridge URL）
4. ✅ Flutter App 运行（显示配对界面）

#### 5.2 生成配对码

打开新的命令行窗口：

```bash
# Windows PowerShell
Invoke-RestMethod -Method Post -Uri http://localhost:5000/api/pairing/start | ConvertTo-Json

# 或使用 curl
curl -X POST http://localhost:5000/api/pairing/start
```

你会看到输出：
```json
{
  "success": true,
  "pairingCode": "123456",
  "bridgeUrl": "http://localhost:5000",
  ...
}
```

记住这个 6 位数配对码！

#### 5.3 在 App 中配对

1. 在 Flutter App 的配对界面
2. 输入 Bridge 地址：
   - Android 模拟器：`10.0.2.2:5000`
   - iOS 模拟器：`localhost:5000`
   - 真实手机：`你电脑的局域网 IP:5000` (例如 `192.168.1.100:5000`)
3. 输入配对码：`123456`
4. 点击 "Connect"

#### 5.4 测试功能

配对成功后，App 会跳转到主界面。

**测试消息接收**：
- 在 vPilot 中接收私信或频率消息
- 应该会在 App 中实时显示

**测试消息发送**：
- 在 App 的聊天界面输入消息
- 点击发送
- 应该通过 vPilot 发送出去

**测试状态监控**：
- 切换到 "Status" 标签
- 查看飞机位置、高度、速度等信息
- 下拉刷新以更新

---

## 🐛 常见问题

### 问题 1: vPilot 插件未加载

**检查**：
1. DLL 文件是否在：`C:\Program Files\vPilot\Plugins\`
2. 右键 DLL → 属性 → 解除阻止（如果有）
3. 重启 vPilot

### 问题 2: Bridge 启动失败 - 端口被占用

**解决**：
```bash
# 修改端口
# 编辑 appsettings.json
"Port": "5001"  # 改为其他端口
```

### 问题 3: App 无法连接 Bridge

**检查网络**：

找到你的电脑 IP：
```bash
# Windows
ipconfig

# 看 "IPv4 地址"，例如 192.168.1.100
```

在 App 中使用：`192.168.1.100:5000`

**检查防火墙**：
```bash
# 允许端口 5000
netsh advfirewall firewall add rule name="VATSIM Bridge" dir=in action=allow protocol=TCP localport=5000
```

### 问题 4: Flutter 编译错误

```bash
cd mobile-app
flutter clean
flutter pub get
flutter run
```

---

## 📊 验证清单

完成后，验证以下功能：

- [ ] vPilot 插件加载成功
- [ ] Bridge 服务运行在 5000 端口
- [ ] Flutter App 编译并运行
- [ ] 成功生成配对码
- [ ] App 配对成功
- [ ] 能接收 vPilot 消息
- [ ] 能从 App 发送消息
- [ ] 能看到飞机状态信息
- [ ] (可选) 推送通知工作

---

## 🎉 成功！

如果所有功能都正常，恭喜你成功组装了 VATSIM Companion App！

### 下一步

- 阅读 [API.md](API.md) 了解完整 API
- 阅读 [ARCHITECTURE.md](ARCHITECTURE.md) 了解架构设计
- 查看 [READINESS_CHECKLIST.md](READINESS_CHECKLIST.md) 进行优化

### 获取帮助

- 查看日志文件：
  - 插件：`%APPDATA%\VatsimCompanion\Logs\`
  - Bridge：控制台输出
  - App：`flutter logs`

- GitHub Issues
- VATSIM 开发者论坛

---

**预计总时间**: 30-60 分钟  
**难度**: 中等  
**成功率**: 95%+ (如果按步骤操作)

祝你好运！✈️
