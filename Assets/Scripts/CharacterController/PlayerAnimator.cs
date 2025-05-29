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
        [SerializeField] private Transform rootVisualTransform;
        [SerializeField] private KinematicCharacterMotor motor;
        
        [Header("Player Persistant Variables")]
        [SerializeField] private MotorInfoSO motorInfo;
        [Header( "Acceleration Tilt Settings" )]
        [SerializeField] private float accelerationTiltSpeed;
        [SerializeField] private float accelerationTiltRecoverySpeed;
        [SerializeField] private float accelerationTiltDeadZone;
        [SerializeField] private float accelerationTiltFactor;
        
        private void Update()
        {
            UpdateAnimations();
        }
        
        private void PerformTilt(float deltaTime)
        {
            Vector3 flatAcceleration = Vector3.ProjectOnPlane(motorInfo.Acceleration, Vector3.up);
            
            Quaternion currentRotation = rootVisualTransform.rotation;
            
            Vector3 upWithLean = (Vector3.up + flatAcceleration * accelerationTiltFactor).normalized;
            
            Vector3 flatForward = Vector3.ProjectOnPlane(rootVisualTransform.forward, Vector3.up).normalized;
            
            Quaternion targetRot = Quaternion.LookRotation(flatForward, upWithLean);
            
            Quaternion uprightRotation = new Quaternion(0f, currentRotation.y, 0f, currentRotation.w);
            
            float angle = Quaternion.Angle(targetRot, uprightRotation);

            if (angle > accelerationTiltDeadZone)
            {
                float tiltSpeed = accelerationTiltSpeed;
                rootVisualTransform.rotation = Quaternion.Slerp(currentRotation, targetRot, deltaTime * tiltSpeed);
            }
        }
        
        private void UpdateAnimations()
        {
            Vector3 currentVelocity = motor.BaseVelocity;
            float normalizedSpeed = currentVelocity.sqrMagnitude / (groundedSpeed * groundedSpeed);
            animator.SetFloat("Velocity", normalizedSpeed);
        }
    }
}