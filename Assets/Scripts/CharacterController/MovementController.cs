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
        [SerializeField] private float airControlStrength;
        [SerializeField] private float maxAirSpeed;
        [SerializeField] private float gravity;
        [SerializeField] private float initialJumpGravityMultiplier;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float accelerationTiltSpeed;
        [SerializeField] private float accelerationTiltRecoverySpeed;
        [SerializeField] private float accelerationTiltDeadZone;
        [SerializeField] private float accelerationTiltFactor;

        [Header("Timer Settings")] 
        [SerializeField] private float cayoteTimeMax;

        [Header("Debug values")] 
        [ReadOnly] 
        [SerializeField]
        private float accelerationMagnitude;
        [ReadOnly] 
        [SerializeField]
        private Vector3 accelerationVector;
    
        //Private variables
        private Camera _mainCamera;
        private Vector3 _currentInputMovementDirection;
        private Vector3 _lastGroundDirection;
        private Vector3 _lastGroundVelocity;
    
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
            /* Character look rotation */
            
            if (_lastGroundDirection.Equals(Vector3.zero)) return;
            
            Quaternion targetRotation = Quaternion.LookRotation(_currentInputMovementDirection);
            currentRotation = Quaternion.Slerp(currentRotation, targetRotation, deltaTime*rotationSpeed);
            
            
            /* Character tilt */
            PerformTilt(ref currentRotation, deltaTime);
            
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
                _lastGroundVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
                var targetVelocity = CalculateGroundMovementVelocity();
                currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, deltaTime * groundedAcceleration);
                
            }
            else
            {
                //In air control
                Vector3 cameraOrientedDirection = GetCameraOrientedDirectionFromInput();
                if (cameraOrientedDirection.sqrMagnitude > 0f)
                {
                    var planarMovement = Vector3.ProjectOnPlane(cameraOrientedDirection, motor.CharacterUp) * cameraOrientedDirection.magnitude;
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
                
                // Vector3 inputDirection = GetCameraOrientedDirectionFromInput();
                //
                // if (!inputDirection.AproxEquals(Vector3.zero))
                // {
                //     Vector3 airMovement = inputDirection.normalized * (groundedSpeed * airControlStrength);
                //     _lastGroundDirection = new Vector3(currentVelocity.x, 0, currentVelocity.z);
                //     currentVelocity = Vector3.Lerp(currentVelocity, new Vector3(airMovement.x, currentVelocity.y, airMovement.z), deltaTime * airControlStrength);
                // }
            
            
            
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


