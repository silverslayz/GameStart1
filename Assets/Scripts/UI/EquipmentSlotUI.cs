using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameStart.Class;

namespace GameStart.UI
{
    /// <summary>
    /// One equipment slot in the character panel. Accepts an item dragged from the inventory
    /// grid, and shows what is currently worn in that slot.
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public static event Action<EquipmentSlotUI> SlotClicked;

        private PlayerInventory inventory;
        private PlayerEquipment equipment;
        private EquipmentSlotType slotType;

        private Image background;
        private Image[] frame;
        private Text itemLabel;
        private bool isSelected;

        public EquipmentSlotType SlotType => slotType;

        public void Configure(PlayerInventory targetInventory, PlayerEquipment targetEquipment, EquipmentSlotType type,
            Image bg, Image[] borderFrame, Text label)
        {
            inventory = targetInventory;
            equipment = targetEquipment;
            slotType = type;
            background = bg;
            frame = borderFrame;
            itemLabel = label;
            Refresh();
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            Refresh();
        }

        public void Refresh()
        {
            bool filled = equipment != null && equipment.IsEquipped(slotType);

            if (itemLabel != null)
            {
                itemLabel.text = filled ? equipment.GetEquipped(slotType).Name : "";
            }

            if (background != null)
            {
                background.color = filled ? InventoryTheme.SlotFilled : InventoryTheme.SlotEmpty;
            }

            Color border = isSelected
                ? InventoryTheme.Accent
                : (filled ? InventoryTheme.SlotBorderFilled : InventoryTheme.SlotBorderEmpty);
            InventoryTheme.SetFrameColor(frame, border);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SlotClicked?.Invoke(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Mid-drag the interesting item is the one on the cursor, not what's worn here.
            if (InventorySlotUI.CurrentDragSource != null
                || equipment == null
                || !equipment.IsEquipped(slotType))
            {
                InventoryTooltipUI.Hide();
                return;
            }

            InventoryTooltipUI.Show(equipment.GetEquipped(slotType), 1,
                "EQUIPPED  //  " + PlayerEquipment.DisplayName(slotType).ToUpperInvariant());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            InventoryTooltipUI.Hide();
        }

        public void OnDrop(PointerEventData eventData)
        {
            var source = InventorySlotUI.CurrentDragSource;
            if (source == null || inventory == null || equipment == null)
            {
                return;
            }

            if (!inventory.TakeOne(source.IsHotbarSlot, source.SlotIndex, out GearItem incoming))
            {
                return;
            }

            GearItem displaced = equipment.Equip(slotType, incoming, out bool hadPrevious);

            // Put whatever was already worn back into the bag rather than dropping it on the
            // floor. If the bag is full it goes back on the character, so nothing is destroyed.
            if (hadPrevious && !inventory.AddItem(displaced))
            {
                equipment.Equip(slotType, displaced, out _);
                inventory.AddItem(incoming);
            }

            // Stops InventorySlotUI.OnEndDrag from also reading this as a drop outside the panel.
            InventorySlotUI.ConsumeDrag();
        }
    }
}
