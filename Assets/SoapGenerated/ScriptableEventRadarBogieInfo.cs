using Interaction;
using UnityEngine;
using Obvious.Soap;

namespace DialogueSystem.Utilities
{
    [CreateAssetMenu(fileName = "ScriptableEvent" + nameof(RadarBogieInfo), menuName = "Soap/ScriptableEvents/"+ nameof(RadarBogieInfo))]
    public class ScriptableEventRadarBogieInfo : ScriptableEvent<RadarBogieInfo>
    {
        
    }
}
