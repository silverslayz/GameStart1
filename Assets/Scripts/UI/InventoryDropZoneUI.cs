using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using GameStart.Class;
using GameStart.Interaction;

namespace GameStart.UI
{
    /// <summary>
    /// An explicit target for discarding items: drag a stack here and it lands in the world
    /// as a <see cref="DroppedItem"/> that can be picked back up.
    ///
    /// Deliberately an opt-in target rather than "anywhere outside the grid counts as a drop".
    /// The old behaviour discarded items whenever a drag ended over nothing, which destroyed
    /// stacks by accident and fired inconsistently depending on what sat under the cursor.
    /// </summary>
    public class InventoryDropZoneUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private PlayerInventory inventory;
        private Transform dropOrigin;
        private Image background;
        private Image[] frame;
        private Text label;

        private const float DropForwardOffset = 1.1f;
        private const float DropHeightOffset = 0.35f;

        public void Configure(PlayerInventory targetInventory, Transform origin, Image bg, Image[] borderFrame, Text zoneLabel)
        {
            inventory = targetInventory;
            dropOrigin = origin;
            background = bg;
            frame = borderFrame;
            label = zoneLabel;
            SetHighlighted(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetHighlighted(InventorySlotUI.CurrentDragSource != null);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHighlighted(false);
        }

        private void SetHighlighted(bool highlighted)
        {
            if (background != null)
            {
                background.color = highlighted
                    ? new Color(InventoryTheme.Danger.r, InventoryTheme.Danger.g, InventoryTheme.Danger.b, 0.22f)
                    : InventoryTheme.SlotEmpty;
            }

            InventoryTheme.SetFrameColor(frame, highlighted ? InventoryTheme.Danger : InventoryTheme.SlotBorderEmpty);

            if (label != null)
            {
                label.color = highlighted ? InventoryTheme.Danger : InventoryTheme.BodyText;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            var source = InventorySlotUI.CurrentDragSource;
            SetHighlighted(false);

            if (source == null || inventory == null)
            {
                return;
            }

            InventorySlot slot = inventory.GetSlot(source.IsHotbarSlot, source.SlotIndex);
            if (slot == null || slot.IsEmpty)
            {
                return;
            }

            // One unit by default; the whole stack only with a deliberate modifier held.
            bool wholeStack = IsStackModifierHeld();
            GearItem item = slot.Item;
            int quantity = wholeStack ? slot.Count : 1;

            int moved = 0;
            for (int i = 0; i < quantity; i++)
            {
                if (!inventory.TakeOne(source.IsHotbarSlot, source.SlotIndex, out _))
                {
                    break;
                }

                moved++;
            }

            if (moved > 0)
            {
                DroppedItem.Spawn(item, moved, DropPosition());
            }

            InventorySlotUI.ConsumeDrag();
        }

        private static bool IsStackModifierHeld()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
        }

        private Vector3 DropPosition()
        {
            if (dropOrigin == null)
            {
                return Vector3.zero;
            }

            return dropOrigin.position
                   + dropOrigin.forward * DropForwardOffset
                   + Vector3.up * DropHeightOffset;
        }
    }
}
