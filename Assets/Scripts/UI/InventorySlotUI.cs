using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameStart.Class;

namespace GameStart.UI
{
    public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text countText;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private bool isHotbarSlot;
        [SerializeField] private int slotIndex;

        private static GameObject dragGhost;
        private static InventorySlotUI dragSource;

        /// <summary>Raised when any slot is clicked. The inventory screen uses this to drive selection.</summary>
        public static event Action<InventorySlotUI> SlotClicked;

        /// <summary>The slot currently being dragged, so equipment slots can accept the drop.</summary>
        public static InventorySlotUI CurrentDragSource => dragSource;

        /// <summary>Consumes the active drag so OnEndDrag won't also treat it as a drop-outside.</summary>
        public static void ConsumeDrag()
        {
            dragSource = null;
        }

        private Image background;
        private Outline glowOutline;
        private Image[] frame;
        private bool isSelected;
        private bool isEmpty = true;

        public bool IsHotbarSlot => isHotbarSlot;
        public int SlotIndex => slotIndex;
        public PlayerInventory Inventory => inventory;

        private void Awake()
        {
            background = GetComponent<Image>();
            glowOutline = GetComponent<Outline>();

            // Built here rather than in the scene so hand-placed and code-built slots
            // get the same border treatment.
            frame = InventoryTheme.CreateFrame("Border", transform, InventoryTheme.SlotBorderEmpty);
            ApplyVisualState();
        }

        public void SetSlot(InventorySlot slot)
        {
            isEmpty = slot == null || slot.IsEmpty;

            if (nameText != null) nameText.text = isEmpty ? "" : slot.Item.Name;
            if (countText != null) countText.text = !isEmpty && slot.Count > 1 ? $"x{slot.Count}" : "";

            ApplyVisualState();
        }

        public void Configure(PlayerInventory targetInventory, bool hotbar, int index)
        {
            inventory = targetInventory;
            isHotbarSlot = hotbar;
            slotIndex = index;
        }

        /// <summary>Injects the label references for slots built at runtime rather than authored in a scene.</summary>
        public void BindLabels(Text name, Text count)
        {
            nameText = name;
            countText = count;
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            ApplyVisualState();
        }

        private void ApplyVisualState()
        {
            if (background != null)
            {
                background.color = isEmpty ? InventoryTheme.SlotEmpty : InventoryTheme.SlotFilled;
            }

            Color border = isSelected
                ? InventoryTheme.Accent
                : (isEmpty ? InventoryTheme.SlotBorderEmpty : InventoryTheme.SlotBorderFilled);

            InventoryTheme.SetFrameColor(frame, border);

            if (glowOutline != null)
            {
                // Only the selected cell carries a glow; everything else stays flat so the
                // grid reads as a calm field rather than a wall of highlights.
                glowOutline.effectColor = isSelected ? InventoryTheme.AccentSoft : Color.clear;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SlotClicked?.Invoke(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Nothing to describe mid-drag, and the tooltip would chase the ghost.
            if (dragSource != null)
            {
                return;
            }

            InventorySlot slot = inventory != null ? inventory.GetSlot(isHotbarSlot, slotIndex) : null;
            if (slot == null || slot.IsEmpty)
            {
                InventoryTooltipUI.Hide();
                return;
            }

            InventoryTooltipUI.Show(slot.Item, slot.Count, isHotbarSlot ? "HOTBAR" : "MAIN INVENTORY");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            InventoryTooltipUI.Hide();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            InventorySlot slot = inventory != null ? inventory.GetSlot(isHotbarSlot, slotIndex) : null;
            if (slot == null || slot.IsEmpty)
            {
                return;
            }

            dragSource = this;
            InventoryTooltipUI.Hide();

            dragGhost = new GameObject("DragGhost", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            var rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
            dragGhost.transform.SetParent(rootCanvas.transform, false);
            dragGhost.transform.SetAsLastSibling();
            var ghostRt = dragGhost.GetComponent<RectTransform>();
            ghostRt.sizeDelta = new Vector2(InventoryTheme.SlotSize, InventoryTheme.SlotSize);
            var ghostImage = dragGhost.GetComponent<Image>();
            ghostImage.color = new Color(InventoryTheme.Cyan.r, InventoryTheme.Cyan.g, InventoryTheme.Cyan.b, 0.45f);
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

            // Anything that isn't a valid drop target snaps back: the item simply stays put.
            // This used to discard the stack whenever eventData.pointerEnter was null, which
            // destroyed items on a slightly long drag and behaved differently depending on
            // what happened to sit under the cursor. Discarding is now an explicit action on
            // the drop zone (see InventoryDropZoneUI).
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
