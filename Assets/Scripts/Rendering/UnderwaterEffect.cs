using UnityEngine;
using GameStart.Player;

namespace GameStart.Rendering
{
    public class UnderwaterEffect : MonoBehaviour
    {
        [SerializeField] private PlayerController controller;
        [SerializeField] private Renderer overlayRenderer;
        [SerializeField] private float fadeSpeed = 3f;

        private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
        private Material materialInstance;
        private float currentAlpha;

        private void Awake()
        {
            if (overlayRenderer != null)
            {
                materialInstance = overlayRenderer.material; // instances it
            }
        }

        private void Update()
        {
            if (controller == null || materialInstance == null)
            {
                return;
            }

            float target = controller.IsSwimming ? 1f : 0f;
            currentAlpha = Mathf.MoveTowards(currentAlpha, target, fadeSpeed * Time.deltaTime);
            materialInstance.SetFloat(AlphaId, currentAlpha);

            if (overlayRenderer.enabled != currentAlpha > 0.001f)
            {
                overlayRenderer.enabled = currentAlpha > 0.001f;
            }
        }
    }
}
