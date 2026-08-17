using System.Collections;
using UnityEngine;
using GameStart.Dungeons;
using GameStart.Flow;

namespace GameStart.Rendering
{
    public class LightingTransition : MonoBehaviour
    {
        [SerializeField] private Renderer overlayRenderer;
        [SerializeField] private PlayerDungeonProgress dungeonProgress;
        [SerializeField] private ApexBoss apexBoss;
        [SerializeField] private float transitionDuration = 2f;

        private static readonly int BlendTId = Shader.PropertyToID("_BlendT");
        private Material materialInstance;
        private Coroutine activeTransition;

        private void Awake()
        {
            // Resolved here rather than in OnEnable, which subscribes to apexBoss and runs
            // immediately after Awake.
            overlayRenderer = SceneLink.ResolveCameraOverlay(overlayRenderer, "LightingTransitionQuad");
            apexBoss = SceneLink.Resolve(apexBoss);

            if (overlayRenderer != null)
            {
                materialInstance = overlayRenderer.material;
            }
        }

        private void OnEnable()
        {
            if (dungeonProgress != null)
            {
                dungeonProgress.DungeonEntered += OnDungeonEntered;
            }

            if (apexBoss != null)
            {
                apexBoss.BossDefeated += OnBossDefeated;
            }
        }

        private void OnDisable()
        {
            if (dungeonProgress != null)
            {
                dungeonProgress.DungeonEntered -= OnDungeonEntered;
            }

            if (apexBoss != null)
            {
                apexBoss.BossDefeated -= OnBossDefeated;
            }
        }

        private void OnDungeonEntered(int dungeonIndex) => TransitionTo(1f);
        private void OnBossDefeated() => TransitionTo(0f);

        public void TransitionTo(float target)
        {
            if (materialInstance == null)
            {
                return;
            }

            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
            }

            activeTransition = StartCoroutine(BlendRoutine(target));
        }

        private IEnumerator BlendRoutine(float target)
        {
            float start = materialInstance.GetFloat(BlendTId);
            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / transitionDuration);
                materialInstance.SetFloat(BlendTId, Mathf.Lerp(start, target, t));
                yield return null;
            }

            materialInstance.SetFloat(BlendTId, target);
        }
    }
}
