using UnityEngine;
using UnityEngine.UI;
using GameStart.CameraSystems;

namespace GameStart.UI
{
    // A lighter-weight sibling of SettingsUI for the title screen, where no
    // CinemachineCamera exists yet to bind CameraSensitivitySettings/CameraFovSettings to.
    // Reads and writes the same PlayerPrefs keys directly - CameraSensitivitySettings
    // and CameraFovSettings pick these up on Awake() once the gameplay scene loads.
    public class TitleSettingsUI : MonoBehaviour
    {
        private const string SensitivityKey = "CameraSensitivity";
        private const string FovKey = "CameraFieldOfView";
        private const float DefaultSensitivity = 1f;
        private const float DefaultFov = 40f;

        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private Slider fovSlider;

        private void OnEnable()
        {
            if (sensitivitySlider != null)
            {
                sensitivitySlider.minValue = CameraSensitivitySettings.MinSensitivity;
                sensitivitySlider.maxValue = CameraSensitivitySettings.MaxSensitivity;
                sensitivitySlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(SensitivityKey, DefaultSensitivity));
            }

            if (fovSlider != null)
            {
                fovSlider.minValue = CameraFovSettings.MinFov;
                fovSlider.maxValue = CameraFovSettings.MaxFov;
                fovSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(FovKey, DefaultFov));
            }
        }

        public void OnSensitivityChanged(float value)
        {
            PlayerPrefs.SetFloat(SensitivityKey, value);
        }

        public void OnFovChanged(float value)
        {
            PlayerPrefs.SetFloat(FovKey, value);
        }
    }
}
