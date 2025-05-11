using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Utilities
{
    [CreateAssetMenu]
    public class LogSettingsSO : ScriptableObject
    {
        public SerializedDictionary<LogCategory, bool> logSettings;
    }
}