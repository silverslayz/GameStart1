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
        [SerializeField] private float sprintSpeed = 14f;
        [SerializeField] private float swimSpeed = 4.5f;
        [SerializeField] private float crouchSpeed = 2f;

        [Header("Jump & Gravity")]
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -20f;

        [Header("Camera")]
        [SerializeField] private Transform cameraTransform;

        private const string SprintActionName = "Sprint";
        private const string CrouchActionName = "Crouch";

        private CharacterController controller;
        private PlayerStamina stamina;
        private PlayerWeight weight;
        private InputAction sprintAction;
        private InputAction crouchAction;

        private Vector2 moveInput;
        private bool sprintHeld;
        private bool crouchHeld;
        private bool jumpRequested;
        private bool isSwimming;

        public bool IsCrouching => crouchHeld;
        public bool IsSwimming => isSwimming;
        public bool IsGrounded => controller.isGrounded;
        public float CurrentSpeed { get; private set; }

        /// <summary>Exposed so the animator bridge can map real speed onto the blend tree.</summary>
        public float WalkSpeed => walkSpeed;
        public float SprintSpeed => sprintSpeed;
        public bool IsAscending => verticalVelocity.y > 0.01f && !controller.isGrounded;
        public bool IsFalling => verticalVelocity.y < -0.01f && !controller.isGrounded;

        private Vector3 verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            stamina = GetComponent<PlayerStamina>();
            weight = GetComponent<PlayerWeight>();

            // Held buttons are polled rather than driven by OnSprint/OnCrouch messages.
            // PlayerInput's Send Messages only forwards `canceled` for Value-type actions
            // (PlayerInput.cs: "ATM we only care about performed and, in the case of value
            // actions, canceled"), and these are Buttons - so the release never arrives and
            // the flag latches on, turning hold into toggle.
            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null && playerInput.actions != null)
            {
                sprintAction = playerInput.actions.FindAction(SprintActionName);
                crouchAction = playerInput.actions.FindAction(CrouchActionName);
            }

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            ReadHeldButtons();

            if (isSwimming)
            {
                MoveSwimming();
            }
            else
            {
                MoveOnTerrain();
            }
        }

        /// <summary>
        /// Authoritative state for buttons whose meaning is "while held". Falls back to the
        /// message-driven flags if the action can't be resolved, so a rig without PlayerInput
        /// still moves.
        /// </summary>
        private void ReadHeldButtons()
        {
            if (sprintAction != null)
            {
                sprintHeld = sprintAction.IsPressed();
            }

            if (crouchAction != null)
            {
                crouchHeld = crouchAction.IsPressed();
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

        // Kept as the fallback path for rigs without a resolvable Sprint/Crouch action.
        // ReadHeldButtons overrides these whenever the actions are available, because the
        // release half of these messages is never sent for Button actions.
        public void OnSprint(InputValue value)
        {
            if (sprintAction == null)
            {
                sprintHeld = value.isPressed;
            }
        }

        public void OnCrouch(InputValue value)
        {
            if (crouchAction == null)
            {
                crouchHeld = value.isPressed;
            }
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
