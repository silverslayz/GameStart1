using UnityEngine;
using GameStart.Class;

namespace GameStart.UI
{
    public class HotbarUI : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private InventorySlotUI[] hotbarSlotUIs;

        private void OnEnable()
        {
            if (inventory != null)
            {
                inventory.InventoryChanged += Refresh;
            }
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.InventoryChanged -= Refresh;
            }
        }

        private void Start()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (inventory == null || hotbarSlotUIs == null || inventory.HotbarSlots == null)
            {
                return;
            }

            var slots = inventory.HotbarSlots;
            for (int i = 0; i < hotbarSlotUIs.Length && i < slots.Count; i++)
            {
                hotbarSlotUIs[i].SetSlot(slots[i]);
            }
        }
    }
}
