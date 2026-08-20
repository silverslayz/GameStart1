using UnityEngine;
using UnityEngine.InputSystem;

namespace GameStart.CameraSystems
{
    /// <summary>
    /// Turns the player's camera target with the Look input.
    ///
    /// An over-the-shoulder rig takes its orientation from the transform it follows rather
    /// than orbiting on its own axes, so something has to turn that transform - this is the
    /// same job the Starter Assets controller does for its camera, kept as its own component
    /// so it can be tuned without touching movement.
    /// </summary>
    public class PlayerCameraLook : MonoBehaviour
    {
        [Tooltip("Transform the camera follows. Found by name under the player when left empty.")]
        [SerializeField] private Transform cameraTarget;

        [Header("Sensitivity")]
        [SerializeField] private float horizontalSensitivity = 0.12f;
        [SerializeField] private float verticalSensitivity = 0.1f;
        [SerializeField] private bool invertVertical;

        [Header("Pitch limits")]
        [Tooltip("How far the camera can look down and up, in degrees.")]
        [SerializeField] private float minPitch = -25f;
        [SerializeField] private float maxPitch = 70f;

        private const string TargetName = "PlayerCameraRoot";
        private const string LookActionName = "Look";

        private InputAction lookAction;
        private float yaw;
        private float pitch;

        private void Awake()
        {
            if (cameraTarget == null)
            {
                cameraTarget = FindDeep(transform, TargetName);
            }

            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null && playerInput.actions != null)
            {
                lookAction = playerInput.actions.FindAction(LookActionName);
            }

            if (cameraTarget != null)
            {
                Vector3 angles = cameraTarget.eulerAngles;
                yaw = angles.y;
                pitch = NormalisePitch(angles.x);
            }
        }

        private void LateUpdate()
        {
            if (cameraTarget == null || lookAction == null)
            {
                return;
            }

            // Nothing to do while a menu owns the cursor - and without this the camera
            // spins as the pointer crosses the screen to click a button.
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            Vector2 look = lookAction.ReadValue<Vector2>();
            if (look.sqrMagnitude < 0.0001f)
            {
                return;
            }

            // Mouse delta is already per-frame, so this must not be scaled by deltaTime:
            // doing so makes sensitivity depend on frame rate.
            yaw += look.x * horizontalSensitivity;
            pitch += look.y * verticalSensitivity * (invertVertical ? 1f : -1f);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private static float NormalisePitch(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name)
            {
                return root;
            }

            foreach (Transform child in root)
            {
                Transform found = FindDeep(child, name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
