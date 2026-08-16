using System;
using UnityEngine;

namespace GameStart.Player
{
    public class PlayerStamina : MonoBehaviour
    {
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float drainPerSecond = 20f;
        [SerializeField] private float regenPerSecond = 12f;
        [SerializeField] private float regenDelay = 1f;

        private float regenDelayTimer;

        public event Action<float, float> StaminaChanged;

        public float MaxStamina => maxStamina;
        public float CurrentStamina { get; private set; }
        public bool IsExhausted => CurrentStamina <= 0f;

        private void Awake()
        {
            CurrentStamina = maxStamina;
        }

        public void Drain(float deltaTime)
        {
            float previous = CurrentStamina;
            CurrentStamina = Mathf.Max(0f, CurrentStamina - drainPerSecond * deltaTime);
            regenDelayTimer = regenDelay;

            if (!Mathf.Approximately(previous, CurrentStamina))
            {
                StaminaChanged?.Invoke(CurrentStamina, maxStamina);
            }
        }

        public void Regen(float deltaTime)
        {
            if (regenDelayTimer > 0f)
            {
                regenDelayTimer -= deltaTime;
                return;
            }

            float previous = CurrentStamina;
            CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + regenPerSecond * deltaTime);

            if (!Mathf.Approximately(previous, CurrentStamina))
            {
                StaminaChanged?.Invoke(CurrentStamina, maxStamina);
            }
        }
    }
}
