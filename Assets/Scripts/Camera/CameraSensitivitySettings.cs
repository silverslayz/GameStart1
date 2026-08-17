using Unity.Cinemachine;
using UnityEngine;

namespace GameStart.CameraSystems
{
    public class CameraSensitivitySettings : MonoBehaviour
    {
        private const string PrefsKey = "CameraSensitivity";
        private const float DefaultSensitivity = 1f;
        public const float MinSensitivity = 0.25f;
        public const float MaxSensitivity = 3f;

        [SerializeField] private CinemachineInputAxisController axisController;

        private float[] baseGains;

        public float Sensitivity { get; private set; } = DefaultSensitivity;

        private void Awake()
        {
            if (axisController == null)
            {
                axisController = GetComponent<CinemachineInputAxisController>();
            }

            CacheBaseGains();
            Sensitivity = PlayerPrefs.GetFloat(PrefsKey, DefaultSensitivity);
            Apply();
        }

        private void CacheBaseGains()
        {
            if (axisController == null)
            {
                return;
            }

            baseGains = new float[axisController.Controllers.Count];
            for (int i = 0; i < axisController.Controllers.Count; i++)
            {
                baseGains[i] = axisController.Controllers[i].Input.Gain;
            }
        }

        public void SetSensitivity(float value)
        {
            Sensitivity = Mathf.Clamp(value, MinSensitivity, MaxSensitivity);
            PlayerPrefs.SetFloat(PrefsKey, Sensitivity);
            Apply();
        }

        private void Apply()
        {
            if (axisController == null || baseGains == null)
            {
                return;
            }

            for (int i = 0; i < axisController.Controllers.Count; i++)
            {
                InputAxisControllerBase<CinemachineInputAxisController.Reader>.Controller controller = axisController.Controllers[i];
                controller.Input.Gain = baseGains[i] * Sensitivity;
                axisController.Controllers[i] = controller;
            }
        }
    }
}
