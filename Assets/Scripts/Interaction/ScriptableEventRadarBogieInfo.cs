using Interaction;
using UnityEngine;
using Obvious.Soap;

namespace Interaction
{
    [CreateAssetMenu(fileName = "ScriptableEvent" + nameof(RadarBogieInfo), menuName = "Soap/ScriptableEvents/"+ nameof(RadarBogieInfo))]
    public class ScriptableEventRadarBogieInfo : ScriptableEvent<RadarBogieInfo>
    {
        
    }
}
