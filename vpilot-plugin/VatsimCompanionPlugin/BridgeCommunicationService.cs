using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VatsimCompanionPlugin.Models;
using VatsimCompanionPlugin.Utils;

namespace VatsimCompanionPlugin.Services
{
    /// <summary>
    /// Service for communicating with Bridge
    /// </summary>
    public class BridgeCommunicationService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly PluginConfig _config;
        private readonly Logger _logger;
        private bool _isConnected = false;

        public bool IsConnected => _isConnected;

        public BridgeCommunicationService(PluginConfig config, Logger logger)
        {
            _config = config;
            _logger = logger;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(config.BridgeUrl),
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        /// <summary>
        /// Check if Bridge is available
        /// </summary>
        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/status/health");
                _isConnected = response.IsSuccessStatusCode;
                return _isConnected;
            }
            catch (Exception ex)
            {
                _logger.Debug($"Bridge connection check failed: {ex.Message}");
                _isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Send event to Bridge
        /// </summary>
        public async Task<bool> SendEventAsync(PluginEventData eventData)
        {
            try
            {
                var payload = new
                {
                    type = GetEventTypeName(eventData.EventType),
                    payload = eventData.Data
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/plugin/event", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.Debug($"Event sent successfully: {eventData.EventType}");
                    return true;
                }
                else
                {
                    _logger.Warning($"Failed to send event: {response.StatusCode}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.Debug($"Bridge not reachable: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending event to Bridge", ex);
                return false;
            }
        }

        /// <summary>
        /// Send event synchronously with retry
        /// </summary>
        public void SendEvent(PluginEventData eventData)
        {
            Task.Run(async () =>
            {
                try
                {
                    await SendEventAsync(eventData);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to send event", ex);
                }
            });
        }

        private string GetEventTypeName(PluginEventType eventType)
        {
            switch (eventType)
            {
                case PluginEventType.PrivateMessage:
                case PluginEventType.RadioMessage:
                    return "message";
                case PluginEventType.NetworkConnected:
                case PluginEventType.NetworkDisconnected:
                case PluginEventType.FrequencyChanged:
                case PluginEventType.SquawkChanged:
                    return "state";
                default:
                    return "unknown";
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
