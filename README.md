# VATSIM Companion App

vPilot 的移动端伴侣应用 — 在手机上实时收发 VATSIM 消息、查看连接状态与呼号。支持 Android / iOS。

```
📱 Flutter App  ←→  ☁️ cloudflared 隧道  ←→  🌉 Bridge 服务  ←→  ✈️ vPilot 插件
  (手机端)          (远程访问,可选)         (PC 本地服务)        (vPilot 内运行)
```

---

## 功能

- 实时收发**频率消息**与**私信**
- 接收**全网广播**（Broadcast）
- **系统消息**：连接/断开网络、会话结束、METAR、ATIS、管制员上下线
- 顶部显示当前**呼号**
- 6 位配对码 + JWT 认证
- 自动重连、消息历史

> **SDK 限制（无法实现）**：vPilot 插件 SDK 未暴露以下数据，故手机端拿不到——
> 1. vPilot 窗口里的 `SERVER:` 欢迎语、`Connected/Disconnected from voice server` 语音服务器状态
> 2. 本机当前所在 COM 频率
> 这些是 vPilot 主程序自有信息，插件接口无对应事件。

---

## 一键启动

PC 端只需双击运行根目录的 **`start.bat`**，它会：

1. 启动 **Bridge 服务**（端口 5000，新窗口）
2. 启动 **cloudflared 隧道**（新窗口，给出 `https://xxx.trycloudflare.com` 远程地址）

> **首次使用前**：用文本编辑器打开 [start.bat](start.bat)，按需修改顶部配置：
> - `CLOUDFLARED` — cloudflared.exe 路径。若已加入系统 PATH 可留空（脚本自动探测）；否则填完整路径，如 `set CLOUDFLARED=C:\Tools\cloudflared.exe`。
> - `BRIDGE_PORT` — 默认 5000，须与 [appsettings.json](bridge-service/windows/VatsimBridge/appsettings.json) 的 `Port` 一致。
> 没有 cloudflared 时脚本仍会启动 Bridge，手机用 **局域网IP:5000** 连接。

**vPilot 需另行手动启动**（插件随 vPilot 自动加载，start.bat 不管理 vPilot 本体）。

---

## 首次构建与安装

一键启动前，三个组件需各构建一次。

### 1. 环境要求
- **.NET SDK**（Bridge 用 .NET 7、插件用 .NET Framework 4.8）
- **Flutter SDK**（构建手机 App）
- **vPilot** 已安装
- **cloudflared**（可选，远程访问用，见下方专节）

### cloudflared 隧道（远程访问）

不在同一局域网时（如手机走 4G/5G），需用 cloudflared 把本地 Bridge 暴露成公网地址。

**安装**（任选其一）：
```powershell
# 方式一: winget
winget install --id Cloudflare.cloudflared

# 方式二: 手动下载 exe，放到任意目录(如 C:\Tools\)
# https://github.com/cloudflare/cloudflared/releases/latest
# 下载 cloudflared-windows-amd64.exe，重命名为 cloudflared.exe
```

**验证安装**：
```powershell
cloudflared --version
```
- 能输出版本号 → 已在 PATH 中，start.bat 会自动找到，`CLOUDFLARED` 变量留空即可。
- 提示"不是命令" → 没进 PATH，需在 [start.bat](start.bat) 顶部填路径：`set CLOUDFLARED=C:\Tools\cloudflared.exe`。

cloudflared tunnel --url http://localhost:5000

**手动运行**（不想用 start.bat 时）：
```powershell
cloudflared tunnel --url http://localhost:5000
```
运行后输出形如：
```
+-----------------------------------------------------------+
|  https://random-words-1234.trycloudflare.com              |
+-----------------------------------------------------------+
```
把这个 `https://...trycloudflare.com` 地址填进手机 App。

> **注意**：
> - 这是**免费临时隧道**，无需 Cloudflare 账号，但**每次重启地址都会变**，需在 App 重新填写。
> - 隧道窗口必须**保持开启**，关掉地址即失效。
> - 想要固定地址需配置 Cloudflare 命名隧道（需账号+域名），本项目默认用临时隧道。

### 2. vPilot 插件（C# / .NET 4.8）
```bash
cd vpilot-plugin\VatsimCompanionPlugin
dotnet build -c Release
```
构建后自动拷贝到 vPilot 插件目录（[csproj](vpilot-plugin/VatsimCompanionPlugin/VatsimCompanionPlugin.csproj) 的 PostBuild，目标 `E:\msfs app\vPilot\Plugins\`，按你的安装位置调整）。

> **拷贝失败 / DLL 没更新**：多半是 **vPilot 正在运行锁定了 DLL**。先完全退出 vPilot 再构建。脚本用 `|| exit 0` 让拷贝失败不中断编译，所以务必检查目标 DLL 的时间戳确认已更新：
> ```powershell
> Get-ChildItem "E:\msfs app\vPilot\Plugins\VatsimCompanionPlugin.dll" | Select LastWriteTime
> ```

### 3. Bridge 服务（C# / .NET 7）
```bash
cd bridge-service\windows\VatsimBridge
dotnet build -c Release
```
> 生产环境请修改 [appsettings.json](bridge-service/windows/VatsimBridge/appsettings.json) 里的 `Jwt:SecretKey`。

cd "d:\HugoMoveData\User\16832\Desktop\AetherLink\vatsim-companion-app\bridge-service\windows\VatsimBridge"
dotnet run


### 获取配对码（Pairing Code）

Bridge **不会自动出码**，需在 Bridge 运行后主动调一次接口生成。任选一种：

```powershell
# 方式一: PowerShell(本机最快)
Invoke-RestMethod -Method Post -Uri http://localhost:5000/api/pairing/start | Select pairingCode, expiresAt

# 方式二: curl
curl -X POST http://localhost:5000/api/pairing/start
```

或浏览器打开 `http://localhost:5000/swagger` → `POST /api/pairing/start` → **Execute**。

生成的 6 位码也会同时打印在 **Bridge 运行窗口**：
```
===========================================
VATSIM Companion - Pairing Code
===========================================
  Code: 123456
  URL:  http://localhost:5000
===========================================
```

> 配对码 **10 分钟有效、一次性使用**。过期或用过需重新调接口生成。


### 4. 手机 App（Flutter）
```bash
cd mobile-app
flutter pub get
flutter build apk --release
```
产物：`build\app\outputs\flutter-apk\app-release.apk`，传到手机安装。设备已连电脑可直接 `flutter install --release`。

---

## 日常使用

1. 启动 **vPilot** 并连上 VATSIM
2. 双击 **`start.bat`** → Bridge + 隧道起来
3. 从隧道窗口复制 `https://xxx.trycloudflare.com` 地址
4. **生成配对码**（见上方[获取配对码](#获取配对码pairing-code)），从 Bridge 窗口或接口响应拿到 6 位码
5. 手机 App 填入隧道地址 + 配对码完成配对
6. 频率/私信/广播/系统消息会实时出现在对应 Tab，顶部显示呼号

> 每次重启 cloudflared，`trycloudflare.com` 地址都会变，需在 App 重新填写。

---

## 项目结构

| 目录 | 组件 | 技术栈 |
|------|------|--------|
| [vpilot-plugin/](vpilot-plugin/) | vPilot 插件，订阅 SDK 事件并上报 Bridge | C# .NET 4.8 |
| [bridge-service/windows/VatsimBridge/](bridge-service/windows/VatsimBridge/) | 本地服务，REST + SignalR，转发消息 | C# .NET 7 |
| [mobile-app/](mobile-app/) | 手机 App | Flutter / Dart |

**数据流**：vPilot 事件 → 插件（HTTP POST 到 Bridge `:5000`）→ Bridge（SignalR 广播）→ App。
App 发消息反向：App → Bridge SignalR → 插件 HTTP（`:8765`）→ vPilot。

**端口**：Bridge `5000`、插件本地 HTTP `8765`。

---

## 排错

| 现象 | 排查 |
|------|------|
| App 连不上 | 隧道窗口是否有有效地址；地址是否填对（每次重启会变）；局域网模式确认手机与 PC 同网段 |
| 能发不能收 | Bridge 必须是含 camelCase 序列化修复的版本（[Program.cs](bridge-service/windows/VatsimBridge/Program.cs) 的 `AddJsonProtocol`），改后需重启 Bridge |
| 收不到任何消息 | 看插件日志（下方），确认 `Subscribed to vPilot events` 与具体事件行 |
| 插件改了没生效 | DLL 被 vPilot 锁住没更新，检查目标 DLL 时间戳；退出 vPilot 重新构建 |

**日志位置**：
- 插件：`%APPDATA%\VatsimCompanion\Logs\plugin_YYYYMMDD.log`（UTC 时间戳）
- Bridge：运行窗口（UTC 时间戳、单行）
- cloudflared：隧道窗口

查看插件日志最新 20 行：
```powershell
Get-Content "$env:APPDATA\VatsimCompanion\Logs\plugin_$(Get-Date -Format yyyyMMdd).log" -Tail 20
```

---

## 更多文档

- [QUICK_START.md](QUICK_START.md) — 快速上手
- [ASSEMBLY_GUIDE.md](ASSEMBLY_GUIDE.md) — 图解组装
- [READINESS_CHECKLIST.md](READINESS_CHECKLIST.md) — 检查清单
- [docs/API.md](docs/API.md) — API 文档
- [docs/SETUP.md](docs/SETUP.md) — 详细安装

---

**版本** v1.0.0


