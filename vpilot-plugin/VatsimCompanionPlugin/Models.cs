using System;
using System.Collections.Generic;
using System.Linq;

namespace VatsimCompanionPlugin.Models
{
    /// <summary>
    /// Plugin event types
    /// </summary>
    public enum PluginEventType
    {
        PrivateMessage,
        RadioMessage,
        NetworkConnected,
        NetworkDisconnected,
        FrequencyChanged,
        SquawkChanged,
        SelcalReceived
    }

    /// <summary>
    /// Base event data
    /// </summary>
    public class PluginEventData
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public PluginEventType EventType { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Plugin configuration
    /// </summary>
    public class PluginConfig
    {
        public int HttpPort { get; set; } = 8765;
        public string BridgeUrl { get; set; } = "http://localhost:5000";
        public bool EnableLogging { get; set; } = true;
        public int MaxRetries { get; set; } = 3;
        public int RetryDelayMs { get; set; } = 1000;
    }

    /// <summary>
    /// HTTP request/response models
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public static ApiResponse Ok(object data = null)
        {
            return new ApiResponse { Success = true, Data = data };
        }

        public static ApiResponse Error(string message)
        {
            return new ApiResponse { Success = false, Message = message };
        }
    }

    public class SendMessageRequest
    {
        public string MessageType { get; set; }
        public string Recipient { get; set; }
        public string Message { get; set; }
    }

    public class ExecuteCommandRequest
    {
        public string Command { get; set; }
    }

    public class AircraftStateResponse
    {
        public bool Success { get; set; }
        public string Callsign { get; set; }
        public bool Connected { get; set; }
        public PositionData Position { get; set; }
        public int Heading { get; set; }
        public int GroundSpeed { get; set; }
        public int VerticalSpeed { get; set; }
        public string Squawk { get; set; }
        public int Com1Frequency { get; set; }
        public int Com2Frequency { get; set; }
        public bool OnGround { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class PositionData
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Altitude { get; set; }
    }

    public class PluginStatusResponse
    {
        public bool Success { get; set; }
        public string PluginName { get; set; }
        public string PluginVersion { get; set; }
        public bool VPilotConnected { get; set; }
        public string Callsign { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int EventsProcessed { get; set; }
        public int ErrorCount { get; set; }
    }
}
