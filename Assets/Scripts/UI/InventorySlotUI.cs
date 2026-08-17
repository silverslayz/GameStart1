using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameStart.Class;

namespace GameStart.UI
{
    public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text countText;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private bool isHotbarSlot;
        [SerializeField] private int slotIndex;

        private static GameObject dragGhost;
        private static InventorySlotUI dragSource;

        public void SetSlot(InventorySlot slot)
        {
            if (slot == null || slot.IsEmpty)
            {
                if (nameText != null) nameText.text = "";
                if (countText != null) countText.text = "";
                return;
            }

            if (nameText != null)
            {
                nameText.text = slot.Item.Name;
            }

            if (countText != null)
            {
                countText.text = slot.Count > 1 ? $"x{slot.Count}" : "";
            }
        }

        public void Configure(PlayerInventory targetInventory, bool hotbar, int index)
        {
            inventory = targetInventory;
            isHotbarSlot = hotbar;
            slotIndex = index;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            InventorySlot slot = inventory != null ? inventory.GetSlot(isHotbarSlot, slotIndex) : null;
            if (slot == null || slot.IsEmpty)
            {
                return;
            }

            dragSource = this;

            dragGhost = new GameObject("DragGhost", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            var rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
            dragGhost.transform.SetParent(rootCanvas.transform, false);
            dragGhost.transform.SetAsLastSibling();
            var ghostRt = dragGhost.GetComponent<RectTransform>();
            ghostRt.sizeDelta = new Vector2(48, 48);
            var ghostImage = dragGhost.GetComponent<Image>();
            ghostImage.color = new Color(1f, 1f, 1f, 0.6f);
            dragGhost.GetComponent<CanvasGroup>().blocksRaycasts = false;

            ghostRt.position = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragGhost != null)
            {
                dragGhost.GetComponent<RectTransform>().position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragGhost != null)
            {
                Destroy(dragGhost);
                dragGhost = null;
            }

            // If we didn't land on a valid slot's OnDrop, treat this as "dropped outside the inventory" - discard the item.
            if (dragSource == this && eventData.pointerEnter == null)
            {
                inventory?.DropSlot(isHotbarSlot, slotIndex);
            }

            dragSource = null;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (dragSource == null || dragSource == this || inventory == null)
            {
                return;
            }

            inventory.SwapSlots(dragSource.isHotbarSlot, dragSource.slotIndex, isHotbarSlot, slotIndex);
        }
    }
}
