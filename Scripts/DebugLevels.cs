using TwistCore;
using UnityEngine;

namespace RequestForMirror
{
    public enum LogLevel
    {
        NoLogs = 4,
        LogErrors = 3,
        LogWarnings = 2,
        LogAll = 1
    }

    public static class DebugLevels
    {
        private static RequestSettings _settings;
        private static RequestSettings Settings => _settings ??= SettingsUtility.Load<RequestSettings>();

        private static bool LogLevelIsLowerThan(LogLevel compareTo)
        {
            return (int)Settings.logLevel <= (int)compareTo;
        }

        public static void Log(object message)
        {
            if (LogLevelIsLowerThan(LogLevel.LogAll))
                Debug.Log(message);
        }

        public static void Log(object message, Object context)
        {
            if (LogLevelIsLowerThan(LogLevel.LogAll))
                Debug.Log(message, context);
        }

        public static void LogWarning(object message)
        {
            if (LogLevelIsLowerThan(LogLevel.LogWarnings))
                Debug.LogWarning(message);
        }

        public static void LogWarning(object message, Object context)
        {
            if (LogLevelIsLowerThan(LogLevel.LogWarnings))
                Debug.LogWarning(message, context);
        }

        public static void LogError(object message)
        {
            if (LogLevelIsLowerThan(LogLevel.LogErrors))
                Debug.LogError(message);
        }

        public static void LogError(object message, Object context)
        {
            if (LogLevelIsLowerThan(LogLevel.LogErrors))
                Debug.LogError(message, context);
        }
    }
}