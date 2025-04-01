using System;
using UnityEditor;
using UnityEngine;

namespace Ratworx.MarsTS.Logging
{
    public class RatLogger
    {
        // Uncomment with release
        //private const string RatworxLogPrefix = "[RATWORX]";
        private const string RatworxLogPrefix = "";
        
#nullable enable
        public static RatLogger? Error { get; private set; }   = new RatLogger(LogLevel.Error);
        public static RatLogger? Warning { get; private set; } = new RatLogger(LogLevel.Warning); 
        public static RatLogger? Message { get; private set; } = new RatLogger(LogLevel.Message); 
        public static RatLogger? Verbose { get; private set; } = new RatLogger(LogLevel.Verbose);
#nullable disable
        
        private readonly LogLevel _logLevel;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void SetEditorLoggers()
        {
            LogLevelConfig config = Resources.Load<LogLevelConfig>(@"LogConfig");
            LogLevel level = config.logLevel;
            
            SetLogLevel(level);
            LogLevelConfig.OnLogLevelChange += SetLogLevel;
        }

#endif

        private static void SetLogLevel(LogLevel level)
        {
            Error = new RatLogger(LogLevel.Error);
            Warning = (int)level >= (int)LogLevel.Warning ? new RatLogger(LogLevel.Warning) : null;
            Message = (int)level >= (int)LogLevel.Message ? new RatLogger(LogLevel.Message) : null;
            Verbose = (int)level >= (int)LogLevel.Verbose ? new RatLogger(LogLevel.Verbose) : null;
            
            Verbose?.Log($"Changing Log Level: {level}");
        }

        private RatLogger(LogLevel level)
        {
            _logLevel = level;
        }

        public void Log(object message)
        {
            switch (_logLevel)
            {
                case LogLevel.Error when message is Exception e:
                    Debug.LogException(e);
                    break;
                case LogLevel.Error when message is string:
                    Debug.LogError($"{RatworxLogPrefix} {LogLevelPrefix(_logLevel)} {message}");
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning($"{RatworxLogPrefix} {LogLevelPrefix(_logLevel)} {message}");
                    break;
                case LogLevel.Message:
                case LogLevel.Verbose:
                    Debug.Log($"{RatworxLogPrefix} {LogLevelPrefix(_logLevel)} {message}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string LogLevelPrefix(LogLevel level) => level switch
        {
            LogLevel.Error => "[ERROR]",
            LogLevel.Warning => "[WARNING]",
            LogLevel.Message => string.Empty,
            LogLevel.Verbose => string.Empty,
            _ => string.Empty
        };

        public enum LogLevel
        {
            Error,
            Warning,
            Message,
            Verbose
        }
    }
}