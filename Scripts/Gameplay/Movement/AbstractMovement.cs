using System.Linq;
using Gameplay.Character;
using Gameplay.Player.Effects;
using Photon.Pun;
using PlayVibe;
using UnityEngine;
using Zenject;

namespace Gameplay
{
    public abstract class AbstractMovement : MonoBehaviourPunCallbacks, IPunObservable
    {
        [SerializeField] protected AbstractCharacterView characterView;
        [SerializeField] protected float obstacleCheckDistance = 0.5f;
        [SerializeField] protected Transform groundCheckTransform;
        [SerializeField] protected LayerMask groundLayerMask;
        [SerializeField] protected float groundCheckDistance = 0.25f; 
        [SerializeField] protected float groundCheckSphereRadius = 0.2f;
        
        [Inject] protected Balance balance;
        [Inject] protected PopupService popupService;
        [Inject] protected EffectsSettings effectsSettings;
        
        protected Vector3 networkPosition;
        protected Quaternion networkRotation;
        protected Vector3 previousNetworkPosition;
        protected Quaternion previousNetworkRotation;
        protected float lastNetworkUpdateTime;
        protected float slowdown;
        protected float horizontalInput;
        protected float verticalInput;

        public StaminaHandler StaminaHandler { get; private set; }
        public LocationCameraController LocationCameraController { get; private set; }
        public bool IsGrounded { get; private set; }
        public float Stamina { get; private set; }
        public float LastSpeed { get; protected set; }

        private void Start()
        {
            StaminaHandler = new StaminaHandler(balance, popupService, characterView, effectsSettings);
        }

        private void FixedUpdate()
        {
            if (photonView.IsMine)
            {
                StaminaHandler.Update(LastSpeed > 0);
                
                CheckGrounded();
                HandleInput();
                CheckFall();

                if (LocationCameraController == null)
                {
                    return;
                }
                
                ApplyRotation();
                ApplyMovement();
                UpdateStamina();
                OnUpdate();
            }
            else
            {
                ApplyNetworkTransform();
            }
        }

        protected abstract void OnUpdate();
        
        private void HandleInput()
        {
            if (ChatPopup.InFocus)
            {
                return;
            }
            
            if (Input.GetKey(KeyCode.W))
            {
                verticalInput = 1f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                verticalInput = -1f;
            }

            if (Input.GetKey(KeyCode.D))
            {
                horizontalInput = 1f;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                horizontalInput = -1f;
            }
        }

        private void CheckGrounded()
        {
            IsGrounded = Physics.SphereCast(groundCheckTransform.position, groundCheckSphereRadius, Vector3.down, out _, groundCheckDistance, groundLayerMask);

            if (!IsGrounded)
            {
                return;
            }
            
            horizontalInput = 0;
            verticalInput = 0;
            slowdown = 1f;
        }

        private void CheckFall()
        {
            if (IsGrounded)
            {
                return;
            }
            
            slowdown = Mathf.Lerp(slowdown, 0.1f, Time.fixedDeltaTime * balance.Movement.FallSlowdownSpeed);

            horizontalInput *= slowdown;
            verticalInput *= slowdown;
        }

        private void ApplyRotation()
        {
            if (horizontalInput == 0 && verticalInput == 0)
            {
                return;
            }

            var cameraForward = LocationCameraController.transform.forward;
            var cameraRight = LocationCameraController.transform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;

            cameraForward.Normalize();
            cameraRight.Normalize();

            var movementDirection = cameraForward * verticalInput + cameraRight * horizontalInput;
            movementDirection.Normalize();

            if (movementDirection == Vector3.zero)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            var rotationSpeed = balance.Movement.RotationSpeed * Time.fixedDeltaTime;

            characterView.Rigidbody.rotation = Quaternion.Lerp(characterView.Rigidbody.rotation, targetRotation, rotationSpeed);
        }

        private void ApplyMovement()
        {
            if (horizontalInput == 0f && verticalInput == 0f)
            {
                LastSpeed = 0;
                return;
            }

            var cameraForward = LocationCameraController.transform.forward;
            var cameraRight = LocationCameraController.transform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;
            
            cameraForward.Normalize();
            cameraRight.Normalize();

            var movementDirection = cameraForward * verticalInput + cameraRight * horizontalInput;
            movementDirection.Normalize();

            if (movementDirection != Vector3.zero && !characterView.Rigidbody.isKinematic)
            {
                if (Physics.Raycast(characterView.Center.position, movementDirection, out var hitInfo, obstacleCheckDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
                {
                    if (Vector3.Dot(hitInfo.normal, Vector3.up) < 0.5f)
                    {
                        movementDirection = Vector3.ProjectOnPlane(movementDirection, hitInfo.normal).normalized;
                        
                        if (Physics.Raycast(characterView.Center.position, movementDirection, out var secondHitInfo, obstacleCheckDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
                        {
                            if (Vector3.Dot(secondHitInfo.normal, Vector3.up) < 0.5f)
                            {
                                horizontalInput = 0f;
                                verticalInput = 0f;
                                LastSpeed = 0;
                                
                                return;
                            }
                        }
                    }
                }
            }

            LastSpeed = StaminaHandler.CurrentSpeed;

            if (characterView.EffectsHandler.Data.ContainsKey(EffectType.Trap))
            {
                LastSpeed = 0;
            }
            
            characterView.Rigidbody.MovePosition(characterView.Rigidbody.position + movementDirection * (LastSpeed * Time.fixedDeltaTime));
        }
        
        private void UpdateStamina()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                Stamina = Mathf.Clamp(Stamina - Time.fixedDeltaTime, 0, balance.Movement.MaxStamina);
            }
            else
            {
                Stamina = Mathf.Clamp(Stamina + Time.fixedDeltaTime, 0, balance.Movement.MaxStamina);
            }
        }

        private void ApplyNetworkTransform()
        {
            var timeSinceLastUpdate = Time.time - lastNetworkUpdateTime;
            var predictedPosition = networkPosition + (networkPosition - previousNetworkPosition) * timeSinceLastUpdate;
            var predictedRotation = Quaternion.Slerp(previousNetworkRotation, networkRotation, timeSinceLastUpdate);

            characterView.Rigidbody.position = Vector3.Lerp(characterView.Rigidbody.position, predictedPosition, balance.Movement.NetworkLerpMoveSpeed * Time.fixedDeltaTime);
            characterView.Rigidbody.rotation = Quaternion.Lerp(characterView.Rigidbody.rotation, predictedRotation, balance.Movement.NetworkLerpRotationSpeed * Time.fixedDeltaTime);
        }

        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(characterView.Rigidbody.position);
                stream.SendNext(characterView.Rigidbody.rotation);
            }
            else
            {
                previousNetworkPosition = networkPosition;
                previousNetworkRotation = networkRotation;
                networkPosition = (Vector3)stream.ReceiveNext();
                networkRotation = (Quaternion)stream.ReceiveNext();
                lastNetworkUpdateTime = Time.time;
            }
        }

        public void SetCamera(LocationCameraController camera)
        {
            LocationCameraController = camera;
        }
    }
}
