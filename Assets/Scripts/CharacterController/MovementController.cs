using KinematicCharacterController;
using Obvious.Soap;
using UnityEngine;
using Utilities;

namespace CharacterController
{
    public class MovementController : MonoBehaviour, ICharacterController
    {
        [Header("Soap References")]
        [SerializeField] private Vector2Variable inputDirection;
        [SerializeField] private ScriptableEventNoParam jumpEvent;
    
        [Header("Other References")]
        [SerializeField] private KinematicCharacterMotor motor;
        
        [SerializeField] private Transform characterVisual;
    
        [Header("Movement Settings")]
        [SerializeField] private float groundedSpeed;
        [SerializeField] private float groundedAcceleration;
        [SerializeField] private float jumpSpeed;
        [SerializeField] private float gravity;
        [SerializeField] private float initialJumpGravityMultiplier;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float accelerationTiltSpeed;
        [SerializeField] private float accelerationTiltRecoverySpeed;
        [SerializeField] private float accelerationTiltFactor;

        [Header("Timer Settings")] 
        [SerializeField] private float cayoteTimeMax;
    
        //Private variables
        private Camera _mainCamera;
        private Vector3 _currentInputMovementDirection;
        private Vector3 _lastGroundDirection;
        private Vector3 _lastGroundVelocity;
        private Quaternion _lastGroundRotation;
    
        //Timers
        private Timer _cayoteTimer;
    
        //requestFlags
        private bool _jumpRequested;
        private bool _hasJumped;
        private void Awake()
        {
            _mainCamera = Camera.main;
            
            motor = gameObject.GetOrAdd<KinematicCharacterMotor>();
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
            _lastGroundRotation = currentRotation;
            /* Character look rotation */
            
            if (_lastGroundDirection.Equals(Vector3.zero)) return;
            Quaternion targetRotation = Quaternion.LookRotation(_lastGroundDirection);
            currentRotation = Quaternion.Slerp(currentRotation, targetRotation, deltaTime*rotationSpeed);
            characterVisual.rotation = currentRotation;
            
            /* Character tilt */

            if (motor.GroundingStatus.IsStableOnGround)
            {
                PerformTilt(ref currentRotation, deltaTime);
            }
            else
            {
                Quaternion uprightRotation = new Quaternion(0f, currentRotation.y, 0f, currentRotation.w);
                currentRotation = Quaternion.Slerp(currentRotation, uprightRotation, deltaTime * accelerationTiltRecoverySpeed);
            }
            
            
            
            
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
                _lastGroundVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
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

        private void PerformTilt(ref Quaternion currentRotation, float deltaTime)
        {
            Vector3 accel = (motor.BaseVelocity - _lastGroundVelocity) / deltaTime;
            
            Vector3 upWithLean = (Vector3.up + accel * accelerationTiltFactor).normalized;
            
            Vector3 flatForward = Vector3.ProjectOnPlane(motor.CharacterForward, Vector3.up).normalized;
            
            Quaternion targetRot = Quaternion.LookRotation(flatForward, upWithLean);

            if (!accel.AproxEquals(Vector3.zero))
            {
                currentRotation = Quaternion.Slerp(currentRotation, targetRot, Time.deltaTime * accelerationTiltSpeed);
            }
            else
            {
                currentRotation = Quaternion.Slerp(currentRotation, targetRot, Time.deltaTime * accelerationTiltRecoverySpeed);
            }
            
            
        }
        #endregion
        #region Calculations

        private Vector3 CalculateGroundMovementVelocity()
        {
            Vector3 cameraOrientedDirection = GetCameraOrientedDirectionFromInput();
            
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
            return Quaternion.Euler(0, yaw, 0) * _currentInputMovementDirection;
        }

        #endregion
        #region Callback Functions
    
        private void SetCurrentMovementDirectionNormalized(Vector2 direction)
        {
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
}


