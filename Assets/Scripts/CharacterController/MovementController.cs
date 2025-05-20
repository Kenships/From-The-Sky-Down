using KinematicCharacterController;
using Obvious.Soap;
using UnityEngine;
using Utilities;

namespace CharacterController
{
    public enum PlayerState
    {
        Idle,
        Running,
        Jumping,
        Falling,
        Dashing
    }
    public class MovementController : MonoBehaviour, ICharacterController
    {
        [Header("Input References")]
        [SerializeField] private InputEventsSO inputEvents;
    
        [Header("Other References")]
        [SerializeField] private KinematicCharacterMotor motor;
        
        [SerializeField] private Transform characterVisual;
    
        [Header("Movement Settings")]
        [SerializeField] private float groundedSpeed;
        [SerializeField] private float groundedAcceleration;
        [SerializeField] private float jumpSpeed;
        [SerializeField] private float airControlStrength;
        [SerializeField] private float maxAirSpeed;
        [SerializeField] private float gravity;
        [SerializeField] private float initialJumpGravityMultiplier;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float accelerationTiltSpeed;
        [SerializeField] private float accelerationTiltRecoverySpeed;
        [SerializeField] private float accelerationTiltDeadZone;
        [SerializeField] private float accelerationTiltFactor;
        [SerializeField] private float dashSpeed;
        [SerializeField] private float dashDrag;
        [SerializeField] private float dashCooldown;

        [Header("Timer Settings")] 
        [SerializeField] private float cayoteTimeMax;
        [SerializeField] private float jumpBufferTimeMax;
        [SerializeField] private float dashBufferTimeMax;

        [Header("Debug values")] 
        [ReadOnly] 
        [SerializeField]
        private float accelerationMagnitude;
        [ReadOnly] 
        [SerializeField]
        private Vector3 accelerationVector;
        [ReadOnly] 
        [SerializeField]
        private PlayerState playerState;
    
        //Private variables
        private Camera _mainCamera;
        private Vector3 _currentInputMovementDirectionNormalized;
        private Vector3 _lastGroundDirection;
        private Vector3 _lastGroundVelocity;
    
        //Timers
        private Timer _cayoteTimer;
        private Timer _jumpBufferTimer;
        private Timer _dashBufferTimer;
        private Timer _dashCooldownTimer;
    
        //requestFlags
        private bool _jumpRequested;
        private bool _dashRequested;
     
        private void Awake()
        {
            _mainCamera = Camera.main;
            
            motor = gameObject.GetOrAdd<KinematicCharacterMotor>();
            motor.CharacterController = this;
            
            _cayoteTimer = new Timer(cayoteTimeMax);
            _jumpBufferTimer = new Timer(jumpBufferTimeMax);
            _dashBufferTimer = new Timer(dashBufferTimeMax);
            _dashCooldownTimer = new Timer(dashCooldown);
        }

        private void Start()
        {
            inputEvents.inputDirection.OnValueChanged += SetCurrentMovementDirectionNormalized;
            inputEvents.jumpEvent.OnRaised += RequestJump;
            inputEvents.dashEvent.OnRaised += RequestDash;
            _cayoteTimer.OnTimerEnd += RevokeJumpRequest;
            _jumpBufferTimer.OnTimerEnd += RevokeJumpRequest;
            _dashBufferTimer.OnTimerEnd += RevokeDashRequest;
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            /* Update Timers */
            _cayoteTimer.Tick(deltaTime);
            _jumpBufferTimer.Tick(deltaTime);
            _dashBufferTimer.Tick(deltaTime);
            _dashCooldownTimer.Tick(deltaTime);
        }
        
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            /* Character look rotation */
            
            if (_lastGroundDirection.Equals(Vector3.zero)) return;
            
            Quaternion targetRotation = Quaternion.LookRotation(_lastGroundDirection);
            currentRotation = Quaternion.Slerp(currentRotation, targetRotation, deltaTime*rotationSpeed);
            
            
            /* Character tilt */
            PerformTilt(ref currentRotation, deltaTime);
            
            characterVisual.rotation = currentRotation;
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            /*TODO: _lastGroundVelocity is kinda just used for the last input direction,
             but is also used for calculating tilt, so probably needs to be refactored sometime*/
        
            //Variable Cache
            bool isStableOnGround = motor.GroundingStatus.IsStableOnGround;
            Vector3 cameraOrientedInput = GetCameraOrientedDirectionFromInput();
        
            /* Movement Sequence */
            _lastGroundVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
            if (isStableOnGround)
            {
                //Reset buffer timers
                _cayoteTimer.Restart(cayoteTimeMax);

                if (playerState == PlayerState.Dashing)
                {
                    //Apply drag
                    var targetVelocity = CalculateGroundMovementVelocityInDirection(cameraOrientedInput);
                    currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, deltaTime * dashDrag);
                }
                else
                {
                    var targetVelocity = CalculateGroundMovementVelocityInDirection(cameraOrientedInput);
                    currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, deltaTime * groundedAcceleration);
                }
            }
            else
            {
                //In air control
                if (cameraOrientedInput.sqrMagnitude > 0f)
                {
                    var planarMovement = Vector3.ProjectOnPlane(cameraOrientedInput, motor.CharacterUp) * cameraOrientedInput.magnitude;
                    var currentPlanarVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);
                    
                    var movementForce = planarMovement * (airControlStrength * deltaTime);

                    if (currentPlanarVelocity.sqrMagnitude < maxAirSpeed * maxAirSpeed)
                    {
                        var targetPlanarVelocity = currentPlanarVelocity + movementForce;
                    
                        targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, maxAirSpeed);
                        
                        movementForce = targetPlanarVelocity - currentPlanarVelocity;
                    }
                    else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f)
                    {
                        var constrainedMovementForce = Vector3.ProjectOnPlane(movementForce, currentPlanarVelocity.normalized);
                        
                        movementForce = constrainedMovementForce;
                    }
                    
                    
                    currentVelocity += movementForce;
                }
                //Apply Gravity
                SimulateGravity(ref currentVelocity, deltaTime);
            }
        
            
            /* Jumping & dashing Sequence */
            bool jumpThisFrame = _jumpRequested && (isStableOnGround || _cayoteTimer.IsRunning) && playerState != PlayerState.Dashing;
            bool dashThisFrame = _dashRequested && isStableOnGround && playerState != PlayerState.Dashing;

            if (jumpThisFrame)
            {
                PerformJump(ref currentVelocity);
            }
            else if (dashThisFrame)
            {
                PerformDash(ref currentVelocity);
            }

            UpdatePlayerState(currentVelocity, isStableOnGround);
        }

        private void UpdatePlayerState(Vector3 currentVelocity, bool isStableOnGround)
        {
            switch (playerState)
            {
                case PlayerState.Jumping:
                    if(currentVelocity.y < 0)
                    {
                        playerState = PlayerState.Falling;
                    }
                    break;
                case PlayerState.Falling:
                    if (isStableOnGround)
                    {
                        playerState = currentVelocity.AproxEquals(Vector3.zero) ? PlayerState.Idle : PlayerState.Running;
                    }

                    break;
                case PlayerState.Dashing:
                    if (!_dashCooldownTimer.IsRunning)
                    {
                        playerState = currentVelocity.AproxEquals(Vector3.zero) ? PlayerState.Idle : PlayerState.Running;
                    }
                    break;
                default:
                    playerState = currentVelocity.AproxEquals(Vector3.zero) ? PlayerState.Idle : PlayerState.Running;
                    break;
            }
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
            playerState = PlayerState.Jumping;
            _cayoteTimer.ForceEnd();

            motor.ForceUnground(time: 0f);
            var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
            
            currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
        }
        
        private void PerformDash(ref Vector3 currentVelocity)
        {
            _dashCooldownTimer.Restart(dashCooldown);
            playerState = PlayerState.Dashing;
            currentVelocity += _lastGroundDirection * dashSpeed;
        }

        private void PerformTilt(ref Quaternion currentRotation, float deltaTime)
        {
            Vector3 motorVelocity = motor.BaseVelocity;
            
            Vector3 motorGroundVelocity = new Vector3(motorVelocity.x, 0, motorVelocity.z);
            
            Vector3 accel = (motorGroundVelocity - _lastGroundVelocity) / deltaTime;

            accelerationVector = accel;
            accelerationMagnitude = accelerationVector.magnitude;
            
            Vector3 upWithLean = (Vector3.up + accel * accelerationTiltFactor).normalized;
            
            Vector3 flatForward = Vector3.ProjectOnPlane(motor.CharacterForward, Vector3.up).normalized;
            
            Debug.DrawLine(transform.position, transform.position + accel, Color.red);
            Debug.DrawLine(transform.position, transform.position + motorGroundVelocity, Color.green);
            
            Quaternion targetRot = Quaternion.LookRotation(flatForward, upWithLean);
            
            Quaternion uprightRotation = new Quaternion(0f, currentRotation.y, 0f, currentRotation.w);
            
            float angle = Quaternion.Angle(targetRot, uprightRotation);

            if (angle > accelerationTiltDeadZone)
            {
                float tiltSpeed = motor.GroundingStatus.IsStableOnGround ? accelerationTiltSpeed : accelerationTiltRecoverySpeed;
                currentRotation = Quaternion.Slerp(currentRotation, targetRot, deltaTime * tiltSpeed);
            }
            
            
        }
        #endregion
        #region Calculations

        private Vector3 CalculateGroundMovementVelocityInDirection(Vector3 cameraOrientedDirection)
        {
            if(!cameraOrientedDirection.Equals(Vector3.zero))
                _lastGroundDirection = cameraOrientedDirection;
            
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
            float yaw = _mainCamera.transform.eulerAngles.y;
            return Quaternion.Euler(0, yaw, 0) * _currentInputMovementDirectionNormalized;
        }

        #endregion
        #region Callback Functions
    
        private void SetCurrentMovementDirectionNormalized(Vector2 direction)
        {
            _currentInputMovementDirectionNormalized = new Vector3(direction.x, 0, direction.y).normalized;
        }
        private void RequestJump()
        {
            _jumpBufferTimer.Restart(jumpBufferTimeMax);
            _jumpRequested = true;
        }
        
        private void RevokeJumpRequest()
        {
            _jumpRequested = false;
        }
        
        private void RequestDash()
        {
            _dashBufferTimer.Restart(dashBufferTimeMax);
            _dashRequested = true;
        }
        
        private void RevokeDashRequest()
        {
            _dashRequested = false;
        }
        #endregion
    }
}


