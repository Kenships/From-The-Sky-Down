using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Utilities
{
    public static class Logger
    {
        private static SerializedDictionary<LogCategory, bool> loggerConfig;
        public static void SetLoggerConfig(SerializedDictionary<LogCategory, bool> loggerConfig)
        {
            Logger.loggerConfig = loggerConfig;
        }
        private static void Initialize()
        {
            loggerConfig = new SerializedDictionary<LogCategory, bool>();
            foreach(LogCategory category in Enum.GetValues(typeof(LogCategory)))
            {
                loggerConfig.Add(category, true);
            }
        }
        public static void Log(string message, LogCategory category = LogCategory.Default)
        {
            if(loggerConfig == null) Initialize();

            if (loggerConfig.TryGetValue(category, out bool value) && value)
            {
                Debug.Log(message);
            }
        }

        public static void LogError(string message, LogCategory category = LogCategory.Default)
        {
            if(loggerConfig == null) Initialize();
            if (loggerConfig.TryGetValue(category, out bool value) && value)
            {
                Debug.LogError(message);
            }
        }
    }
    
    public enum LogCategory{
        Default,
        Debug
    }
}
