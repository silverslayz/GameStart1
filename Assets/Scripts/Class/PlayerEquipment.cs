using System;
using UnityEngine;
using GameStart.Player;

namespace GameStart.Class
{
    public enum EquipmentSlotType
    {
        Head,
        Chest,
        Legs,
        Boots,
        MainHand,
        OffHand,
        Cloak,
        Accessory
    }

    /// <summary>
    /// Holds the gear the player is currently wearing, one item per slot.
    ///
    /// Deliberately carries no stat effects - those belong to the equipment stat system
    /// (issue #109), which isn't built yet. This exists so gear can be equipped, displayed
    /// and persisted now, with modifiers layered on later.
    /// </summary>
    public class PlayerEquipment : MonoBehaviour
    {
        public static readonly EquipmentSlotType[] AllSlots =
        {
            EquipmentSlotType.Head,
            EquipmentSlotType.Chest,
            EquipmentSlotType.Legs,
            EquipmentSlotType.Boots,
            EquipmentSlotType.MainHand,
            EquipmentSlotType.OffHand,
            EquipmentSlotType.Cloak,
            EquipmentSlotType.Accessory
        };

        private GearItem[] equipped;
        private bool[] occupied;

        private PlayerWeight weight;

        public event Action EquipmentChanged;

        private void Awake()
        {
            EnsureArrays();
            weight = GetComponent<PlayerWeight>();
        }

        /// <summary>
        /// Callers can run before this component's Awake (Unity gives no ordering guarantee),
        /// so every public entry point allocates first rather than assuming Awake ran.
        /// </summary>
        private void EnsureArrays()
        {
            if (equipped == null)
            {
                int count = Enum.GetValues(typeof(EquipmentSlotType)).Length;
                equipped = new GearItem[count];
                occupied = new bool[count];
            }

            // Resolved here as well as in Awake: the save controller can restore gear before
            // this component's Awake has run, and a null reference here would silently drop
            // the equipped weight.
            if (weight == null)
            {
                weight = GetComponent<PlayerWeight>();
            }
        }

        public bool IsEquipped(EquipmentSlotType type)
        {
            EnsureArrays();
            return occupied[(int)type];
        }

        public GearItem GetEquipped(EquipmentSlotType type)
        {
            EnsureArrays();
            return equipped[(int)type];
        }

        /// <summary>
        /// Puts an item in a slot and returns whatever it displaced, so the caller can decide
        /// where the old item goes rather than having it silently destroyed.
        /// </summary>
        public GearItem Equip(EquipmentSlotType type, GearItem item, out bool displaced)
        {
            EnsureArrays();
            int i = (int)type;

            GearItem previous = equipped[i];
            displaced = occupied[i];

            if (displaced)
            {
                weight?.RemoveWeight(previous.Weight);
            }

            equipped[i] = item;
            occupied[i] = true;
            weight?.AddWeight(item.Weight);

            EquipmentChanged?.Invoke();
            return previous;
        }

        public bool Unequip(EquipmentSlotType type, out GearItem item)
        {
            EnsureArrays();
            int i = (int)type;

            if (!occupied[i])
            {
                item = default;
                return false;
            }

            item = equipped[i];
            weight?.RemoveWeight(item.Weight);

            equipped[i] = default;
            occupied[i] = false;

            EquipmentChanged?.Invoke();
            return true;
        }

        public void Clear()
        {
            EnsureArrays();
            for (int i = 0; i < equipped.Length; i++)
            {
                if (occupied[i])
                {
                    weight?.RemoveWeight(equipped[i].Weight);
                }

                equipped[i] = default;
                occupied[i] = false;
            }

            EquipmentChanged?.Invoke();
        }

        /// <summary>
        /// Restores one slot from saved data without touching carried weight.
        /// Call <see cref="FinishLoading"/> once the whole batch is in.
        /// </summary>
        public void LoadSlot(EquipmentSlotType type, string itemName, float itemWeight)
        {
            EnsureArrays();
            int i = (int)type;

            if (string.IsNullOrEmpty(itemName))
            {
                equipped[i] = default;
                occupied[i] = false;
                return;
            }

            equipped[i] = new GearItem(itemName, itemWeight);
            occupied[i] = true;
        }

        /// <summary>
        /// Adds equipped weight on top of what the inventory already accounted for, then
        /// notifies listeners.
        ///
        /// Must run AFTER <see cref="PlayerInventory.FinishLoading"/>, which resets carried
        /// weight to zero before adding the bag's contents. Running this first would have
        /// that reset wipe the equipped weight straight back out.
        /// </summary>
        public void FinishLoading()
        {
            EnsureArrays();

            for (int i = 0; i < equipped.Length; i++)
            {
                if (occupied[i])
                {
                    weight?.AddWeight(equipped[i].Weight);
                }
            }

            EquipmentChanged?.Invoke();
        }

        /// <summary>Human-readable slot name for UI labels.</summary>
        public static string DisplayName(EquipmentSlotType type)
        {
            switch (type)
            {
                case EquipmentSlotType.MainHand: return "Main Hand";
                case EquipmentSlotType.OffHand: return "Off Hand";
                default: return type.ToString();
            }
        }
    }
}
