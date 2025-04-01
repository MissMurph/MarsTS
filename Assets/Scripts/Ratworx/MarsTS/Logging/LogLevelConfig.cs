using System;
using UnityEngine;

namespace Ratworx.MarsTS.Logging
{
    [CreateAssetMenu(fileName = "LogConfig", menuName = "ScriptableObjects/LoggerConfig")]
    public class LogLevelConfig : ScriptableObject
    {
        public RatLogger.LogLevel logLevel;

        public static event Action<RatLogger.LogLevel> OnLogLevelChange;

        private void OnValidate()
        {
            OnLogLevelChange?.Invoke(logLevel);
        }
    }
}