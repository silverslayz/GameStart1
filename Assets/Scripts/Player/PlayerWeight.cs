using System;
using UnityEngine;

namespace GameStart.Player
{
    public class PlayerWeight : MonoBehaviour
    {
        [SerializeField] private float maxCapacity = 50f;
        [SerializeField] private float overEncumberedSpeedMultiplier = 0.5f;

        public event Action<float, float> WeightChanged;

        public float MaxCapacity => maxCapacity;
        public float CurrentWeight { get; private set; }
        public bool IsOverEncumbered => CurrentWeight > maxCapacity;
        public float SpeedMultiplier => IsOverEncumbered ? overEncumberedSpeedMultiplier : 1f;

        public void AddWeight(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            SetWeight(CurrentWeight + amount);
        }

        public void RemoveWeight(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            SetWeight(Mathf.Max(0f, CurrentWeight - amount));
        }

        private void SetWeight(float value)
        {
            if (Mathf.Approximately(value, CurrentWeight))
            {
                return;
            }

            CurrentWeight = value;
            WeightChanged?.Invoke(CurrentWeight, maxCapacity);
        }
    }
}
