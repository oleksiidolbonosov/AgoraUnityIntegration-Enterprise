using System;
using System.Threading.Tasks;
using AgoraIntegration.Core;

namespace AgoraIntegration.Interfaces
{
    /// <summary>
    /// Abstraction over Agora SDK for testability and dependency inversion
    /// Enables mocking and simulation without actual SDK dependencies
    /// </summary>
    public interface IAgoraEngine : IDisposable
    {
        bool IsInitialized { get; }

        event Action<uint> OnUserJoined;
        event Action<uint, int> OnUserOffline;

        void Initialize(string appId);
        Task<JoinResult> JoinChannelAsync(string token, string channel, uint? userId);
        Task LeaveChannelAsync();
        Task MuteLocalAudioAsync(bool mute);
    }

    /// <summary>
    /// Logger abstraction for different logging strategies
    /// Supports Unity console, file logging, or remote logging
    /// </summary>
    public interface ILogger
    {
        void Log(string message);
        void LogError(string error);
        void LogWarning(string warning);
    }
}
