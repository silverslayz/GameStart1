using System;
using System.Collections.Generic;
using UnityEngine;
using GameStart.Player;

namespace GameStart.Class
{
    public class PlayerInventory : MonoBehaviour
    {
        private readonly List<GearItem> items = new List<GearItem>();

        public event Action<IReadOnlyList<GearItem>> ItemsChanged;

        public IReadOnlyList<GearItem> Items => items;

        private PlayerClassSelection classSelection;
        private PlayerWeight weight;

        private void Awake()
        {
            classSelection = GetComponent<PlayerClassSelection>();
            weight = GetComponent<PlayerWeight>();
        }

        private void OnEnable()
        {
            if (classSelection != null)
            {
                classSelection.ClassSelected += GrantStarterKit;
            }
        }

        private void OnDisable()
        {
            if (classSelection != null)
            {
                classSelection.ClassSelected -= GrantStarterKit;
            }
        }

        public void AddItem(GearItem item)
        {
            items.Add(item);
            if (weight != null)
            {
                weight.AddWeight(item.Weight);
            }

            ItemsChanged?.Invoke(items);
        }

        private void GrantStarterKit(PlayerClassType classType)
        {
            foreach (GearItem item in StarterGearCatalog.GetStarterKit(classType))
            {
                AddItem(item);
            }
        }
    }
}
