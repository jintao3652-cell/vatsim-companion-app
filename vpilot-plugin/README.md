# VATSIM Companion Plugin for vPilot

这是 VATSIM Companion App 的 vPilot 插件部分，负责监听 vPilot 事件并提供 HTTP API 供 Bridge 服务调用。

## 功能特性

✅ **事件监听**
- 私信接收
- 频率消息接收（含 callsign 提及检测）
- 网络连接/断开
- 频率变化
- 应答机变化
- SELCAL 接收

✅ **HTTP API**
- 获取插件状态
- 发送消息（私信/频率）
- 执行 dot commands
- 获取飞机状态

✅ **日志系统**
- 自动记录到文件
- 日志文件自动清理
- 支持调试模式

✅ **错误处理**
- 完整的异常捕获
- 自动重试机制
- 统计信息跟踪

## 文件结构

```
VatsimCompanionPlugin/
├── Plugin.cs                          # 主插件类
├── Models.cs                          # 数据模型
├── Utils.cs                           # 工具类（日志、重试）
├── BridgeCommunicationService.cs     # Bridge 通信服务
└── VatsimCompanionPlugin.csproj      # 项目配置
```

## 编译和安装

### 前置要求
- Visual Studio 2022
- .NET Framework 4.8
- vPilot 3.x 已安装

### 编译步骤

1. 打开 Visual Studio 2022
2. 打开 `VatsimCompanionPlugin.sln`
3. 确认 vPilot SDK 引用路径正确：
   ```
   C:\Program Files\vPilot\RossCarlson.Vatsim.Vpilot.Plugins.dll
   ```
4. 选择 **Release** 配置
5. 点击 **生成 → 生成解决方案** (Ctrl+Shift+B)

### 自动安装

插件会自动复制到 vPilot Plugins 目录：
```
C:\Program Files\vPilot\Plugins\VatsimCompanionPlugin.dll
```

### 手动安装

如果自动复制失败，手动复制：
```bash
copy build\Release\VatsimCompanionPlugin.dll "C:\Program Files\vPilot\Plugins\"
```

### 验证安装

1. 启动 vPilot
2. 检查日志文件：
   ```
   %APPDATA%\VatsimCompanion\Logs\plugin_YYYYMMDD.log
   ```
3. 查看是否有 "Plugin initialized successfully" 消息
4. 在浏览器访问：http://localhost:8765/status

## HTTP API 端点

### 基础 URL
```
http://localhost:8765
```

### 端点列表

#### 1. 获取状态
```http
GET /status
```

**响应示例**：
```json
{
  "success": true,
  "pluginName": "VATSIM Companion",
  "pluginVersion": "1.0.0",
  "vPilotConnected": true,
  "callsign": "ABC123",
  "timestamp": "2026-06-02T12:00:00Z",
  "eventsProcessed": 42,
  "errorCount": 0
}
```

#### 2. 发送消息
```http
POST /send-message
Content-Type: application/json

{
  "messageType": "private",
  "recipient": "XYZ456",
  "message": "Hello from mobile!"
}
```

**消息类型**：
- `private` - 私信（需要 recipient）
- `radio` - 频率消息

#### 3. 执行指令
```http
POST /execute-command
Content-Type: application/json

{
  "command": ".atis KJFK"
}
```

#### 4. 获取飞机状态
```http
GET /get-state
```

**响应示例**：
```json
{
  "success": true,
  "callsign": "ABC123",
  "connected": true,
  "position": {
    "latitude": 40.7128,
    "longitude": -74.0060,
    "altitude": 3500
  },
  "heading": 270,
  "groundSpeed": 120,
  "verticalSpeed": 500,
  "squawk": "1200",
  "com1Frequency": 118750,
  "com2Frequency": 121500,
  "onGround": false,
  "timestamp": "2026-06-02T12:00:00Z"
}
```

#### 5. Ping
```http
GET /ping
```

## 配置

插件配置在 `Models.cs` 中的 `PluginConfig` 类：

```csharp
public class PluginConfig
{
    public int HttpPort { get; set; } = 8765;              // HTTP 服务器端口
    public string BridgeUrl { get; set; } = "http://localhost:5000";  // Bridge URL
    public bool EnableLogging { get; set; } = true;         // 启用日志
    public int MaxRetries { get; set; } = 3;               // 最大重试次数
    public int RetryDelayMs { get; set; } = 1000;          // 重试延迟
}
```

## 日志位置

日志文件保存在：
```
%APPDATA%\VatsimCompanion\Logs\plugin_YYYYMMDD.log
```

例如：
```
C:\Users\YourName\AppData\Roaming\VatsimCompanion\Logs\plugin_20260602.log
```

日志自动按天分割，保留最近 7 天。

## 调试

### 启用详细日志

在 Visual Studio 中：
1. 选择 **Debug** 配置
2. 按 F5 启动调试
3. 在 vPilot 中触���事件
4. 查看 Output 窗口

### 附加到进程

如果 vPilot 已运行：
1. Visual Studio → **调试 → 附加到进程**
2. 选择 `vPilot.exe`
3. 设置断点并测试

### 查看 vPilot 日志

vPilot 自身日志位置：
```
%LOCALAPPDATA%\vPilot\Logs\
```

## 常见问题

### 插件未加载

**检查项**：
1. DLL 是否在 `C:\Program Files\vPilot\Plugins\` 目录
2. 右键 DLL → 属性 → 解除阻止
3. 确认 .NET Framework 4.8 已安装
4. 查看 vPilot 日志中的错误信息

### HTTP 端口被占用

修改 `Models.cs` 中的 `HttpPort`：
```csharp
public int HttpPort { get; set; } = 8766;  // 改为其他端口
```

重新编译并重启 vPilot。

### Bridge 无法连接插件

**检查项**：
1. 插件 HTTP 服务器是否启动（访问 http://localhost:8765/ping）
2. 防火墙是否阻止
3. Bridge 是否使用正确的 URL

## 事件流程

```
vPilot 事件 → Plugin.cs 事件处理器 → BridgeCommunicationService → HTTP POST → Bridge
                ↑                                                                    ↓
Bridge HTTP 请求 ← HTTP Server ← Plugin.cs HTTP 处理器 ← PluginCommunicationService
```

## 性能优化

- 事件异步处理，不阻塞 vPilot 主线程
- 重要消息过滤（避免频繁发送所有频率消息）
- HTTP 请求超时设置（5 秒）
- 自动重试机制（最多 3 次）

## 安全性

- HTTP 服务器仅监听 localhost（127.0.0.1）
- 不暴露敏感信息
- 仅允许发送文本消息
- 符合 VATSIM 开发者政策

## 许可证

MIT License

## 支持

- GitHub Issues：报告 Bug
- VATSIM 开发者论坛：技术讨论
- 日志文件：包含详细调试信息
