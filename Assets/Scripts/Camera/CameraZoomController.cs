using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameStart.CameraSystems
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraZoomController : MonoBehaviour
    {
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minOrthographicSize = 3f;
        [SerializeField] private float maxOrthographicSize = 12f;
        [SerializeField] private float minFieldOfView = 20f;
        [SerializeField] private float maxFieldOfView = 70f;

        private CinemachineCamera cmCamera;

        private void Awake()
        {
            cmCamera = GetComponent<CinemachineCamera>();
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
