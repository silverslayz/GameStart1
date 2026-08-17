using Unity.Cinemachine;
using UnityEngine;

namespace GameStart.CameraSystems
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraFovSettings : MonoBehaviour
    {
        private const string PrefsKey = "CameraFieldOfView";
        public const float MinFov = 40f;
        public const float MaxFov = 90f;

        private CinemachineCamera cmCamera;
        private float defaultFov;

        public float FieldOfView { get; private set; }

        private void Awake()
        {
            cmCamera = GetComponent<CinemachineCamera>();
            defaultFov = cmCamera.Lens.FieldOfView;

            FieldOfView = PlayerPrefs.GetFloat(PrefsKey, defaultFov);
            Apply();
        }

        public void SetFieldOfView(float value)
        {
            FieldOfView = Mathf.Clamp(value, MinFov, MaxFov);
            PlayerPrefs.SetFloat(PrefsKey, FieldOfView);
            Apply();
        }

        private void Apply()
        {
            LensSettings lens = cmCamera.Lens;
            lens.FieldOfView = FieldOfView;
            cmCamera.Lens = lens;
        }
    }
}
