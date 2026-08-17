using UnityEngine;
using UnityEngine.InputSystem;
using GameStart.Class;

namespace GameStart.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private InventorySlotUI[] mainSlotUIs;

        private bool isOpen;

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

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
            {
                isOpen = !isOpen;
                if (panel != null)
                {
                    panel.SetActive(isOpen);
                }

                if (isOpen)
                {
                    Refresh();
                }
            }
        }

        private void Refresh()
        {
            if (inventory == null || mainSlotUIs == null)
            {
                return;
            }

            var slots = inventory.MainSlots;
            for (int i = 0; i < mainSlotUIs.Length && i < slots.Count; i++)
            {
                mainSlotUIs[i].SetSlot(slots[i]);
            }
        }
    }
}
