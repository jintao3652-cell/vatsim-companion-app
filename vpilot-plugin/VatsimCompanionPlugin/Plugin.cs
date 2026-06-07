using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using RossCarlson.Vatsim.Vpilot.Plugins;
using RossCarlson.Vatsim.Vpilot.Plugins.Events;
using Newtonsoft.Json;
using VatsimCompanionPlugin.Models;
using VatsimCompanionPlugin.Services;
using VatsimCompanionPlugin.Utils;

namespace VatsimCompanionPlugin
{
    /// <summary>
    /// VATSIM Companion Plugin for vPilot
    /// 监听 vPilot 事件并通过本地 HTTP 服务器与 Bridge 通信
    /// </summary>
    public class VatsimCompanionPlugin : IPlugin
    {
        private IBroker _broker;
        private HttpListener _httpListener;
        private bool _isRunning;
        private PluginConfig _config;
        private Logger _logger;
        private BridgeCommunicationService _bridgeService;

        // 统计信息
        private int _eventsProcessed = 0;
        private int _errorCount = 0;

        // 当前会话状态(SDK 不提供查询，连接时缓存)
        private string _currentCallsign = "N/A";
        private bool _networkConnected = false;

        // 管制 callsign->格式化频率，供下线时显示频率(下线事件 SDK 只给 callsign)
        private readonly Dictionary<string, string> _controllerFreqs = new Dictionary<string, string>();
        // 消息去重: key=from|content -> 收取时刻
        private readonly Dictionary<string, DateTime> _recentMessages = new Dictionary<string, DateTime>();

        public string Name => "VATSIM Companion";

        public void Initialize(IBroker broker)
        {
            try
            {
                _broker = broker;
                _config = new PluginConfig();
                _logger = new Logger(_config.EnableLogging);
                _bridgeService = new BridgeCommunicationService(_config, _logger);

                _logger.Info("===========================================");
                _logger.Info("VATSIM Companion Plugin Initializing...");
                _logger.Info("===========================================");

                // 订阅 vPilot 事件
                SubscribeToEvents();

                // 启动本地 HTTP 服务器供 Bridge 调用
                StartHttpServer();

                // 清理旧日志
                _logger.ClearOldLogs(7);

                // 检查 Bridge 连接
                Task.Run(async () =>
                {
                    var connected = await _bridgeService.CheckConnectionAsync();
                    if (connected)
                    {
                        _logger.Info($"Bridge service detected at {_config.BridgeUrl}");
                    }
                    else
                    {
                        _logger.Info("Bridge service not detected (will retry on events)");
                    }
                });

                _logger.Info("Plugin initialized successfully");
                _logger.Info($"HTTP server listening on port {_config.HttpPort}");
                LogMessage("VATSIM Companion Plugin loaded successfully");
            }
            catch (Exception ex)
            {
                _logger?.Error("Failed to initialize plugin", ex);
                throw;
            }
        }

        #region Event Subscription

        private void SubscribeToEvents()
        {
            _broker.PrivateMessageReceived += OnPrivateMessageReceived;
            _broker.RadioMessageReceived += OnRadioMessageReceived;
            _broker.BroadcastMessageReceived += OnBroadcastMessageReceived;
            _broker.SessionEnded += OnSessionEnded;
            _broker.ControllerAdded += OnControllerAdded;
            _broker.ControllerDeleted += OnControllerDeleted;
            _broker.NetworkConnected += OnNetworkConnected;
            _broker.NetworkDisconnected += OnNetworkDisconnected;
            // FrequencyChanged and SquawkChanged events don't exist in vPilot 3.12.1 SDK
            // _broker.FrequencyChanged += OnFrequencyChanged;
            // _broker.SquawkChanged += OnSquawkChanged;
            _broker.SelcalAlertReceived += OnSelcalReceived;

            _logger.Info("Subscribed to vPilot events");
        }

        private void UnsubscribeFromEvents()
        {
            if (_broker != null)
            {
                _broker.PrivateMessageReceived -= OnPrivateMessageReceived;
                _broker.RadioMessageReceived -= OnRadioMessageReceived;
                _broker.BroadcastMessageReceived -= OnBroadcastMessageReceived;
                _broker.SessionEnded -= OnSessionEnded;
                _broker.ControllerAdded -= OnControllerAdded;
                _broker.ControllerDeleted -= OnControllerDeleted;
                _broker.NetworkConnected -= OnNetworkConnected;
                _broker.NetworkDisconnected -= OnNetworkDisconnected;
                // _broker.FrequencyChanged -= OnFrequencyChanged;
                // _broker.SquawkChanged -= OnSquawkChanged;
                _broker.SelcalAlertReceived -= OnSelcalReceived;

                _logger.Info("Unsubscribed from vPilot events");
            }
        }

        #endregion

        #region Event Handlers

        private void OnPrivateMessageReceived(object sender, PrivateMessageReceivedEventArgs e)
        {
            try
            {
                _eventsProcessed++;

                // 去重检查：2秒内相同来源+内容视为重复
                if (IsDuplicate(e.From, e.Message))
                {
                    _logger.Info($"[DEDUP] Skipped duplicate private message from {e.From}");
                    return;
                }

                _logger.Info($"Private message from {e.From}: {e.Message}");

                var eventData = new PluginEventData
                {
                    EventType = PluginEventType.PrivateMessage,
                    Data = new Dictionary<string, object>
                    {
                        { "messageType", "private" },
                        { "from", e.From },
                        { "to", "" }, // 'To' property doesn't exist in new SDK
                        { "content", e.Message },
                        { "timestamp", DateTime.UtcNow.ToString("o") }
                    }
                };

                _bridgeService.SendEvent(eventData);
            }
            catch (Exception ex)
            {
                _errorCount++;
                _logger.Error("Error handling private message", ex);
            }
        }

        private void OnRadioMessageReceived(object sender, RadioMessageReceivedEventArgs e)
        {
            try
            {
                _eventsProcessed++;

                // 去重检查：2秒内相同来源+内容视为重复
                if (IsDuplicate(e.From, e.Message))
                {
                    _logger.Info($"[DEDUP] Skipped duplicate radio message from {e.From}");
                    return;
                }

                // Note: Cannot check if message mentions us without access to OurCallsign in new SDK
                // Send all messages to bridge, let it handle filtering
                _logger.Info($"Radio message from {e.From}");

                var eventData = new PluginEventData
                {
                    EventType = PluginEventType.RadioMessage,
                    Data = new Dictionary<string, object>
                    {
                        { "messageType", "radio" },
                        { "from", e.From },
                        { "content", e.Message },
                        { "frequencies", e.Frequencies }, // Note: Frequencies (plural) is an array
                        { "mentionedUs", false }, // Cannot determine without OurCallsign
                        { "timestamp", DateTime.UtcNow.ToString("o") }
                    }
                };

                // Send all messages (filtering can be done in bridge/app)
                _bridgeService.SendEvent(eventData);
            }
            catch (Exception ex)
            {
                _errorCount++;
                _logger.Error("Error handling radio message", ex);
            }
        }

        private void OnBroadcastMessageReceived(object sender, BroadcastMessageReceivedEventArgs e)
        {
            try
            {
                _eventsProcessed++;

                // 去重检查：2秒内相同来源+内容视为重复
                if (IsDuplicate(e.From, e.Message))
                {
                    _logger.Info($"[DEDUP] Skipped duplicate broadcast message from {e.From}");
                    return;
                }

                _logger.Info($"Broadcast message from {e.From}");

                _bridgeService.SendEvent(new PluginEventData
                {
                    EventType = PluginEventType.RadioMessage,
                    Data = new Dictionary<string, object>
                    {
                        { "messageType", "radio" },
                        { "from", e.From },
                        { "content", e.Message },
                        { "timestamp", DateTime.UtcNow.ToString("o") }
                    }
                });
            }
            catch (Exception ex)
            {
                _errorCount++;
                _logger.Error("Error handling broadcast message", ex);
            }
        }

        private void OnSessionEnded(object sender, EventArgs e)
        {
            try
            {
                _eventsProcessed++;
                _networkConnected = false;
                SendSystemMessage("Session ended");
            }
            catch (Exception ex) { _errorCount++; _logger.Error("Error handling session ended", ex); }
        }

        private void OnControllerAdded(object sender, ControllerAddedEventArgs e)
        {
            try
            {
                _eventsProcessed++;
                if (e.Callsign != null && e.Callsign.EndsWith("_ATIS")) return; // 不提示 ATIS 上线
                string freq = FormatFrequency(e.Frequency);
                _controllerFreqs[e.Callsign] = freq;
                SendSystemMessage($"Controller online: {e.Callsign} ({freq})");
            }
            catch (Exception ex) { _errorCount++; _logger.Error("Error handling controller added", ex); }
        }

        private void OnControllerDeleted(object sender, ControllerDeletedEventArgs e)
        {
            try
            {
                _eventsProcessed++;
                if (e.Callsign != null && e.Callsign.EndsWith("_ATIS")) return;
                string freq = _controllerFreqs.TryGetValue(e.Callsign, out var f) ? f : null;
                _controllerFreqs.Remove(e.Callsign);
                SendSystemMessage(freq != null
                    ? $"Controller offline: {e.Callsign} ({freq})"
                    : $"Controller offline: {e.Callsign}");
            }
            catch (Exception ex) { _errorCount++; _logger.Error("Error handling controller deleted", ex); }
        }

        private void OnNetworkConnected(object sender, NetworkConnectedEventArgs e)
        {
            try
            {
                _eventsProcessed++;
                _logger.Info($"Connected to VATSIM as {e.Callsign}");

                _currentCallsign = e.Callsign;
                _networkConnected = true;
                var eventData = new PluginEventData
                {
                    EventType = PluginEventType.NetworkConnected,
                    Data = new Dictionary<string, object>
                    {
                        { "connected", true },
                        { "callsign", e.Callsign },
                        { "server", "N/A" }, // ServerName doesn't exist in new SDK
                        { "timestamp", DateTime.UtcNow.ToString("o") }
                    }
                };

                _bridgeService.SendEvent(eventData);

                SendSystemMessage($"Connected to network as {e.Callsign}");
            }
            catch (Exception ex)
            {
                _errorCount++;
                _logger.Error("Error handling network connected", ex);
            }
        }

        private void OnNetworkDisconnected(object sender, EventArgs e)
        {
            try
            {
                _eventsProcessed++;
                _logger.Info("Disconnected from VATSIM");

                _networkConnected = false;

                var eventData = new PluginEventData
                {
                    EventType = PluginEventType.NetworkDisconnected,
                    Data = new Dictionary<string, object>
                    {
                        { "connected", false },
                        { "timestamp", DateTime.UtcNow.ToString("o") }
                    }
                };

                _bridgeService.SendEvent(eventData);

                SendSystemMessage("Disconnected from network");
            }
            catch (Exception ex)
            {
                _errorCount++;
                _logger.Error("Error handling network disconnected", ex);
            }
        }

        private void OnFrequencyChanged(object sender, dynamic e)
        {
            try
            {
                _eventsProcessed++;
                _logger.Info($"Frequency changed: {e.Radio} = {FormatFrequency(e.NewFrequency)}");

                var eventData = new PluginEventData
                {
                    EventType = PluginEventType.FrequencyChanged,
                    Data = new Dictionary<string, object>
                    {
                        { "frequencyChanged", true },
                        { "radio", e.Radio },
                        { "frequency", e.NewFrequency },
                        { "formattedFrequency", FormatFrequency(e.NewFrequency) },
                        { "timestamp", DateTime.UtcNow.ToString("o") }
                    }
                };

                _bridgeService.SendEvent(eventData);
            }
            catch (Exception ex)
            {
                _errorCount++;
                _logger.Error("Error handling frequency change", ex);
            }
        }

        private void OnSquawkChanged(object sender, dynamic e)
        {
            try
            {
                _eventsProcessed++;
                _logger.Info($"Squawk changed: {e.NewSquawk}");

                var eventData = new PluginEventData
                {
                    EventType = PluginEventType.SquawkChanged,
                    Data = new Dictionary<string, object>
                    {
                        { "squawkChanged", true },
                        { "squawk", e.NewSquawk },
                        { "timestamp", DateTime.UtcNow.ToString("o") }
                    }
                };

                _bridgeService.SendEvent(eventData);
            }
            catch (Exception ex)
            {
                _errorCount++;
                _logger.Error("Error handling squawk change", ex);
            }
        }

        private void OnSelcalReceived(object sender, SelcalAlertReceivedEventArgs e)
        {
            try
            {
                _eventsProcessed++;
                _logger.Info($"SELCAL received from {e.From}");

                var eventData = new PluginEventData
                {
                    EventType = PluginEventType.SelcalReceived,
                    Data = new Dictionary<string, object>
                    {
                        { "selcal", true },
                        { "from", e.From },
                        { "timestamp", DateTime.UtcNow.ToString("o") }
                    }
                };

                _bridgeService.SendEvent(eventData);

                // SELCAL is important - always notify
                LogMessage($"SELCAL received from {e.From}");
            }
            catch (Exception ex)
            {
                _errorCount++;
                _logger.Error("Error handling SELCAL", ex);
            }
        }

        #endregion

        #region HTTP Server

        private void StartHttpServer()
        {
            try
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add($"http://localhost:{_config.HttpPort}/");
                _httpListener.Start();
                _isRunning = true;

                Task.Run(() => ListenForRequests());

                _logger.Info($"HTTP server started on port {_config.HttpPort}");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to start HTTP server", ex);
                throw;
            }
        }

        private async void ListenForRequests()
        {
            while (_isRunning)
            {
                try
                {
                    var context = await _httpListener.GetContextAsync();
                    Task.Run(() => HandleRequest(context));
                }
                catch (HttpListenerException)
                {
                    // Listener stopped
                    break;
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        _logger.Error("Error listening for requests", ex);
                    }
                }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                _logger.Debug($"HTTP {request.HttpMethod} {request.Url.AbsolutePath}");

                // 设置 CORS
                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");

                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 200;
                    response.Close();
                    return;
                }

                string responseString = "";
                response.StatusCode = 200;

                switch (request.Url.AbsolutePath.ToLower())
                {
                    case "/status":
                        responseString = HandleStatusRequest();
                        break;

                    case "/send-message":
                        responseString = HandleSendMessage(request);
                        break;

                    case "/execute-command":
                        responseString = HandleExecuteCommand(request);
                        break;

                    case "/get-state":
                        responseString = HandleGetState();
                        break;

                    case "/ping":
                        responseString = JsonConvert.SerializeObject(ApiResponse.Ok(new { pong = true }));
                        break;

                    default:
                        response.StatusCode = 404;
                        responseString = JsonConvert.SerializeObject(ApiResponse.Error("Endpoint not found"));
                        break;
                }

                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.ContentType = "application/json; charset=utf-8";
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.Close();
            }
            catch (Exception ex)
            {
                _errorCount++;
                _logger.Error("Error handling HTTP request", ex);

                try
                {
                    context.Response.StatusCode = 500;
                    var errorResponse = JsonConvert.SerializeObject(ApiResponse.Error(ex.Message));
                    byte[] buffer = Encoding.UTF8.GetBytes(errorResponse);
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    context.Response.Close();
                }
                catch { }
            }
        }

        private string HandleStatusRequest()
        {
            var status = new PluginStatusResponse
            {
                Success = true,
                PluginName = Name,
                PluginVersion = "1.0.0",
                VPilotConnected = _networkConnected,
                Callsign = _currentCallsign,
                Timestamp = DateTime.UtcNow,
                EventsProcessed = _eventsProcessed,
                ErrorCount = _errorCount
            };

            return JsonConvert.SerializeObject(status);
        }

        private string HandleSendMessage(HttpListenerRequest request)
        {
            try
            {
                using (var reader = new System.IO.StreamReader(request.InputStream))
                {
                    string body = reader.ReadToEnd();
                    var data = JsonConvert.DeserializeObject<SendMessageRequest>(body);

                    if (data == null || string.IsNullOrEmpty(data.Message))
                    {
                        return JsonConvert.SerializeObject(ApiResponse.Error("Invalid request"));
                    }

                    if (data.MessageType == "private")
                    {
                        if (string.IsNullOrEmpty(data.Recipient))
                        {
                            return JsonConvert.SerializeObject(ApiResponse.Error("Recipient required for private message"));
                        }

                        _broker.SendPrivateMessage(data.Recipient, data.Message);
                        _logger.Info($"Sent private message to {data.Recipient}: {data.Message}");
                    }
                    else if (data.MessageType == "radio")
                    {
                        _broker.SendRadioMessage(data.Message);
                        _logger.Info($"Sent radio message: {data.Message}");
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(ApiResponse.Error("Invalid message type"));
                    }

                    return JsonConvert.SerializeObject(ApiResponse.Ok());
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending message", ex);
                return JsonConvert.SerializeObject(ApiResponse.Error(ex.Message));
            }
        }

        private string HandleExecuteCommand(HttpListenerRequest request)
        {
            try
            {
                using (var reader = new System.IO.StreamReader(request.InputStream))
                {
                    string body = reader.ReadToEnd();
                    var data = JsonConvert.DeserializeObject<ExecuteCommandRequest>(body);

                    if (data == null || string.IsNullOrEmpty(data.Command))
                    {
                        return JsonConvert.SerializeObject(ApiResponse.Error("Invalid request"));
                    }

                    // ExecuteDotCommand method doesn't exist in vPilot 3.12.1 SDK
                    // This functionality is not available
                    _logger.Warning($"Command execution not supported in this vPilot version: {data.Command}");

                    return JsonConvert.SerializeObject(ApiResponse.Error("Command execution not supported in vPilot 3.12.1"));
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error executing command", ex);
                return JsonConvert.SerializeObject(ApiResponse.Error(ex.Message));
            }
        }

        private string HandleGetState()
        {
            try
            {
                // vPilot 3.12.1 SDK doesn't provide access to aircraft state properties
                // Return minimal placeholder data
                var state = new AircraftStateResponse
                {
                    Success = true,
                    Callsign = _currentCallsign,
                    Connected = _networkConnected,
                    Position = new PositionData
                    {
                        Latitude = 0,
                        Longitude = 0,
                        Altitude = 0
                    },
                    Heading = 0,
                    GroundSpeed = 0,
                    VerticalSpeed = 0,
                    Squawk = "0000",
                    Com1Frequency = 0,
                    Com2Frequency = 0,
                    OnGround = false,
                    Timestamp = DateTime.UtcNow
                };

                return JsonConvert.SerializeObject(state);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting state", ex);
                return JsonConvert.SerializeObject(ApiResponse.Error(ex.Message));
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 以系统消息形式推送状态变化给 App(显示在聊天流)
        /// </summary>
        private void SendSystemMessage(string text)
        {
            _bridgeService.SendEvent(new PluginEventData
            {
                EventType = PluginEventType.PrivateMessage, // 映射成 "message" 类型上报
                Data = new Dictionary<string, object>
                {
                    { "messageType", "system" },
                    { "from", "SYSTEM" },
                    { "content", text },
                    { "timestamp", DateTime.UtcNow.ToString("o") }
                }
            });
        }

        private bool IsImportantMessage(string message)
        {
            // 检查是否是重要消息（ATC 指令关键词）
            string lowerMessage = message.ToLower();
            string[] keywords = {
                "taxi", "cleared", "hold", "contact", "squawk",
                "descend", "climb", "maintain", "turn", "heading"
            };

            foreach (var keyword in keywords)
            {
                if (lowerMessage.Contains(keyword))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsDuplicate(string from, string content)
        {
            // 构造去重 key：来源|内容
            string key = $"{from}|{content}";
            DateTime now = DateTime.UtcNow;

            // 清理 2 秒前的旧记录（避免字典无限增长）
            var keysToRemove = new List<string>();
            foreach (var kvp in _recentMessages)
            {
                if ((now - kvp.Value).TotalSeconds > 2)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            foreach (var k in keysToRemove)
            {
                _recentMessages.Remove(k);
            }

            // 检查是否重复
            if (_recentMessages.ContainsKey(key))
            {
                var lastTime = _recentMessages[key];
                if ((now - lastTime).TotalSeconds <= 2)
                {
                    return true; // 2 秒内重复
                }
            }

            // 记录新消息
            _recentMessages[key] = now;
            return false;
        }

        private string FormatFrequency(int frequency)
        {
            // 频率单位 kHz。区域管制可能只填后几位(如 28325 表示 128.325)，
            // VATSIM 频率均为 1xx.xxx MHz，不足 6 位时首位补 1 而非 0。
            if (frequency < 100000) frequency += 100000;
            string freqStr = frequency.ToString();
            return $"{freqStr.Substring(0, 3)}.{freqStr.Substring(3)}";
        }

        private void LogMessage(string message)
        {
            _broker?.PostDebugMessage($"[VATSIM Companion] {message}");
            System.Diagnostics.Debug.WriteLine($"[VATSIM Companion] {message}");
        }

        #endregion

        public void Dispose()
        {
            try
            {
                _logger.Info("===========================================");
                _logger.Info("VATSIM Companion Plugin Shutting Down...");
                _logger.Info($"Total events processed: {_eventsProcessed}");
                _logger.Info($"Total errors: {_errorCount}");
                _logger.Info("===========================================");

                _isRunning = false;

                // 取消订阅事件
                UnsubscribeFromEvents();

                // 停止 HTTP 服务器
                if (_httpListener != null)
                {
                    _httpListener.Stop();
                    _httpListener.Close();
                }

                // 释放 Bridge 服务
                _bridgeService?.Dispose();

                LogMessage("Plugin disposed successfully");
            }
            catch (Exception ex)
            {
                _logger?.Error("Error disposing plugin", ex);
            }
        }
    }
}
