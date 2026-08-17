using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameStart.Gathering
{
    public class PlayerResources : MonoBehaviour
    {
        private readonly Dictionary<string, int> resourceCounts = new Dictionary<string, int>();

        public event Action<string, int> ResourceChanged;

        public int GetAmount(string resourceName) => resourceCounts.TryGetValue(resourceName, out int amount) ? amount : 0;

        public void AddResource(string resourceName, int amount)
        {
            if (amount <= 0 || string.IsNullOrEmpty(resourceName))
            {
                return;
            }

            resourceCounts[resourceName] = GetAmount(resourceName) + amount;
            ResourceChanged?.Invoke(resourceName, resourceCounts[resourceName]);
        }

        public bool TryConsume(string resourceName, int amount)
        {
            if (amount <= 0 || GetAmount(resourceName) < amount)
            {
                return false;
            }

            resourceCounts[resourceName] -= amount;
            ResourceChanged?.Invoke(resourceName, resourceCounts[resourceName]);
            return true;
        }
    }
}
