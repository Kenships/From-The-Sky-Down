using UnityEngine;

namespace CharacterController
{
    [CreateAssetMenu(fileName = "DashSettings", menuName = "Scriptable Object/DashSettings")]
    public class DashSettingsSO : ScriptableObject
    {
        [field: SerializeField] public float DashSpeed{get; private set;}
        [field: SerializeField] public float DashDrag{get; private set;}
        [field: SerializeField] public float DashCooldown{get; private set;}
        [field: SerializeField] public float BulletJumpSpeed{get; private set;}
    }
}