using System;
using UnityEngine;

namespace GameStart.Economy
{
    public class PlayerCurrency : MonoBehaviour
    {
        [SerializeField] private int startingGems;

        public event Action<int> GemsChanged;

        public int Gems { get; private set; }

        private void Awake()
        {
            Gems = startingGems;
        }

        public void AddGems(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Gems += amount;
            GemsChanged?.Invoke(Gems);
        }

        /// <summary>Sets gems directly - for restoring saved progress.</summary>
        public void LoadGems(int amount)
        {
            Gems = Mathf.Max(0, amount);
            GemsChanged?.Invoke(Gems);
        }

        public void ResetGems()
        {
            Gems = 0;
            GemsChanged?.Invoke(Gems);
        }

        public bool TrySpendGems(int amount)
        {
            if (amount <= 0 || Gems < amount)
            {
                return false;
            }

            Gems -= amount;
            GemsChanged?.Invoke(Gems);
            return true;
        }
    }
}
