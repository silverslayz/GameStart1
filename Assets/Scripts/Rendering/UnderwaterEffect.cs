using UnityEngine;
using GameStart.Player;
using GameStart.Flow;

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
            // The quad is parented to the camera, not the player, so a prefab instance
            // starts with this null.
            overlayRenderer = SceneLink.ResolveCameraOverlay(overlayRenderer, "UnderwaterOverlayQuad");

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
