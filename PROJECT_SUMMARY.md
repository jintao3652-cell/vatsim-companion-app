# VATSIM Companion App - 项目总结

## 项目概述

这是一个完整的 VATSIM 伴侣 App 项目，支持 Android/iOS 平台，用于与 vPilot/xPilot 进行双向通信。项目采用三层架构：

1. **移动端 App (Flutter)** - 用户界面和交互
2. **Bridge 服务 (.NET)** - 运行在用户电脑上的中转服务
3. **vPilot 插件 (C#)** - 与 vPilot 集成的插件

## 已完成的核心功能

### 1. vPilot 插件 ✅
- ✅ 实现 IPlugin 接口
- ✅ 监听私信、频率消息、连接状态等事件
- ✅ 提供本地 HTTP 服务器供 Bridge 调用
- ✅ 支持发送消息和执行 dot commands
- ✅ 自动复制到 vPilot Plugins 目录

**位置**: `vpilot-plugin/VatsimCompanionPlugin/`

**关键文件**:
- `Plugin.cs` - 主插件逻辑
- `VatsimCompanionPlugin.csproj` - 项目配置

**端口**: 8765 (HTTP)

### 2. Bridge 服务 ✅
- ✅ ASP.NET Core Web API + SignalR
- ✅ 与 vPilot 插件通信 (HTTP)
- ✅ 与移动端通信 (WebSocket/REST)
- ✅ 推送通知支持 (FCM)
- ✅ 健康检查和状态端点

**位置**: `bridge-service/windows/VatsimBridge/`

**关键文件**:
- `Program.cs` - 应用入口
- `Hubs/VatsimHub.cs` - SignalR Hub
- `Services/PluginCommunicationService.cs` - 插件通信
- `Services/PushNotificationService.cs` - 推送通知
- `Controllers/` - REST API 端点
- `Models/Dtos.cs` - 数据模型

**端口**: 5000 (HTTP/WebSocket)

**主要端点**:
- WebSocket: `ws://localhost:5000/vatsimhub`
- REST API: `http://localhost:5000/api/`
- Status: `http://localhost:5000/api/status`

### 3. Flutter Mobile App ✅
- ✅ Material Design 3 UI
- ✅ 配对界面
- ✅ 聊天界面 (私信 + 频率消息)
- ✅ 飞机状态显示
- ✅ 快捷指令
- ✅ 推送通知集成

**位置**: `mobile-app/`

**关键文件**:
- `lib/main.dart` - 应用入口
- `lib/services/` - 网络和推送服务
- `lib/models/` - 数据模型
- `lib/screens/` - 界面页面
- `lib/config/theme.dart` - 主题配置

**支持平台**:
- Android 8.0+ (API 26+)
- iOS 13.0+

## 项目结构

```
vatsim-companion-app/
├── README.md                     # 项目说明
├── docs/
│   ├── ARCHITECTURE.md           # 架构设计文档
│   ├── API.md                    # API 文档
│   └── SETUP.md                  # 开发环境设置指南
│
├── vpilot-plugin/                # vPilot 插件
│   └── VatsimCompanionPlugin/
│       ├── Plugin.cs             # 主插件代码
│       └── VatsimCompanionPlugin.csproj
│
├── bridge-service/               # Bridge 服务
│   └── windows/
│       └── VatsimBridge/
│           ├── Program.cs        # 主程序
│           ├── Hubs/             # SignalR Hubs
│           ├── Services/         # 业务服务
│           ├── Controllers/      # API 控制器
│           ├── Models/           # 数据模型
│           └── appsettings.json  # 配置文件
│
└── mobile-app/                   # Flutter App
    ├── pubspec.yaml              # 依赖配置
    ├── lib/
    │   ├── main.dart             # 应用入口
    │   ├── config/               # 配置
    │   ├── models/               # 数据模型
    │   ├── services/             # 服务层
    │   └── screens/              # 界面
    ├── android/                  # Android 配置
    └── ios/                      # iOS 配置
```

## 核心技术栈

| 组件 | 技术栈 |
|------|--------|
| **vPilot 插件** | C#, .NET Framework 4.8, vPilot Plugin SDK |
| **Bridge 服务** | C#, .NET 7, ASP.NET Core, SignalR |
| **移动端 App** | Dart, Flutter 3.x, Riverpod |
| **推送通知** | Firebase Cloud Messaging (FCM) |
| **实时通信** | SignalR (WebSocket) |
| **REST API** | ASP.NET Core Web API |

## 通信流程

```
┌─────────────┐    WebSocket     ┌─────────────┐     HTTP      ┌─────────────┐
│ Mobile App  │ ←─────────────→ │   Bridge    │ ←──────────→ │   Plugin    │
│  (Flutter)  │    REST API      │  (.NET 7)   │              │ (vPilot API)│
└─────────────┘                  └─────────────┘              └─────────────┘
       │                                │                             │
       │                                │                             │
       ▼                                ▼                             ▼
  推送通知                         中转/推送                      vPilot 事件
```

## 快速开始

### 1. 开发 vPilot 插件

```bash
# 使用 Visual Studio 2022
cd vpilot-plugin
# 打开 VatsimCompanionPlugin.sln
# 构建 → 自动复制到 vPilot Plugins 目录
```

### 2. 运行 Bridge 服务

```bash
cd bridge-service/windows/VatsimBridge
dotnet restore
dotnet run

# 访问 http://localhost:5000
```

### 3. 运行 Flutter App

```bash
cd mobile-app
flutter pub get
flutter run

# 或者指定设备
flutter run -d <device-id>
```

### 4. 配对流程

1. 启动 vPilot 并连接到 VATSIM
2. 启动 Bridge 服务 (会显示配对信息)
3. 在 App 中输入 Bridge 地址和配对码
4. 配对成功后即可使用

## 功能清单

### 已实现 ✅

#### vPilot 插件
- [x] 监听私信事件
- [x] 监听频率消息事件
- [x] 监听连接状态变化
- [x] 监听频率变化
- [x] 监听应答机变化
- [x] 发送私信
- [x] 发送频率消息
- [x] 执行 dot commands
- [x] 获取飞机状态
- [x] HTTP 服务器

#### Bridge 服务
- [x] SignalR WebSocket Hub
- [x] REST API 端点
- [x] 与插件 HTTP 通信
- [x] 推送通知服务
- [x] 状态查询
- [x] 健康检查
- [x] 事件转发
- [x] CORS 支持

#### Mobile App
- [x] 配对界面
- [x] 主页导航
- [x] 聊天界面 (双 Tab)
- [x] 消息气泡显示
- [x] 快捷指令按钮
- [x] 飞机状态页面
- [x] WebSocket 连接
- [x] REST API 调用
- [x] 推送通知集成
- [x] Material Design 3 主题
- [x] 深色模式支持

### 待完成 ⏳

#### Phase 2 功能
- [ ] QR 码扫描配对
- [ ] mDNS 自动发现
- [ ] JWT 认证
- [ ] 消息历史存储
- [ ] 飞行计划显示
- [ ] 在线 ATC 列表
- [ ] 附近飞机列表
- [ ] 自定义快捷指令
- [ ] 语音转文字
- [ ] 地图视图

#### Phase 3 功能
- [ ] xPilot 支持
- [ ] 离线模式
- [ ] 消息搜索
- [ ] 导出日志
- [ ] 多设备同步
- [ ] 主题自定义
- [ ] 通知过滤设置
- [ ] 性能优化
- [ ] 单元测试
- [ ] 集成测试

## 配置说明

### Bridge 服务配置 (appsettings.json)

```json
{
  "Port": "5000",
  "PushNotification": {
    "FcmServerKey": "YOUR_FCM_SERVER_KEY_HERE"
  }
}
```

### Firebase 配置

1. 创建 Firebase 项目
2. 添加 Android/iOS 应用
3. 下载配置文件:
   - Android: `google-services.json` → `android/app/`
   - iOS: `GoogleService-Info.plist` → `ios/Runner/`
4. 获取 FCM Server Key 并配置到 Bridge

### vPilot 插件安装

1. 编译插件生成 `VatsimCompanionPlugin.dll`
2. 复制到 `C:\Program Files\vPilot\Plugins\`
3. 重启 vPilot
4. 检查插件是否加载: vPilot → Plugins 菜单

## 调试技巧

### 查看 Bridge 日志
```bash
cd bridge-service/windows/VatsimBridge
dotnet run --Logging:LogLevel:Default=Debug
```

### 查看 vPilot 日志
```
C:\Users\<YourName>\AppData\Local\vPilot\Logs\
```

### Flutter 调试
```bash
flutter run --verbose
flutter logs
```

### 网络抓包
- 使用 Fiddler/Wireshark 抓取 HTTP/WebSocket 流量
- Android 模拟器使用 `10.0.2.2` 代替 `localhost`

## 已知问题

1. **配对功能未完整实现** - 需要实现 JWT Token 生成和验证
2. **推送通知需要 FCM 配置** - 需要创建 Firebase 项目
3. **消息历史未持久化** - 需要添加本地数据库 (Hive)
4. **xPilot 支持未实现** - 仅支持 vPilot

## 下一步计划

### 短期 (1-2 周)
1. 完善配对功能 (JWT 认证)
2. 实现消息历史存储 (Hive)
3. 添加 mDNS 服务发现
4. 完善错误处理和重连逻辑
5. 添加加载状态指示器

### 中期 (1 个月)
1. 实现飞行计划显示
2. 添加在线 ATC 列表
3. 实现附近飞机显示
4. 自定义快捷指令管理
5. 添加设置页面

### 长期 (2-3 个月)
1. xPilot 支持
2. 地图集成
3. 语音转文字
4. 多设备同步
5. 应用商店上架准备

## 贡献指南

欢迎贡献代码！请遵循以下步骤：

1. Fork 本仓库
2. 创建功能分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m 'Add amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 创建 Pull Request

## 开源协议

MIT License

## 相关资源

- [vPilot 官网](https://www.vpilot.net/)
- [xPilot GitHub](https://github.com/xpilot-project)
- [VATSIM 开发者论坛](https://forums.vatsim.net/forum/134-developer-forum/)
- [Flutter 文档](https://flutter.dev/)
- [SignalR 文档](https://docs.microsoft.com/aspnet/core/signalr)
- [Firebase 文档](https://firebase.google.com/docs)

## 联系方式

- GitHub Issues: 报告 Bug 和功能请求
- GitHub Discussions: 技术讨论
- VATSIM Discord: 实时交流

---

**项目状态**: 🚧 开发中 (MVP 阶段)

**最后更新**: 2026-06-02
