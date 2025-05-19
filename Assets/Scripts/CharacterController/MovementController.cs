using System;
using KinematicCharacterController;
using Obvious.Soap;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;
using Logger = Utilities.Logger;

public class MovementController : MonoBehaviour, ICharacterController
{
    [Header("Soap References")]
    [SerializeField] private Vector2Variable inputDirection;
    [SerializeField] private ScriptableEventNoParam jumpEvent;
    
    [Header("Other References")]
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform characterVisual;
    
    [Header("Movement Settings")]
    [SerializeField] private float groundedSpeed;
    [SerializeField] private float groundedAcceleration;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float initialJumpGravityMultiplier;
    [SerializeField] private float rotationSpeed;

    [Header("Timer Settings")] 
    [SerializeField] private float cayoteTimeMax;
    
    //Private variables
    private Vector3 _currentInputMovementDirection;
    private Vector3 _lastMovementDirection;
    
    //Timers
    private Timer _cayoteTimer;
    
    //requestFlags
    private bool _jumpRequested;
    private bool _hasJumped;
    private void Awake()
    {
        if (!mainCamera)
        {
            mainCamera = Camera.main;
        }
        motor.CharacterController = this;
        _cayoteTimer = new Timer(cayoteTimeMax);
    }

    private void Start()
    {
        inputDirection.OnValueChanged += SetCurrentMovementDirectionNormalized;
        jumpEvent.OnRaised += RequestJump;
        _cayoteTimer.OnTimerEnd += RevokeJumpRequest;
    }


    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (_lastMovementDirection.Equals(Vector3.zero)) return;
        Quaternion targetRotation = Quaternion.LookRotation(_lastMovementDirection);
        currentRotation = Quaternion.Slerp(currentRotation, targetRotation, deltaTime*rotationSpeed);
        characterVisual.rotation = currentRotation;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        
        //Variable Cache
        bool isStableOnGround = motor.GroundingStatus.IsStableOnGround;
        
        /* Update Timers */
        _cayoteTimer.Tick(deltaTime);
        
        /* Movement Sequence */
        if (isStableOnGround)
        {
            var targetVelocity = CalculateGroundMovementVelocity();
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, deltaTime * groundedAcceleration);
        }
        else
        {
            //TODO: In air control
            
            
            
            //Apply Gravity
            SimulateGravity(ref currentVelocity, deltaTime);
        }
        
        /* Jumping Sequence */
        
        if (isStableOnGround)
        {
            _hasJumped = false;
        }
        
        if (_jumpRequested && !_hasJumped && (isStableOnGround || _cayoteTimer.IsRunning))
        {
            PerformJump(ref currentVelocity);
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

    #region Player Actions

    private void PerformJump(ref Vector3 currentVelocity)
    {
        _hasJumped = true;
        _cayoteTimer.ForceEnd();

        motor.ForceUnground(time: 0f);
        var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
        var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
            
        currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
    }

    #endregion
    #region Calculations

    private Vector3 CalculateGroundMovementVelocity()
    {
        Vector3 cameraOrientedDirection = GetCameraOrientedDirectionFromInput();
            
        if(!cameraOrientedDirection.Equals(Vector3.zero))
            _lastMovementDirection = cameraOrientedDirection;
            
        var groundedMovement = motor.GetDirectionTangentToSurface(cameraOrientedDirection.normalized, motor.GroundingStatus.GroundNormal);
        var targetVelocity = groundedMovement * groundedSpeed;

        return targetVelocity;
    }

    private void SimulateGravity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (currentVelocity.y > 0)
        {
            currentVelocity += motor.CharacterUp * (gravity * initialJumpGravityMultiplier * deltaTime);
        }
        else
        {
            currentVelocity += motor.CharacterUp * (gravity * deltaTime);
        }
    }

    #endregion
    #region Utilities

    private Vector3 GetCameraOrientedDirectionFromInput()
    {
        float yaw = mainCamera.transform.eulerAngles.y;
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
        _cayoteTimer.Restart(cayoteTimeMax);
    }
    
    private void RevokeJumpRequest()
    {
        _jumpRequested = false;
    }
    #endregion
}


