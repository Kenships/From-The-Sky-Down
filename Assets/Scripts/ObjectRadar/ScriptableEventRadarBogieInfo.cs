using Obvious.Soap;
using UnityEngine;

namespace ObjectRadar
{
    [CreateAssetMenu(fileName = "ScriptableEvent" + nameof(RadarBogieInfo), menuName = "Soap/ScriptableEvents/"+ nameof(RadarBogieInfo))]
    public class ScriptableEventRadarBogieInfo : ScriptableEvent<RadarBogieInfo>
    {
        
    }
}
