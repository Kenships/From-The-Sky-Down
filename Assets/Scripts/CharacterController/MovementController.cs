using KinematicCharacterController;
using UnityEngine;
using Utilities;
using ImprovedTimers;

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

        [SerializeField] private Animator animator;
    
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
        [SerializeField] private float bulletJumpSpeed;

        [Header("Timer Settings")] 
        [SerializeField] private float cayoteTimeMax;
        [SerializeField] private float jumpBufferTimeMax;
        [SerializeField] private float dashBufferTimeMax;
        [SerializeField] private float bulletJumpBufferTimeMax;

        [Header("Debug values")] 
        
        [ReadOnly, SerializeField]
        private float accelerationMagnitude;
        [ReadOnly, SerializeField]
        private Vector3 accelerationVector;
        [ReadOnly, SerializeField]
        private float planarSpeed;
        [ReadOnly, SerializeField]
        private PlayerState playerState;
        
    
        //Private variables
        private Camera _mainCamera;
        private Vector3 _cartesianInputMovementDirectionNormalized;
        private Vector3 _cameraOrientedInputDirectionNormalized;
        private Vector3 _lastGroundDirection;
        private Vector3 _lastGroundVelocity;
        private bool _isGrounded;
    
        //Timers
        private CountdownTimer _cayoteTimer;
        private CountdownTimer _jumpBufferTimer;
        private CountdownTimer _dashBufferTimer;
        private CountdownTimer _bulletJumpBufferTimer;
        private CountdownTimer _dashCooldownTimer;
        private CountdownTimer _bulletJumpCooldownTimer;
    
        //requestFlags
        private bool _jumpRequested;
        private bool _dashRequested;
        private bool _bulletJumpRequested;
     
        private void Awake()
        {
            _mainCamera = Camera.main;
            
            motor = gameObject.GetOrAdd<KinematicCharacterMotor>();
            motor.CharacterController = this;
            
            _cayoteTimer = new CountdownTimer(cayoteTimeMax);
            _jumpBufferTimer = new CountdownTimer(jumpBufferTimeMax);
            _dashBufferTimer = new CountdownTimer(dashBufferTimeMax);
            _bulletJumpBufferTimer = new CountdownTimer(bulletJumpBufferTimeMax);
            _dashCooldownTimer = new CountdownTimer(dashCooldown);
        }

        private void Start()
        {
            inputEvents.inputDirection.OnValueChanged += SetCurrentMovementDirectionNormalized;
            inputEvents.jumpEvent.OnRaised += RequestJump;
            inputEvents.dashEvent.OnRaised += RequestDash;
            inputEvents.bulletJumpEvent.OnRaised += RequestBulletJump;

            _cayoteTimer.OnTimerEnd += RevokeJumpRequest;
            _jumpBufferTimer.OnTimerEnd += RevokeJumpRequest;
            _dashBufferTimer.OnTimerEnd += RevokeDashRequest;
            _bulletJumpBufferTimer.OnTimerEnd += RevokeBulletJumpRequest;
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            /* Update Timers */
            _cayoteTimer.Tick();
            _jumpBufferTimer.Tick();
            _dashBufferTimer.Tick();
            _bulletJumpBufferTimer.Tick();

            _dashCooldownTimer.Tick();
        }
        
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            /* Character look rotation */
            
            if (_lastGroundDirection.Equals(Vector3.zero)) return;
            
            Quaternion targetRotation = Quaternion.LookRotation(_lastGroundDirection);
            currentRotation = Quaternion.Slerp(currentRotation, targetRotation, deltaTime*rotationSpeed);
            
            
            /* Character tilt */
            PerformTilt(ref currentRotation, deltaTime);
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            /*TODO: _lastGroundVelocity is kinda just used for the last input direction,
             but is also used for calculating tilt, so probably needs to be refactored sometime*/
            //Variable Cache
            CacheVelocityUpdatePerams(currentVelocity);

            /* Movement Sequence */
            ApplyDirectionalMovement(ref currentVelocity, deltaTime);
            
            /* Jumping & dashing Sequence */
            ApplyVelocityModifiers(ref currentVelocity);
        }
        

        public void PostGroundingUpdate(float deltaTime)
        {
            //Reset Cayote Timer
            if (motor.GroundingStatus.IsStableOnGround)
            {
                _cayoteTimer.Reset(cayoteTimeMax);
            }
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            //Animation Adjustment
            UpdateAnimations();

            //UpdatePlayerState
            UpdatePlayerState();
        }
        //TODO: Refactor animations to separate module
        private void UpdateAnimations()
        {
            float normalizedSpeed = motor.BaseVelocity.sqrMagnitude / (groundedSpeed * groundedSpeed);
            animator.SetFloat("Velocity", normalizedSpeed);
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
        
        #region Velocity Update Pipeline
        private void ApplyVelocityModifiers(ref Vector3 currentVelocity)
        {
            bool jumpThisFrame = _jumpRequested && (_isGrounded || _cayoteTimer.IsRunning) && playerState != PlayerState.Dashing;
            bool dashThisFrame = _dashRequested && _isGrounded && playerState != PlayerState.Dashing;
            bool bulletJumpThisFrame = _bulletJumpRequested && _isGrounded && playerState != PlayerState.Dashing;

            if (jumpThisFrame)
            {
                PerformJump(ref currentVelocity);
            }
            else if (dashThisFrame)
            {
                PerformDash(ref currentVelocity);
            }           
            else if (bulletJumpThisFrame)
            {
                PerformBulletJump(ref currentVelocity);
            }
        }

        private void ApplyDirectionalMovement(ref Vector3 currentVelocity, float deltaTime)
        {
            if (_isGrounded)
            {
                PerformGroundMovement(ref currentVelocity, deltaTime);
            }
            else
            {
                //In air control
                PerformAirMovement(ref currentVelocity, deltaTime);
                //Apply Gravity
                SimulateGravity(ref currentVelocity, deltaTime);
            }
        }

        private void CacheVelocityUpdatePerams(Vector3 currentVelocity)
        {
            _isGrounded = motor.GroundingStatus.IsStableOnGround;
            _cameraOrientedInputDirectionNormalized = GetCameraOrientedDirectionFromInput();
            _lastGroundVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
            planarSpeed = _lastGroundVelocity.magnitude;
        }
        #endregion
        
        #region Player Actions
        
        //TODO: Refactor State Calculations into separate module
        private void UpdatePlayerState()
        {
            Vector3 currentVelocity = motor.BaseVelocity;
            
            switch (playerState)
            {
                case PlayerState.Jumping:
                    if(currentVelocity.y < 0)
                    {
                        playerState = PlayerState.Falling;
                    }
                    break;
                case PlayerState.Falling:
                    if (_isGrounded)
                    {

                        playerState = currentVelocity.AproxEquals(Vector3.zero) ? PlayerState.Idle : PlayerState.Running;
                    }

                    break;
                case PlayerState.Dashing:
                    Debug.Log(_dashCooldownTimer.IsRunning);
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
        private void PerformGroundMovement(ref Vector3 currentVelocity, float deltaTime)
        {
           
            var targetVelocity = CalculateGroundMovementVelocityInDirection(_cameraOrientedInputDirectionNormalized);
            var transientDrag = playerState == PlayerState.Dashing ? dashDrag : groundedAcceleration;
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, deltaTime * transientDrag);
        }

        private void PerformAirMovement(ref Vector3 currentVelocity, float deltaTime)
        {
            if (Mathf.Approximately(_cameraOrientedInputDirectionNormalized.sqrMagnitude, 0)) return;
            
            
            var planarMovement = Vector3.ProjectOnPlane(_cameraOrientedInputDirectionNormalized, motor.CharacterUp) * _cameraOrientedInputDirectionNormalized.magnitude;
            var currentPlanarVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);
                    
            var movementForce = planarMovement * (airControlStrength * deltaTime);
            if (playerState == PlayerState.Dashing)
            {
                //Apply drag
                var planarVelocity = CalculateGroundMovementVelocityInDirection(_cameraOrientedInputDirectionNormalized);
                var targetVelocity = new Vector3(planarVelocity.x, currentVelocity.y, planarVelocity.z);
                currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, deltaTime * dashDrag);
            }
            else if (currentPlanarVelocity.sqrMagnitude < maxAirSpeed * maxAirSpeed)
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
        private void PerformJump(ref Vector3 currentVelocity)
        {
            playerState = PlayerState.Jumping;
            _cayoteTimer.Stop();

            motor.ForceUnground(time: 0f);
            var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
            
            currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
        }
        
        private void PerformDash(ref Vector3 currentVelocity)
        {
            _dashCooldownTimer.Reset(dashCooldown);
            playerState = PlayerState.Dashing;
            
            Vector3 planarGroundSpeed = new Vector3(_lastGroundDirection.x, 0, _lastGroundDirection.z);
            
            currentVelocity += planarGroundSpeed * dashSpeed;
        }

        private void PerformBulletJump(ref Vector3 currentVelocity)
        {
            //I used to have code here but it broke so now its just an mega jump dash
            PerformDash(ref currentVelocity);
            PerformJump(ref currentVelocity);
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
       
        #region Utilities

        private Vector3 GetCameraOrientedDirectionFromInput()
        {
            float yaw = _mainCamera.transform.eulerAngles.y;
            return Quaternion.Euler(0, yaw, 0) * _cartesianInputMovementDirectionNormalized;
        }
        
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
        
        #region Callback Functions
    
        private void SetCurrentMovementDirectionNormalized(Vector2 direction)
        {
            _cartesianInputMovementDirectionNormalized = new Vector3(direction.x, 0, direction.y).normalized;
            _cameraOrientedInputDirectionNormalized = GetCameraOrientedDirectionFromInput();
        }
        private void RequestJump()
        {
            _jumpBufferTimer.Reset(jumpBufferTimeMax);
            _jumpRequested = true;
        }
        
        private void RevokeJumpRequest()
        {
            _jumpRequested = false;
        }
        
        private void RequestDash()
        {
            _dashBufferTimer.Reset(dashBufferTimeMax);
            _dashRequested = true;
        }
        
        private void RevokeDashRequest()
        {
            _dashRequested = false;
        }

        private void RequestBulletJump()
        {
            _bulletJumpBufferTimer.Reset(bulletJumpBufferTimeMax);
            _bulletJumpRequested = true;
            
        }

        private void RevokeBulletJumpRequest()
        {
            _bulletJumpRequested = false;
        }

        #endregion
    }
}


