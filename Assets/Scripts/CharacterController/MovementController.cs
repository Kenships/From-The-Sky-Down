using System;
using KinematicCharacterController;
using Obvious.Soap;
using UnityEngine;
using UnityEngine.Serialization;
using Logger = Utilities.Logger;

public class MovementController : MonoBehaviour, ICharacterController
{
    [Header("Soap References")]
    [SerializeField] private Vector2Variable inputDirection;
    [SerializeField] private ScriptableEventNoParam jumpEvent;
    
    [Header("Other References")]
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Camera camera;
    [SerializeField] private Transform characterVisual;
    
    [Header("Movement Settings")]
    [SerializeField] private float speed;
    [FormerlySerializedAs("jumpForce")] [SerializeField] private float jumpSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float rotationSpeed;
    
    //Private variables
    private Vector3 _currentInputMovementDirection;
    private Vector3 _lastMovementDirection;
    
    //requestFlags
    private bool _jumpRequested;
    private void Awake()
    {
        motor.CharacterController = this;
    }

    private void Start()
    {
        inputDirection.OnValueChanged += SetCurrentMovementDirectionNormalized;
        jumpEvent.OnRaised += RequestJump;
    }


    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        Quaternion targetRotation = Quaternion.LookRotation(_lastMovementDirection);
        currentRotation = Quaternion.Slerp(currentRotation, targetRotation, deltaTime*rotationSpeed);
        characterVisual.rotation = currentRotation;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (motor.GroundingStatus.IsStableOnGround)
        {
            Vector3 cameraOrientedDirection = GetCameraOrientedDirectionFromInput();
            
            if(!cameraOrientedDirection.Equals(Vector3.zero))
                _lastMovementDirection = cameraOrientedDirection;
            
            var groundedMovement = motor.GetDirectionTangentToSurface(cameraOrientedDirection.normalized, motor.GroundingStatus.GroundNormal);
            currentVelocity = groundedMovement * speed;
        }
        else
        {
            //Apply Gravity
            currentVelocity += motor.CharacterUp * (gravity * deltaTime);
        }

        if (_jumpRequested)
        {
            _jumpRequested = false;
            
            motor.ForceUnground(time: 0f);

            var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
            
            currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
        }
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
        Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
        
    }

    #region Utilities

    private Vector3 GetCameraOrientedDirectionFromInput()
    {
        float yaw = camera.transform.eulerAngles.y;
        return Quaternion.Euler(0, yaw, 0) * _currentInputMovementDirection;
    }

    #endregion
    #region Callback Functions
    
    private void SetCurrentMovementDirectionNormalized(Vector2 direction)
    {
        // Vector3 forwardComponent = camera.transform.forward * direction.y;
        // Vector3 horizontalComponent = camera.transform.right * direction.x;
        // _currentMovementDirection = forwardComponent + horizontalComponent;
        // _currentMovementDirection.y = 0;
        // _currentMovementDirection.Normalize();
        _currentInputMovementDirection = new Vector3(direction.x, 0, direction.y).normalized;
        
    }
    private void RequestJump()
    {
        _jumpRequested = true;
    }
    #endregion
}


