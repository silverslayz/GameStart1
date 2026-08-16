using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameStart.CameraSystems
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraZoomController : MonoBehaviour
    {
        [SerializeField] private float zoomSpeed = 65f;
        [SerializeField] private float minOrthographicSize = 3f;
        [SerializeField] private float maxOrthographicSize = 12f;
        [SerializeField] private float minFieldOfView = 20f;
        [SerializeField] private float maxFieldOfView = 70f;
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

            // Orbital rigs read as "zoom" through camera distance, not lens FOV/ortho size.
            if (orbitalFollow != null)
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
