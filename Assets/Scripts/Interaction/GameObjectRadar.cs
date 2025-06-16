using System;
using DialogueSystem.Utilities;
using Obvious.Soap;
using UnityEngine;

namespace Interaction
{
    [Serializable]
    public class RadarBogieInfo
    {
        public RadarBogieInfo(GameObject bogie, bool inRange)
        {
            Bogie = bogie;
            InRange = inRange;
        }
        
        public GameObject Bogie { get; private set; }
        public bool InRange { get; private set; }
    }
    [RequireComponent(typeof(Collider))]
    public class GameObjectRadar : MonoBehaviour
    {
        void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }
        
        [SerializeField] private ScriptableEventRadarBogieInfo radarBogieInfo;

        
        
        private void OnTriggerEnter(Collider other)
        {
            radarBogieInfo.Raise(new RadarBogieInfo(other.gameObject, true));
        }

        private void OnTriggerExit(Collider other)
        {
            radarBogieInfo.Raise(new RadarBogieInfo(other.gameObject, false));
        }
    }
}
