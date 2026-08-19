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

        /// <summary>
        /// How much stamina must come back before sprinting is allowed again. Without a
        /// gap between "hit zero" and "can sprint again", the first frame of regen is
        /// immediately spent re-entering sprint, which drains it and restarts regenDelay -
        /// so holding sprint starves its own recovery forever.
        /// </summary>
        [SerializeField] private float exhaustionRecoveryThreshold = 25f;

        private float regenDelayTimer;
        private bool isExhausted;

        public event Action<float, float> StaminaChanged;

        public float MaxStamina => maxStamina;
        public float CurrentStamina { get; private set; }
        public bool IsExhausted => isExhausted;

        private void Awake()
        {
            CurrentStamina = maxStamina;
        }

        public void Drain(float deltaTime)
        {
            float previous = CurrentStamina;
            CurrentStamina = Mathf.Max(0f, CurrentStamina - drainPerSecond * deltaTime);
            regenDelayTimer = regenDelay;

            if (CurrentStamina <= 0f)
            {
                isExhausted = true;
            }

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

            // Latch clears only once there's enough back to be worth spending, so sprint
            // resumes in usable bursts rather than one frame at a time.
            if (isExhausted && CurrentStamina >= Mathf.Min(exhaustionRecoveryThreshold, maxStamina))
            {
                isExhausted = false;
            }

            if (!Mathf.Approximately(previous, CurrentStamina))
            {
                StaminaChanged?.Invoke(CurrentStamina, maxStamina);
            }
        }
    }
}
