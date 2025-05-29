using Obvious.Soap;
using UnityEngine;

namespace CharacterController
{
    [CreateAssetMenu(fileName = "MotorInfo", menuName = "Scriptable Object/MotorInfo")]
    public class MotorInfoSO: ScriptableObject
    {
        public Vector3 Acceleration
        {
            get => acceleration.Value;
            set => acceleration.Value = value;
        }
        
        [SerializeField] private Vector3Variable acceleration;
    }
}