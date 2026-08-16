using System;
using UnityEngine;

namespace GameStart.Town
{
    public class HouseCondition : MonoBehaviour
    {
        [SerializeField] private float maxCondition = 100f;
        [SerializeField] private float decayPerSecond = 0.5f;
        [SerializeField] private Renderer wallsRenderer;

        public event Action<float, float> ConditionChanged;

        public float MaxCondition => maxCondition;
        public float CurrentCondition { get; private set; }

        public void SetWallsRenderer(Renderer renderer)
        {
            wallsRenderer = renderer;
        }

        private void Awake()
        {
            CurrentCondition = maxCondition;
        }

        private void Update()
        {
            SetCondition(CurrentCondition - decayPerSecond * Time.deltaTime);
        }

        public void Repair(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            SetCondition(CurrentCondition + amount);
        }

        private void SetCondition(float value)
        {
            float clamped = Mathf.Clamp(value, 0f, maxCondition);
            if (Mathf.Approximately(clamped, CurrentCondition))
            {
                return;
            }

            CurrentCondition = clamped;
            ConditionChanged?.Invoke(CurrentCondition, maxCondition);

            if (wallsRenderer != null)
            {
                float t = maxCondition > 0f ? CurrentCondition / maxCondition : 0f;
                Color wellMaintained = new Color(0.65f, 0.55f, 0.4f);
                Color decayed = new Color(0.25f, 0.22f, 0.18f);
                wallsRenderer.sharedMaterial.color = Color.Lerp(decayed, wellMaintained, t);
            }
        }
    }
}
