using UnityEngine;
using UnityEngine.UI;

namespace GameStart.UI
{
    [RequireComponent(typeof(Image))]
    public class StatusBar : MonoBehaviour
    {
        private Image fillImage;

        private Image FillImage => fillImage != null ? fillImage : (fillImage = GetComponent<Image>());

        public void SetValue(float current, float max)
        {
            FillImage.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        }
    }
}
