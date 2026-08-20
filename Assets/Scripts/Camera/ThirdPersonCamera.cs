using UnityEngine;
using UnityEngine.InputSystem;

namespace GameStart.CameraSystems
{
    /// <summary>
    /// A third-person camera that orbits the player, zooms on the wheel, and stays out of
    /// walls. Deliberately self-contained: it drives the Camera transform directly rather
    /// than through Cinemachine.
    ///
    /// The Cinemachine rigs this replaces failed for reasons that had nothing to do with
    /// how a camera should behave - an orbital rig whose axes needed separately-bound input
    /// actions, then a third-person rig whose look depended on another script that silently
    /// did nothing because the scene never locked the cursor. Every one of those failure
    /// modes is a piece of wiring that can go missing. This has no wiring: drop it on the
    /// camera, and it finds the player itself.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("What to orbit. Found automatically from the player if left empty.")]
        [SerializeField] private Transform target;

        [Tooltip("Height above the target's pivot to look at - roughly chest or head height.")]
        [SerializeField] private float targetHeight = 1.5f;

        [Header("Zoom (mouse wheel)")]
        [SerializeField] private float distance = 6f;
        [SerializeField] private float minDistance = 1.5f;
        [SerializeField] private float maxDistance = 14f;
        [SerializeField] private float zoomSpeed = 1.6f;
        [Tooltip("How quickly the camera settles into a new zoom distance. 0 is instant.")]
        [SerializeField] private float zoomSmoothing = 0.12f;

        [Header("Look")]
        [SerializeField] private float yawSensitivity = 0.16f;
        [SerializeField] private float pitchSensitivity = 0.13f;
        [SerializeField] private bool invertVertical;
        [SerializeField] private float minPitch = -25f;
        [SerializeField] private float maxPitch = 70f;

        [Header("Collision")]
        [Tooltip("What the camera refuses to pass through. Leave the player's own layer out.")]
        [SerializeField] private LayerMask obstacles = ~0;
        [SerializeField] private float obstaclePadding = 0.25f;

        [Header("Cursor")]
        [Tooltip("Locks and hides the cursor on start. Turn off if a menu owns the cursor.")]
        [SerializeField] private bool captureCursor = true;

        private float yaw;
        private float pitch = 20f;
        private float currentDistance;
        private float zoomVelocity;

        private void Awake()
        {
            currentDistance = distance;

            if (target == null)
            {
                target = FindPlayer();
            }
        }

        private void Start()
        {
            if (captureCursor)
            {
                // Done here rather than relying on some other screen to have done it. The
                // previous camera read as broken purely because nothing in this scene ever
                // locked the cursor, so its look input was ignored on every frame.
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (target != null)
            {
                yaw = target.eulerAngles.y;
                Reposition(Pivot());
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                target = FindPlayer();
                if (target == null)
                {
                    return;
                }
            }

            ReadZoom();
            ReadLook();

            currentDistance = zoomSmoothing > 0f
                ? Mathf.SmoothDamp(currentDistance, distance, ref zoomVelocity, zoomSmoothing)
                : distance;

            Reposition(Pivot());
        }

        /// <summary>The point being orbited: the target's position lifted to eye level.</summary>
        private Vector3 Pivot()
        {
            return target.position + Vector3.up * targetHeight;
        }

        private void ReadZoom()
        {
            if (Mouse.current == null)
            {
                return;
            }

            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Approximately(scroll, 0f))
            {
                return;
            }

            // Wheel deltas arrive as chunky steps (120 per notch on Windows), so this is
            // normalised rather than scaled by the raw value or by deltaTime.
            distance = Mathf.Clamp(distance - Mathf.Sign(scroll) * zoomSpeed, minDistance, maxDistance);
        }

        private void ReadLook()
        {
            if (Mouse.current == null || !Mouse.current.rightButton.isPressed && Cursor.lockState != CursorLockMode.Locked)
            {
                // Free cursor: only orbit while the right button is held, so clicking around
                // a menu doesn't spin the camera.
                return;
            }

            Vector2 delta = Mouse.current.delta.ReadValue();
            if (delta.sqrMagnitude < 0.0001f)
            {
                return;
            }

            // Mouse delta is already per-frame. Scaling it by deltaTime would make
            // sensitivity depend on frame rate.
            yaw += delta.x * yawSensitivity;
            pitch += delta.y * pitchSensitivity * (invertVertical ? 1f : -1f);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        private void Reposition(Vector3 pivot)
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 wanted = pivot - rotation * Vector3.forward * currentDistance;

            // Pull in rather than clip through whatever is between the player and the
            // camera. A sphere rather than a ray so it reacts before the near plane does.
            Vector3 toCamera = wanted - pivot;
            if (Physics.SphereCast(pivot, obstaclePadding, toCamera.normalized, out RaycastHit hit,
                    toCamera.magnitude, obstacles, QueryTriggerInteraction.Ignore))
            {
                wanted = pivot + toCamera.normalized * Mathf.Max(hit.distance - obstaclePadding, 0.1f);
            }

            transform.SetPositionAndRotation(wanted, rotation);
        }

        private static Transform FindPlayer()
        {
            var controller = Object.FindAnyObjectByType<CharacterController>(FindObjectsInactive.Exclude);
            return controller == null ? null : controller.transform;
        }
    }
}
