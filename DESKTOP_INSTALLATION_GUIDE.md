# VATSIM Companion - 桌面安装完整指南

## 📦 快速安装（推荐）

### 一键安装到桌面

```bash
双击运行: install-desktop.bat
```

**安装位置**: `C:\Users\你的用户名\Desktop\VatsimCompanion\`

**完成后**:
- ✅ 桌面会出现快捷方式
- ✅ Bridge 服务已编译
- ✅ 所有文件已复制

---

## 🎮 启动方式

### 方式 1: 桌面快捷方式
```
双击: "VATSIM Companion" 快捷方式
```

### 方式 2: GUI 启动器（推荐）
```
双击: Desktop\VatsimCompanion\launcher-gui.bat
```
**功能**:
- 启动/停止 Bridge
- 配置设置
- 查看日志
- 实时状态显示
- 显示连接信息（IP、端口）

### 方式 3: 命令行启动器
```
双击: Desktop\VatsimCompanion\launcher.bat
```
**菜单选项**:
```
[1] Start Bridge Service
[2] Stop Bridge Service  
[3] Configure Settings
[4] View Logs
[5] Exit
```

### 方式 4: 手动启动
```bash
cd Desktop\VatsimCompanion\bridge-service\windows\VatsimBridge
dotnet run
```

---

## 📱 移动应用

### 编译 APK

```bash
# 在项目根目录
双击: build-apk.bat
```

**编译时间**: 约 5-10 分钟

**输出位置**:
```
mobile-app\build\app\outputs\flutter-apk\app-release.apk
```

### 安装 APK

1. 通过 USB 或云盘传输 APK 到手机
2. 在手机上安装
3. 打开应用
4. 输入电脑 IP 地址（如 `192.168.1.100`）
5. 输入端口：`5000`
6. 输入配对码：`123456`
7. 点击 Connect

### 获取电脑 IP

**方法 1**: GUI 启动器会自动显示

**方法 2**: 命令行
```bash
ipconfig | findstr IPv4
```

---

## 🔧 配置文件

### Bridge 配置

**位置**: `Desktop\VatsimCompanion\bridge-service\windows\VatsimBridge\appsettings.json`

```json
{
  "Bridge": {
    "Port": 5000,
    "PairingCode": "123456",
    "JwtSecret": "your-random-secret-key-here",
    "CorsOrigins": ["*"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

**修改配置**:
- GUI 启动器: 点击 "Configure" 按钮
- 手动: 用记事本打开编辑

---

## 🔌 vPilot 插件

### 插件位置

**编译后**: `vpilot-plugin\VatsimCompanionPlugin\bin\Release\VatsimCompanionPlugin.dll`

**安装位置**: `C:\Users\你的用户名\Documents\vPilot Files\Plugins\`

### 手动安装

1. 找到编译的 DLL
2. 复制到 vPilot Plugins 目录
3. 重启 vPilot
4. 在 vPilot 插件管理中启用

### 验证安装

1. 启动 vPilot
2. 查看插件列表
3. 应该看到 "VATSIM Companion Plugin"

---

## 🌐 防火墙设置

### Windows 防火墙

**自动规则**（首次启动时 Windows 会询问）:
- 允许 VatsimBridge.exe 访问网络
- 允许端口 5000

**手动添加规则**:
```
1. Windows 安全中心
2. 防火墙和网络保护
3. 高级设置
4. 入站规则 → 新建规则
5. 端口 → TCP → 5000
6. 允许连接
```

---

## 📊 完整使用流程

### 1. 安装阶段
```
✅ 运行 install-desktop.bat
✅ 等待安装完成
✅ 查看桌面快捷方式
```

### 2. 首次配置
```
✅ 打开 launcher-gui.bat
✅ 点击 Configure 修改配对码（可选）
✅ 记下显示的 IP 地址
```

### 3. 启动服务
```
✅ 点击 "Start" 按钮
✅ 等待状态变为 "Bridge Running"
✅ 确认端口 5000 可访问
```

### 4. 安装移动应用
```
✅ 运行 build-apk.bat 编译 APK
✅ 传输到手机并安装
✅ 打开应用
```

### 5. 配对连接
```
✅ 在手机 App 输入电脑 IP
✅ 输入端口 5000
✅ 输入配对码
✅ 点击 Connect
✅ 等待连接成功
```

### 6. 使用 vPilot
```
✅ 启动 vPilot
✅ 连接 VATSIM
✅ 手机 App 自动接收消息
✅ 查看飞机状态
```

---

## 🆘 故障排查

### Bridge 启动失败

**症状**: 点击 Start 无反应

**解决**:
1. 检查 .NET SDK 是否安装
   ```bash
   dotnet --version
   ```
2. 查看日志文件
3. 确认端口 5000 未被占用
   ```bash
   netstat -ano | findstr :5000
   ```

### 手机连接不上

**症状**: App 显示 "Connection failed"

**解决**:
1. 确认手机和电脑在同一 WiFi
2. 检查防火墙是否阻止
3. 使用浏览器测试: `http://电脑IP:5000`
4. 尝试关闭 Windows 防火墙测试
5. 确认 Bridge 正在运行

### 配对码错误

**症状**: "Invalid pairing code"

**解决**:
1. 打开 appsettings.json
2. 查看 `PairingCode` 值
3. 在手机 App 输入相同的值

### vPilot 插件不工作

**症状**: 手机收不到消息

**解决**:
1. 确认插件 DLL 在正确位置
2. 重启 vPilot
3. 查看 vPilot 日志
4. 确认 Bridge 正在运行

---

## 📋 文件清单

### 安装后的目录结构

```
Desktop\VatsimCompanion\
├── bridge-service\
│   └── windows\VatsimBridge\
│       ├── bin\Release\net7.0\
│       │   └── VatsimBridge.exe      ← 主程序
│       ├── appsettings.json          ← 配置文件
│       └── logs\                     ← 日志目录
├── vpilot-plugin\
│   └── VatsimCompanionPlugin\
│       └── bin\Release\
│           └── VatsimCompanionPlugin.dll
├── start.bat                         ← 快速启动
├── launcher.bat                      ← 命令行启动器
├── launcher-gui.bat                  ← GUI 启动器
├── launcher-gui.ps1                  ← GUI 脚本
└── README.md                         ← 说明文档
```

---

## 🚀 高级功能

### Cloudflare 隧道（外网访问）

如果需要外网访问（不在同一局域网）:

```bash
# 下载 cloudflared.exe
# 放在 Desktop\VatsimCompanion\ 目录

# 启动隧道
cloudflared tunnel --url http://localhost:5000

# 使用显示的 URL（如 https://xxx.trycloudflare.com）
# 在手机 App 中连接
```

### 自动启动

创建 Windows 任务计划:
```
1. Win + R → taskschd.msc
2. 创建基本任务
3. 触发器: 登录时
4. 操作: 启动程序
5. 程序: Desktop\VatsimCompanion\start.bat
```

### 多设备连接

Bridge 支持多个设备同时连接:
- 每个设备使用相同的 IP 和配对码
- 所有设备都会收到消息推送
- 适合家人/朋友共同使用

---

## 📞 获取帮助

### 文档
- `DESKTOP_QUICK_START.md` - 本文档
- `README.md` - 完整项目说明
- `QUICK_START.md` - 快速开始指南

### 日志
- Bridge 日志: `Desktop\VatsimCompanion\bridge-service\windows\VatsimBridge\logs\`
- vPilot 日志: `Documents\vPilot Files\Logs\`

### 支持
- GitHub Issues
- 项目文档

---

## ✅ 快速检查清单

启动前检查:
- [ ] .NET SDK 已安装
- [ ] Bridge 服务已编译
- [ ] 配置文件已修改（可选）
- [ ] 防火墙已配置

移动应用检查:
- [ ] APK 已编译
- [ ] APK 已安装到手机
- [ ] 手机和电脑在同一 WiFi
- [ ] 已获取电脑 IP 地址

vPilot 检查:
- [ ] 插件 DLL 已复制
- [ ] vPilot 已重启
- [ ] 插件已启用

连接检查:
- [ ] Bridge 正在运行
- [ ] 手机 App 显示已连接
- [ ] 能收到测试消息

---

## 🎯 快速开始（3 分钟）

```bash
# 1. 安装（1 分钟）
双击: install-desktop.bat

# 2. 启动（30 秒）
双击桌面快捷方式

# 3. 连接（1 分钟）
手机 App → 输入 IP → Connect

# 完成！
```

现在你可以享受 VATSIM Companion 的便利了！✈️
