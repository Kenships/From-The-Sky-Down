using UnityEngine;

namespace Player.ControllerSettings
{
    [CreateAssetMenu(fileName = "MovementSettings", menuName = "Scriptable Object/MovementSettings")]
    public class MovementSettingsSO: ScriptableObject
    {
        [field: SerializeField] public float GroundedSpeed{get; private set;}
        [field: SerializeField] public float GroundedAcceleration{get; private set;}
        
        [field: SerializeField] public float AirControlStrength{get; private set;}
        [field: SerializeField] public float MaxAirSpeed{get; private set;}
    }
}
