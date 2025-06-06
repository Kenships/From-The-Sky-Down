using KinematicCharacterController;
using Obvious.Soap;
using UnityEngine;

namespace CharacterController
{
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("Player Movement Settings")]
        [SerializeField] private float groundedSpeed = 15;
        
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private KinematicCharacterMotor motor;
        
        private void Update()
        {
            UpdateAnimations();
        }
        
        private void UpdateAnimations()
        {
            Vector3 currentVelocity = motor.BaseVelocity;
            float normalizedSpeed = currentVelocity.sqrMagnitude / (groundedSpeed * groundedSpeed);
            animator.SetFloat("Velocity", normalizedSpeed);
        }
    }
}