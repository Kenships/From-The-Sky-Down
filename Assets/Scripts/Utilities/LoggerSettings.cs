using System;
using AYellowpaper.SerializedCollections;
using UnityEditor;
using UnityEngine;

namespace Utilities
{
    public class LoggerSettings : MonoBehaviour
    {
        [SerializeField] private LogSettingsSO logSettingsSO;
        [SerializeField] public SerializedDictionary<LogCategory, bool> logSettings;
        
        public void Awake()
        {
            logSettings = logSettingsSO.logSettings;
            foreach(LogCategory logCategory in Enum.GetValues(typeof(LogCategory)))
            {
                if(!logSettings.TryGetValue(logCategory, out bool isEnabled))
                {
                    logSettings.Add(logCategory, true);
                }
            }
            Logger.SetLoggerConfig(logSettings);
        }
    }
}