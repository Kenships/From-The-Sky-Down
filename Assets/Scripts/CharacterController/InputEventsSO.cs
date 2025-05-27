using Obvious.Soap;
using UnityEngine;
using UnityEngine.Serialization;

namespace CharacterController
{
    [CreateAssetMenu(fileName = "InputEvents", menuName = "ScriptableObjects")]
    public class InputEventsSO : ScriptableObject
    {
        public Vector2Variable inputDirection;
        public ScriptableEventNoParam jumpEvent;
        public ScriptableEventNoParam dashEvent;
        public ScriptableEventNoParam bulletJumpEvent;
    }
}