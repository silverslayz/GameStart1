using UnityEngine;
using UnityEngine.InputSystem;

namespace GameStart.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerStamina))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Speeds")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float jogSpeed = 5.5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float swimSpeed = 4.5f;
        [SerializeField] private float crouchSpeed = 2f;

        [Header("Jump & Gravity")]
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -20f;

        [Header("Camera")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController controller;
        private PlayerStamina stamina;
        private PlayerWeight weight;

        private Vector2 moveInput;
        private bool sprintHeld;
        private bool crouchHeld;
        private bool jumpRequested;
        private bool isSwimming;

        public bool IsCrouching => crouchHeld;
        public bool IsGrounded => controller.isGrounded;
        public float CurrentSpeed { get; private set; }
        public bool IsAscending => verticalVelocity.y > 0.01f && !controller.isGrounded;
        public bool IsFalling => verticalVelocity.y < -0.01f && !controller.isGrounded;

        private Vector3 verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            stamina = GetComponent<PlayerStamina>();
            weight = GetComponent<PlayerWeight>();

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (isSwimming)
            {
                MoveSwimming();
            }
            else
            {
                MoveOnTerrain();
            }
        }

        private void MoveOnTerrain()
        {
            Vector3 moveDirection = CameraRelativeDirection(moveInput);

            bool wantsSprint = !crouchHeld && sprintHeld && moveDirection.sqrMagnitude > 0f && !stamina.IsExhausted;
            float speed;
            if (crouchHeld)
            {
                speed = crouchSpeed;
            }
            else
            {
                speed = wantsSprint ? sprintSpeed : (sprintHeld ? jogSpeed : walkSpeed);
            }

            if (weight != null)
            {
                speed *= weight.SpeedMultiplier;
            }

            if (wantsSprint)
            {
                stamina.Drain(Time.deltaTime);
            }
            else
            {
                stamina.Regen(Time.deltaTime);
            }

            if (controller.isGrounded)
            {
                if (verticalVelocity.y < 0f)
                {
                    verticalVelocity.y = -2f;
                }

                if (jumpRequested)
                {
                    verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }

            jumpRequested = false;

            verticalVelocity.y += gravity * Time.deltaTime;

            Vector3 motion = moveDirection * speed + Vector3.up * verticalVelocity.y;
            controller.Move(motion * Time.deltaTime);

            CurrentSpeed = moveDirection.sqrMagnitude > 0f ? speed : 0f;

            if (moveDirection.sqrMagnitude > 0f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            }
        }

        private void MoveSwimming()
        {
            Vector3 forward = cameraTransform != null ? cameraTransform.forward : transform.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : transform.right;

            Vector3 moveDirection = (right * moveInput.x + forward * moveInput.y);
            if (moveDirection.sqrMagnitude > 1f)
            {
                moveDirection.Normalize();
            }

            stamina.Regen(Time.deltaTime);
            jumpRequested = false;

            controller.Move(moveDirection * swimSpeed * Time.deltaTime);

            CurrentSpeed = moveDirection.sqrMagnitude > 0f ? swimSpeed : 0f;

            if (moveDirection.sqrMagnitude > 0f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            }
        }

        private Vector3 CameraRelativeDirection(Vector2 input)
        {
            Vector3 forward = cameraTransform != null ? cameraTransform.forward : transform.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 direction = right * input.x + forward * input.y;
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        public void SetSwimming(bool swimming)
        {
            isSwimming = swimming;
            if (!swimming)
            {
                verticalVelocity.y = -2f;
            }
        }

        // Called automatically by PlayerInput (Behavior: Send Messages)
        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        public void OnSprint(InputValue value)
        {
            sprintHeld = value.isPressed;
        }

        public void OnCrouch(InputValue value)
        {
            crouchHeld = value.isPressed;
        }

        public void OnJump(InputValue value)
        {
            if (value.isPressed)
            {
                jumpRequested = true;
            }
        }
    }
}
