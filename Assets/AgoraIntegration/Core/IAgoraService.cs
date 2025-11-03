using System;
using System.Threading.Tasks;

namespace AgoraIntegration.Core
{
    /// <summary>
    /// Core Agora service contract following Interface Segregation Principle
    /// Defines the main operations for voice/video communication
    /// </summary>
    public interface IAgoraService
    {
        // Properties
        string CurrentChannel { get; }
        bool IsConnected { get; }
        bool IsAudioMuted { get; }

        // Events - Observer Pattern
        event Action<string> OnLogMessage;
        event Action<uint> OnUserJoined;
        event Action<uint> OnUserLeft;
        event Action<bool> OnConnectionStateChanged;

        // Core Operations
        void Initialize(string appId);
        Task<JoinResult> JoinChannelAsync(ChannelConfig config);
        Task LeaveChannelAsync();
        Task MuteAudioAsync(bool mute);
    }

    /// <summary>
    /// Configuration DTO for channel joining
    /// Immutable data structure for channel parameters
    /// </summary>
    public struct ChannelConfig
    {
        public string ChannelName;
        public string Token;
        public uint? UserId;
        public AudioProfile AudioProfile;

        public ChannelConfig(string channelName, string token = null, uint? userId = null)
        {
            ChannelName = channelName;
            Token = token;
            UserId = userId;
            AudioProfile = AudioProfile.MusicStandard;
        }
    }

    /// <summary>
    /// Result of join operation with success state and user ID
    /// </summary>
    public struct JoinResult
    {
        public bool Success;
        public uint UserId;
        public string ErrorMessage;

        public static JoinResult SuccessResult(uint userId) => new JoinResult 
        { 
            Success = true, 
            UserId = userId 
        };

        public static JoinResult Failure(string error) => new JoinResult 
        { 
            Success = false, 
            ErrorMessage = error 
        };
    }

    /// <summary>
    /// Audio quality profiles for different use cases
    /// </summary>
    public enum AudioProfile
    {
        Default,
        MusicStandard,
        MusicHighQuality,
        SpeechStandard
    }
}
