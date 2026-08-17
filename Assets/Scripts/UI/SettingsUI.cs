using GameStart.CameraSystems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameStart.UI
{
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private Text valueLabel;
        [SerializeField] private CameraSensitivitySettings sensitivitySettings;

        private bool isOpen;

        private void Start()
        {
            if (sensitivitySlider != null)
            {
                sensitivitySlider.minValue = CameraSensitivitySettings.MinSensitivity;
                sensitivitySlider.maxValue = CameraSensitivitySettings.MaxSensitivity;
                if (sensitivitySettings != null)
                {
                    sensitivitySlider.SetValueWithoutNotify(sensitivitySettings.Sensitivity);
                    UpdateLabel(sensitivitySettings.Sensitivity);
                }
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.oKey.wasPressedThisFrame)
            {
                isOpen = !isOpen;
                if (panel != null)
                {
                    panel.SetActive(isOpen);
                }
            }
        }

        public void OnSliderChanged(float value)
        {
            sensitivitySettings?.SetSensitivity(value);
            UpdateLabel(value);
        }

        private void UpdateLabel(float value)
        {
            if (valueLabel != null)
            {
                valueLabel.text = $"Camera Sensitivity: {value:0.00}x";
            }
        }
    }
}
