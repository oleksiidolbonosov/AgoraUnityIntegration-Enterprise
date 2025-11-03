using UnityEngine;
using AgoraIntegration.Interfaces;

namespace AgoraIntegration.Infrastructure
{
    /// <summary>
    /// Concrete logger implementation using Unity's Debug class
    /// Can be easily replaced with custom logging solution
    /// </summary>
    public class UnityLogger : ILogger
    {
        private readonly bool _enableLogging;

        public UnityLogger(bool enableLogging = true)
        {
            _enableLogging = enableLogging;
        }

        public void Log(string message)
        {
            if (_enableLogging)
            {
                Debug.Log(message);
            }
        }

        public void LogError(string error)
        {
            Debug.LogError(error);
        }

        public void LogWarning(string warning)
        {
            if (_enableLogging)
            {
                Debug.LogWarning(warning);
            }
        }
    }
}
