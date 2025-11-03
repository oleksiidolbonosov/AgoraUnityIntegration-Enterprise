using System;
using System.Threading.Tasks;
using UnityEngine;
using AgoraIntegration.Interfaces;
using AgoraIntegration.Core;

namespace AgoraIntegration.Infrastructure
{
    /// <summary>
    /// Concrete implementation wrapping the actual Agora SDK
    /// Follows Adapter pattern to bridge our interface to Agora SDK
    /// </summary>
    public class AgoraEngineWrapper : IAgoraEngine
    {
        public bool IsInitialized => _rtcEngine != null;

        public event Action<uint> OnUserJoined;
        public event Action<uint, int> OnUserOffline;

#if USING_AGORA_SDK
        private IRtcEngine _rtcEngine;
#endif

        /// <summary>
        /// Initializes the Agora RTC engine with provided App ID
        /// </summary>
        public void Initialize(string appId)
        {
#if USING_AGORA_SDK
            if (_rtcEngine != null) return;

            _rtcEngine = IRtcEngine.GetEngine(appId);

            // Subscribe to SDK events and forward to our interface
            _rtcEngine.OnUserJoined += (uid, elapsed) => OnUserJoined?.Invoke(uid);
            _rtcEngine.OnUserOffline += (uid, reason) => OnUserOffline?.Invoke(uid, (int)reason);

            _rtcEngine.EnableAudio();
            Debug.Log($"[AgoraEngineWrapper] Agora SDK initialized successfully");
#else
            Debug.LogWarning("[AgoraEngineWrapper] Agora SDK not available - running in simulation mode");
#endif
        }

        /// <summary>
        /// Joins a channel with specified parameters
        /// </summary>
        public async Task<JoinResult> JoinChannelAsync(string token, string channel, uint? userId)
        {
#if USING_AGORA_SDK
            if (_rtcEngine == null)
                return JoinResult.Failure("Engine not initialized");

            return await Task.Run(() =>
            {
                try
                {
                    var actualUid = _rtcEngine.JoinChannelByKey(token, channel, null, userId ?? 0);
                    return JoinResult.SuccessResult(actualUid);
                }
                catch (Exception ex)
                {
                    return JoinResult.Failure(ex.Message);
                }
            });
#else
            // Simulation mode for testing without SDK
            await Task.Delay(100);
            var simulatedUid = userId ?? (uint)UnityEngine.Random.Range(1000, 10000);
            OnUserJoined?.Invoke(simulatedUid); // Simulate self-join
            return JoinResult.SuccessResult(simulatedUid);
#endif
        }

        /// <summary>
        /// Leaves the current channel
        /// </summary>
        public async Task LeaveChannelAsync()
        {
#if USING_AGORA_SDK
            if (_rtcEngine != null)
            {
                await Task.Run(() => _rtcEngine.LeaveChannel());
            }
#else
            await Task.Delay(50);
#endif
        }

        /// <summary>
        /// Mutes or unmutes local audio stream
        /// </summary>
        public async Task MuteLocalAudioAsync(bool mute)
        {
#if USING_AGORA_SDK
            if (_rtcEngine != null)
            {
                await Task.Run(() => _rtcEngine.MuteLocalAudioStream(mute));
            }
#else
            await Task.Delay(10);
#endif
        }

        /// <summary>
        /// Cleans up Agora engine resources
        /// </summary>
        public void Dispose()
        {
#if USING_AGORA_SDK
            if (_rtcEngine != null)
            {
                IRtcEngine.Destroy();
                _rtcEngine = null;
                Debug.Log("[AgoraEngineWrapper] Agora engine disposed");
            }
#endif
        }
    }
}
