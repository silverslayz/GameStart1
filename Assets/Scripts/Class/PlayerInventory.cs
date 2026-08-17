using System;
using System.Collections.Generic;
using UnityEngine;
using GameStart.Player;

namespace GameStart.Class
{
    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private int hotbarSlotCount = 6;
        [SerializeField] private int mainSlotCount = 20;
        [SerializeField] private int maxStackSize = 10;

        private InventorySlot[] hotbarSlots;
        private InventorySlot[] mainSlots;

        public event Action<IReadOnlyList<GearItem>> ItemsChanged;
        public event Action InventoryChanged;

        public IReadOnlyList<InventorySlot> HotbarSlots => hotbarSlots;
        public IReadOnlyList<InventorySlot> MainSlots => mainSlots;
        public int MaxStackSize => maxStackSize;

        // Flattened view (one entry per unit held) for backward compatibility
        // with systems built before the slot model existed.
        public IReadOnlyList<GearItem> Items
        {
            get
            {
                var list = new List<GearItem>();
                foreach (InventorySlot slot in AllSlots())
                {
                    for (int i = 0; i < slot.Count; i++)
                    {
                        list.Add(slot.Item);
                    }
                }

                return list;
            }
        }

        private PlayerClassSelection classSelection;
        private PlayerWeight weight;

        private void Awake()
        {
            classSelection = GetComponent<PlayerClassSelection>();
            weight = GetComponent<PlayerWeight>();

            hotbarSlots = CreateSlots(hotbarSlotCount);
            mainSlots = CreateSlots(mainSlotCount);
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

        private static InventorySlot[] CreateSlots(int count)
        {
            var slots = new InventorySlot[count];
            for (int i = 0; i < count; i++)
            {
                slots[i] = new InventorySlot();
            }

            return slots;
        }

        private IEnumerable<InventorySlot> AllSlots()
        {
            foreach (InventorySlot slot in hotbarSlots) yield return slot;
            foreach (InventorySlot slot in mainSlots) yield return slot;
        }

        public void Clear()
        {
            foreach (InventorySlot slot in AllSlots())
            {
                slot.Item = default;
                slot.Count = 0;
            }

            weight?.ResetWeight();
            ItemsChanged?.Invoke(Items);
            InventoryChanged?.Invoke();
        }

        /// <summary>Adds one unit of the item to the first matching stack, or the first empty slot. Returns false if there's no room.</summary>
        public bool AddItem(GearItem item)
        {
            InventorySlot target = null;

            foreach (InventorySlot slot in AllSlots())
            {
                if (!slot.IsEmpty && slot.Item.Name == item.Name && slot.Count < maxStackSize)
                {
                    target = slot;
                    break;
                }
            }

            if (target == null)
            {
                foreach (InventorySlot slot in AllSlots())
                {
                    if (slot.IsEmpty)
                    {
                        target = slot;
                        break;
                    }
                }
            }

            if (target == null)
            {
                return false; // inventory full
            }

            target.Item = item;
            target.Count++;

            weight?.AddWeight(item.Weight);
            ItemsChanged?.Invoke(Items);
            InventoryChanged?.Invoke();
            return true;
        }

        public InventorySlot GetSlot(bool hotbar, int index)
        {
            InventorySlot[] slots = hotbar ? hotbarSlots : mainSlots;
            if (index < 0 || index >= slots.Length)
            {
                return null;
            }

            return slots[index];
        }

        /// <summary>Swaps the contents of two slots (works across hotbar/main, and as a no-op if from==to).</summary>
        public void SwapSlots(bool fromHotbar, int fromIndex, bool toHotbar, int toIndex)
        {
            InventorySlot from = GetSlot(fromHotbar, fromIndex);
            InventorySlot to = GetSlot(toHotbar, toIndex);
            if (from == null || to == null || from == to)
            {
                return;
            }

            (from.Item, to.Item) = (to.Item, from.Item);
            (from.Count, to.Count) = (to.Count, from.Count);

            ItemsChanged?.Invoke(Items);
            InventoryChanged?.Invoke();
        }

        /// <summary>Removes the item from a slot entirely (e.g. dragging it out of the inventory), reducing carried weight.</summary>
        public void DropSlot(bool hotbar, int index)
        {
            InventorySlot slot = GetSlot(hotbar, index);
            if (slot == null || slot.IsEmpty)
            {
                return;
            }

            weight?.RemoveWeight(slot.Item.Weight * slot.Count);
            slot.Item = default;
            slot.Count = 0;

            ItemsChanged?.Invoke(Items);
            InventoryChanged?.Invoke();
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
