using UnityEngine;

namespace CharacterController
{
    [CreateAssetMenu(fileName = "JumpSettings", menuName = "Scriptable Object/JumpSettings")]
    public class JumpSettingsSO : ScriptableObject
    {
        [field: SerializeField] public float JumpSpeed{get; private set;}
        [field: SerializeField] public float FreefallGravity{get; private set;}
        [field: SerializeField] public float InitialJumpGravityMultiplier{get; private set;}
        [field: SerializeField] public float TerminalVelocity {get; private set;}
    }
}