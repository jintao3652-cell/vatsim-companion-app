# 后台保活和自动重连功能实现

## 问题描述

1. **符号链接错误**: Windows 上 Flutter 构建时出现 PathExistsException
2. **连接断开**: 应用退到后台时 Cloudflare 隧道自动断开
3. **无法重连**: 断线后需要手动重新连接

## 解决方案

### 1. 符号链接错误修复

**原因**: Windows 文件系统的符号链接缓存问题

**解决方法**:
```bash
# Swift 项目
cd swift-companion-app/mobile-app
rm -rf windows/flutter/ephemeral/.plugin_symlinks
flutter clean

# VATSIM 项目
cd vatsim-companion-app/mobile-app
rm -rf windows/flutter/ephemeral/.plugin_symlinks
flutter clean
```

之后可以正常运行 `flutter run`

### 2. 后台保活功能

**实现**: 使用 `flutter_foreground_task` 保持后台运行

#### Android
应用已配置前台服务权限：
- 在 `AndroidManifest.xml` 中声明前台服务
- 应用连接时自动启动前台通知
- 通知显示 "Connected — listening for messages"
- 息屏/后台时保持 SignalR 连接

#### iOS
- 使用后台模式保持连接
- 需要在设置中授予后台权限

### 3. 自动重连功能

**实现细节**:

#### 连接健康检查
```dart
// 每 5 秒检查连接状态
Timer.periodic(Duration(seconds: 5), (_) async {
  if (!state.isConnected) {
    // 触发重连
    _startAutoReconnect();
  }
});
```

#### 自动重连机制
```dart
// 检测到断线后每 5 秒尝试重连
Timer.periodic(Duration(seconds: 5), (_) async {
  try {
    await connect(_lastBridgeAddress!, _lastToken!);
    // 成功后停止重连计时器
  } catch (e) {
    // 继续尝试
  }
});
```

#### 通知提醒
- **断线时**: "Connection lost, trying to reconnect..."
- **重连成功**: "Connection restored successfully"

## 修改的文件

### Swift 项目
```
swift-companion-app/mobile-app/lib/providers/connection_provider.dart
```

### VATSIM 项目
```
vatsim-companion-app/mobile-app/lib/providers/connection_provider.dart
```

## 新增功能

### ConnectionNotifier 类
- ✅ `_reconnectTimer` - 重连计时器
- ✅ `_healthCheckTimer` - 健康检查计时器
- ✅ `_lastBridgeAddress` - 保存地址用于重连
- ✅ `_lastToken` - 保存令牌用于重连

### 方法
- ✅ `_startAutoReconnect()` - 启动自动重连（5秒间隔）
- ✅ `_stopAutoReconnect()` - 停止自动重连
- ✅ `_startConnectionHealthCheck()` - 启动健康检查（5秒间隔）
- ✅ `_stopHealthCheck()` - 停止健康检查
- ✅ `dispose()` - 清理计时器

## 工作流程

```
连接成功
  ↓
启动健康检查 (每5秒)
  ↓
检测到断线
  ↓
显示断线通知
  ↓
启动自动重连 (每5秒)
  ↓
尝试重连...
  ↓
重连成功 → 显示成功通知 → 停止重连计时器
```

## 用户体验改进

### 之前
❌ 退到后台 → 连接断开  
❌ 需要手动重新连接  
❌ 无断线提醒  

### 现在
✅ 退到后台 → 前台服务保持连接  
✅ 断线自动重连（5秒间隔）  
✅ 断线/重连通知提醒  
✅ 健康检查持续监控（5秒间隔）  

## 技术细节

### Cloudflare 隧道优化
```dart
HttpConnectionOptions(
  transport: HttpTransportType.LongPolling,  // 长轮询适配 CF 隧道
  requestTimeout: 30000,                     // 30秒超时
)
```

### SignalR 自动重连
```dart
HubConnectionBuilder()
  .withAutomaticReconnect()  // SignalR 内置重连
  .build()
```

### 双重保障
1. **SignalR 自动重连**: 处理短暂网络波动
2. **应用层重连**: 处理长时间断线和隧道重启

## 权限要求

### Android
```xml
<uses-permission android:name="android.permission.FOREGROUND_SERVICE" />
<uses-permission android:name="android.permission.WAKE_LOCK" />
<uses-permission android:name="android.permission.POST_NOTIFICATIONS" />
```

### iOS
```xml
<key>UIBackgroundModes</key>
<array>
  <string>fetch</string>
  <string>remote-notification</string>
</array>
```

## 测试场景

### 场景 1: 正常断线重连
1. 启动应用并连接
2. 关闭 Bridge 服务
3. 观察: 5秒后开始尝试重连
4. 重启 Bridge 服务
5. 观察: 自动重连成功

### 场景 2: 后台保活
1. 连接成功
2. 按 Home 键退到后台
3. 观察: 通知栏显示前台服务
4. 等待 5 分钟
5. 回到应用: 连接仍然活跃

### 场景 3: 网络切换
1. 连接到 WiFi
2. 切换到移动数据
3. 观察: 自动重连

### 场景 4: 隧道重启
1. 连接到 CF 隧道
2. 重启 cloudflared
3. 观察: 自动重连到新隧道

## 日志输出

```
// 启动重连
Starting auto-reconnect timer (5s interval)

// 重连尝试
Attempting to reconnect...

// 重连失败
Reconnection failed: [error]

// 重连成功
Reconnection successful
Auto-reconnect timer stopped

// 健康检查
Health check: Connection OK
```

## 配置选项

可以通过修改 `Duration` 调整检查间隔：

```dart
// 当前: 5秒
Timer.periodic(const Duration(seconds: 5), ...)

// 更快: 3秒
Timer.periodic(const Duration(seconds: 3), ...)

// 更慢: 10秒
Timer.periodic(const Duration(seconds: 10), ...)
```

## 注意事项

1. **电池消耗**: 前台服务和定时检查会增加电池消耗
2. **通知常驻**: 前台服务通知无法关闭（Android 要求）
3. **后台限制**: iOS 后台运行时间有限制
4. **网络费用**: 移动网络下的重连会消耗流量

## 下一步优化

- [ ] 添加重连次数限制（防止无限重连）
- [ ] 指数退避策略（5s → 10s → 20s）
- [ ] 用户可配置重连间隔
- [ ] 网络状态监听（仅在网络可用时重连）
- [ ] 电池优化模式（降低检查频率）

## 完成状态

✅ Swift 项目已实现  
✅ VATSIM 项目已实现  
✅ 符号链接错误已修复  
✅ 自动重连已启用  
✅ 健康检查已启用  
✅ 通知提醒已完善  
