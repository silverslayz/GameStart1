using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameStart.CameraSystems
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraZoomController : MonoBehaviour
    {
        /// <summary>What the scroll wheel actually changes.</summary>
        public enum ZoomTarget
        {
            /// <summary>Pull the camera nearer or further on an orbital rig.</summary>
            OrbitDistance,

            /// <summary>Widen or narrow the lens, leaving the camera where it is.</summary>
            FieldOfView,
        }

        [Header("Zoom")]
        [Tooltip("Distance zoom moves the camera; field of view changes the lens instead.")]
        [SerializeField] private ZoomTarget zoomTarget = ZoomTarget.OrbitDistance;

        [Tooltip("Degrees (or units) per second at full scroll deflection.")]
        [SerializeField] private float zoomSpeed = 65f;
        [SerializeField] private float minOrthographicSize = 3f;
        [SerializeField] private float maxOrthographicSize = 12f;
        [Header("Field of view")]
        [SerializeField] private float minFieldOfView = 40f;
        [SerializeField] private float maxFieldOfView = 120f;
        [Header("Orbit distance")]
        [SerializeField] private float minOrbitRadius = 2f;
        [SerializeField] private float maxOrbitRadius = 12f;

        private CinemachineCamera cmCamera;
        private CinemachineOrbitalFollow orbitalFollow;

        private void Awake()
        {
            cmCamera = GetComponent<CinemachineCamera>();
            orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        }

        private void Update()
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

            // An orbital rig can zoom either way. Moving the camera keeps the lens honest
            // and is the better default, but widening the lens is what you want when the
            // framing matters more than the distance - so it's a choice, not an assumption.
            if (orbitalFollow != null && zoomTarget == ZoomTarget.OrbitDistance)
            {
                orbitalFollow.Radius = Mathf.Clamp(
                    orbitalFollow.Radius - scroll * zoomSpeed * Time.deltaTime,
                    minOrbitRadius,
                    maxOrbitRadius);
                return;
            }

            LensSettings lens = cmCamera.Lens;

            if (lens.Orthographic)
            {
                lens.OrthographicSize = Mathf.Clamp(
                    lens.OrthographicSize - scroll * zoomSpeed * Time.deltaTime,
                    minOrthographicSize,
                    maxOrthographicSize);
            }
            else
            {
                lens.FieldOfView = Mathf.Clamp(
                    lens.FieldOfView - scroll * zoomSpeed * Time.deltaTime,
                    minFieldOfView,
                    maxFieldOfView);
            }

            cmCamera.Lens = lens;
        }
    }
}
