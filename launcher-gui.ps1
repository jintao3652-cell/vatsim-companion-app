Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# 创建主窗体
$form = New-Object System.Windows.Forms.Form
$form.Text = "VATSIM Companion Launcher"
$form.Size = New-Object System.Drawing.Size(500, 400)
$form.StartPosition = "CenterScreen"
$form.FormBorderStyle = "FixedDialog"
$form.MaximizeBox = $false

# 标题
$title = New-Object System.Windows.Forms.Label
$title.Location = New-Object System.Drawing.Point(20, 20)
$title.Size = New-Object System.Drawing.Size(460, 30)
$title.Text = "VATSIM Companion - Control Panel"
$title.Font = New-Object System.Drawing.Font("Arial", 14, [System.Drawing.FontStyle]::Bold)
$form.Controls.Add($title)

# 状态标签
$statusLabel = New-Object System.Windows.Forms.Label
$statusLabel.Location = New-Object System.Drawing.Point(20, 60)
$statusLabel.Size = New-Object System.Drawing.Size(460, 20)
$statusLabel.Text = "Status: Checking..."
$form.Controls.Add($statusLabel)

# Bridge 服务控制
$bridgeGroup = New-Object System.Windows.Forms.GroupBox
$bridgeGroup.Location = New-Object System.Drawing.Point(20, 90)
$bridgeGroup.Size = New-Object System.Drawing.Size(460, 100)
$bridgeGroup.Text = "Bridge Service"
$form.Controls.Add($bridgeGroup)

$startButton = New-Object System.Windows.Forms.Button
$startButton.Location = New-Object System.Drawing.Point(20, 30)
$startButton.Size = New-Object System.Drawing.Size(100, 50)
$startButton.Text = "Start"
$startButton.Add_Click({
    $statusLabel.Text = "Status: Starting Bridge..."
    $statusLabel.ForeColor = "Blue"
    Start-Process -FilePath "bridge-service\windows\VatsimBridge\bin\Release\net7.0\VatsimBridge.exe" -WindowStyle Normal
    Start-Sleep -Seconds 2
    $statusLabel.Text = "Status: Bridge Running"
    $statusLabel.ForeColor = "Green"
})
$bridgeGroup.Controls.Add($startButton)

$stopButton = New-Object System.Windows.Forms.Button
$stopButton.Location = New-Object System.Drawing.Point(130, 30)
$stopButton.Size = New-Object System.Drawing.Size(100, 50)
$stopButton.Text = "Stop"
$stopButton.Add_Click({
    Stop-Process -Name "VatsimBridge" -Force -ErrorAction SilentlyContinue
    $statusLabel.Text = "Status: Bridge Stopped"
    $statusLabel.ForeColor = "Red"
})
$bridgeGroup.Controls.Add($stopButton)

$configButton = New-Object System.Windows.Forms.Button
$configButton.Location = New-Object System.Drawing.Point(240, 30)
$configButton.Size = New-Object System.Drawing.Size(100, 50)
$configButton.Text = "Configure"
$configButton.Add_Click({
    Start-Process notepad "bridge-service\windows\VatsimBridge\appsettings.json"
})
$bridgeGroup.Controls.Add($configButton)

$logsButton = New-Object System.Windows.Forms.Button
$logsButton.Location = New-Object System.Drawing.Point(350, 30)
$logsButton.Size = New-Object System.Drawing.Size(90, 50)
$logsButton.Text = "View Logs"
$logsButton.Add_Click({
    Start-Process explorer "bridge-service\windows\VatsimBridge\logs"
})
$bridgeGroup.Controls.Add($logsButton)

# 信息显示
$infoGroup = New-Object System.Windows.Forms.GroupBox
$infoGroup.Location = New-Object System.Drawing.Point(20, 200)
$infoGroup.Size = New-Object System.Drawing.Size(460, 120)
$infoGroup.Text = "Connection Info"
$form.Controls.Add($infoGroup)

$ipLabel = New-Object System.Windows.Forms.Label
$ipLabel.Location = New-Object System.Drawing.Point(20, 25)
$ipLabel.Size = New-Object System.Drawing.Size(420, 20)
$ipLabel.Text = "Local URL: http://localhost:5000"
$infoGroup.Controls.Add($ipLabel)

# 获取本机 IP
$ip = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.IPAddress -like "192.168.*"} | Select-Object -First 1).IPAddress
$networkLabel = New-Object System.Windows.Forms.Label
$networkLabel.Location = New-Object System.Drawing.Point(20, 50)
$networkLabel.Size = New-Object System.Drawing.Size(420, 20)
$networkLabel.Text = "Network URL: http://${ip}:5000"
$infoGroup.Controls.Add($networkLabel)

$pairingLabel = New-Object System.Windows.Forms.Label
$pairingLabel.Location = New-Object System.Drawing.Point(20, 75)
$pairingLabel.Size = New-Object System.Drawing.Size(420, 20)
$pairingLabel.Text = "Pairing Code: 123456 (change in config)"
$infoGroup.Controls.Add($pairingLabel)

# 底部按钮
$exitButton = New-Object System.Windows.Forms.Button
$exitButton.Location = New-Object System.Drawing.Point(380, 330)
$exitButton.Size = New-Object System.Drawing.Size(100, 30)
$exitButton.Text = "Exit"
$exitButton.Add_Click({ $form.Close() })
$form.Controls.Add($exitButton)

# 检查 Bridge 状态
$timer = New-Object System.Windows.Forms.Timer
$timer.Interval = 3000
$timer.Add_Tick({
    $process = Get-Process -Name "VatsimBridge" -ErrorAction SilentlyContinue
    if ($process) {
        $statusLabel.Text = "Status: Bridge Running (PID: $($process.Id))"
        $statusLabel.ForeColor = "Green"
    } else {
        $statusLabel.Text = "Status: Bridge Not Running"
        $statusLabel.ForeColor = "Red"
    }
})
$timer.Start()

# 显示窗体
[void]$form.ShowDialog()
