using System;
using System.Threading.Tasks;
using UnityEngine;
using AgoraIntegration.Interfaces;
using AgoraIntegration.Infrastructure;

namespace AgoraIntegration.Core
{
    /// <summary>
    /// Main Agora service implementation following SOLID principles
    /// Single Responsibility: Only handles Agora RTC operations
    /// Open/Closed: Extensible via events and configuration
    /// </summary>
    public class AgoraService : MonoBehaviour, IAgoraService
    {
        [Header("Configuration")]
        [SerializeField] private string _defaultAppId;
        [SerializeField] private string _defaultChannel = "default-room";
        [SerializeField] private bool _enableDebugLogging = true;

        // IAgoraService Implementation
        public string CurrentChannel { get; private set; }
        public bool IsConnected { get; private set; }
        public bool IsAudioMuted { get; private set; }

        public event Action<string> OnLogMessage;
        public event Action<uint> OnUserJoined;
        public event Action<uint> OnUserLeft;
        public event Action<bool> OnConnectionStateChanged;

        private IAgoraEngine _agoraEngine;
        private ILogger _logger;

        /// <summary>
        /// Dependency Injection constructor for testability
        /// Enables inversion of control and mocking
        /// </summary>
        public void Construct(IAgoraEngine engine, ILogger logger)
        {
            _agoraEngine = engine;
            _logger = logger;
        }

        private void Awake()
        {
            // Default construction if not injected (Convention over Configuration)
            if (_agoraEngine == null)
            {
                _agoraEngine = new AgoraEngineWrapper();
                _logger = new UnityLogger(_enableDebugLogging);
            }

            Log("AgoraService initialized");
        }

        /// <summary>
        /// Initializes the Agora engine with provided App ID
        /// </summary>
        /// <param name="appId">Agora App ID from developer console</param>
        /// <exception cref="ArgumentException">Thrown when App ID is invalid</exception>
        public void Initialize(string appId)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentException("AppId cannot be null or empty", nameof(appId));
            }

            try
            {
                _agoraEngine.Initialize(appId);
                _agoraEngine.OnUserJoined += HandleUserJoined;
                _agoraEngine.OnUserOffline += HandleUserLeft;

                Log($"Agora SDK initialized with AppId: {appId}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to initialize Agora: {ex.Message}");
                throw new InvalidOperationException("Agora initialization failed", ex);
            }
        }

        /// <summary>
        /// Joins a voice channel with specified configuration
        /// </summary>
        /// <param name="config">Channel configuration parameters</param>
        /// <returns>Join result with success state and user ID</returns>
        public async Task<JoinResult> JoinChannelAsync(ChannelConfig config)
        {
            if (!_agoraEngine.IsInitialized)
            {
                return JoinResult.Failure("Agora not initialized. Call Initialize() first.");
            }

            if (string.IsNullOrEmpty(config.ChannelName))
            {
                return JoinResult.Failure("Channel name cannot be empty");
            }

            try
            {
                CurrentChannel = config.ChannelName;
                var result = await _agoraEngine.JoinChannelAsync(config.Token, config.ChannelName, config.UserId);

                if (result.Success)
                {
                    IsConnected = true;
                    OnConnectionStateChanged?.Invoke(true);
                    Log($"Successfully joined channel: {CurrentChannel} as user {result.UserId}");
                }
                else
                {
                    LogError($"Failed to join channel: {result.ErrorMessage}");
                }

                return result;
            }
            catch (Exception ex)
            {
                LogError($"Failed to join channel: {ex.Message}");
                return JoinResult.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Leaves the current channel and cleans up resources
        /// </summary>
        public async Task LeaveChannelAsync()
        {
            if (!IsConnected) return;

            try
            {
                await _agoraEngine.LeaveChannelAsync();
                IsConnected = false;
                CurrentChannel = null;
                OnConnectionStateChanged?.Invoke(false);
                Log("Left channel successfully");
            }
            catch (Exception ex)
            {
                LogError($"Error leaving channel: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggles local audio mute state
        /// </summary>
        /// <param name="mute">True to mute, false to unmute</param>
        public async Task MuteAudioAsync(bool mute)
        {
            if (!_agoraEngine.IsInitialized) return;

            try
            {
                await _agoraEngine.MuteLocalAudioAsync(mute);
                IsAudioMuted = mute;
                Log($"Audio {(mute ? "muted" : "unmuted")}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to toggle audio mute: {ex.Message}");
            }
        }

        private void HandleUserJoined(uint userId)
        {
            Log($"Remote user joined: {userId}");
            OnUserJoined?.Invoke(userId);
        }

        private void HandleUserLeft(uint userId, int reason)
        {
            Log($"Remote user left: {userId}, reason: {reason}");
            OnUserLeft?.Invoke(userId);
        }

        private void Log(string message)
        {
            _logger.Log($"[AgoraService] {message}");
            OnLogMessage?.Invoke(message);
        }

        private void LogError(string error)
        {
            _logger.LogError($"[AgoraService] ERROR: {error}");
            OnLogMessage?.Invoke($"ERROR: {error}");
        }

        private void OnDestroy()
        {
            _agoraEngine?.Dispose();
        }
    }
}
