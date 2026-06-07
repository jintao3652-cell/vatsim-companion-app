; AetherLink 安装脚本 (Inno Setup)
; 编译: iscc AetherLink.iss  (需先 publish Bridge 到 ..\publish\bridge, 并放好 tunnel\cloudflared.exe)

#define AppName "AetherLink"
#define AppVer "1.0.0"

[Setup]
AppName={#AppName}
AppVersion={#AppVer}
SourceDir=.
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputBaseFilename=AetherLinkSetup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
; 部署 DLL 到 vPilot 目录可能需要写入 Program Files 外的路径, 默认装到 Program Files 需管理员
PrivilegesRequired=admin

[Code]
var
  VPilotPage: TInputDirWizardPage;

procedure InitializeWizard;
begin
  // 让用户选择 vPilot 的 Plugins 目录
  VPilotPage := CreateInputDirPage(wpSelectDir,
    'vPilot Plugins 目录', '选择 vPilot 加载插件的目录',
    '安装程序会把 AetherLink 插件复制到此目录。通常是 vPilot 安装目录下的 Plugins 文件夹。',
    False, '');
  VPilotPage.Add('');
  VPilotPage.Values[0] := 'E:\msfs app\vPilot\Plugins';
end;

// 供 [Files] 用: 返回用户选的 vPilot Plugins 目录
function GetVPilotDir(Param: String): String;
begin
  Result := VPilotPage.Values[0];
end;

[Files]
; Bridge 发布产物 (self-contained, 含运行时)
Source: "..\publish\bridge\*"; DestDir: "{app}\bridge"; Flags: recursesubdirs createallsubdirs ignoreversion
; cloudflared 临时隧道
Source: "tunnel\cloudflared-windows-amd64.exe"; DestDir: "{app}\tunnel"; Flags: ignoreversion
; 启动与配对脚本
Source: "start-aetherlink.bat"; DestDir: "{app}"; Flags: ignoreversion
Source: "generate-code.bat"; DestDir: "{app}"; Flags: ignoreversion
; 插件 DLL -> 用户选择的 vPilot Plugins 目录
Source: "..\build\Release\net48\VatsimCompanionPlugin.dll"; DestDir: "{code:GetVPilotDir}"; Flags: ignoreversion
Source: "..\build\Release\net48\Newtonsoft.Json.dll"; DestDir: "{code:GetVPilotDir}"; Flags: ignoreversion

[Icons]
Name: "{group}\Start AetherLink"; Filename: "{app}\start-aetherlink.bat"
Name: "{group}\Pairing Code"; Filename: "{app}\generate-code.bat"
Name: "{autodesktop}\Start AetherLink"; Filename: "{app}\start-aetherlink.bat"

[Run]
Filename: "{app}\start-aetherlink.bat"; Description: "立即启动 AetherLink"; Flags: postinstall shellexec skipifsilent
