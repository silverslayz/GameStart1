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

        private static readonly Color EmptyGlow = new Color(0.2f, 0.55f, 0.7f, 0.25f);
        private static readonly Color FilledGlow = new Color(0.35f, 0.85f, 1f, 0.95f);
        private static readonly Color EmptyBackground = new Color(0.08f, 0.12f, 0.2f, 0.35f);
        private static readonly Color FilledBackground = new Color(0.1f, 0.18f, 0.3f, 0.75f);

        private static GameObject dragGhost;
        private static InventorySlotUI dragSource;

        private Image background;
        private Outline glowOutline;

        private void Awake()
        {
            background = GetComponent<Image>();
            glowOutline = GetComponent<Outline>();
        }

        public void SetSlot(InventorySlot slot)
        {
            bool isEmpty = slot == null || slot.IsEmpty;

            if (nameText != null) nameText.text = isEmpty ? "" : slot.Item.Name;
            if (countText != null) countText.text = !isEmpty && slot.Count > 1 ? $"x{slot.Count}" : "";

            if (background != null)
            {
                background.color = isEmpty ? EmptyBackground : FilledBackground;
            }

            if (glowOutline != null)
            {
                glowOutline.effectColor = isEmpty ? EmptyGlow : FilledGlow;
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
