using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using GameStart.Class;

namespace GameStart.UI
{
    /// <summary>
    /// Hover tooltip for inventory and equipment slots. The detail panel already answers
    /// "what is this?" for the selected slot, but only after a click - comparing two items
    /// meant clicking each in turn and remembering the first. This answers it for whatever
    /// the cursor is over, without disturbing the selection.
    ///
    /// Built in code and owned by <see cref="InventoryScreenUI"/>, matching how the rest of
    /// that screen is built.
    /// </summary>
    public class InventoryTooltipUI : MonoBehaviour
    {
        private const float Width = 268f;
        private const float PaddingX = 14f;
        private const float PaddingY = 12f;
        private const float TitleHeight = 26f;
        private const float RowHeight = 21f;
        private const float ContextHeight = 18f;
        private const float DividerGap = 10f;
        private const float CursorGap = 16f;

        private static InventoryTooltipUI instance;

        private RectTransform canvasRect;
        private RectTransform panel;
        private Text titleText;
        private Text contextText;
        private RectTransform divider;
        private Row[] rows;
        private int activeRows;

        /// <summary>Shows the tooltip for an inventory stack. No-ops when the screen isn't built.</summary>
        public static void Show(GearItem item, int count, string context)
        {
            instance?.ShowInternal(item, count, context);
        }

        public static void Hide()
        {
            instance?.HideInternal();
        }

        public void Build(RectTransform canvas)
        {
            instance = this;
            canvasRect = canvas;

            panel = InventoryTheme.CreateRect("Tooltip", canvas);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0f, 1f);
            panel.sizeDelta = new Vector2(Width, 100f);

            // Nothing in here may take raycasts: the panel chases the cursor, and a
            // raycast target under it would swallow the pointer-exit of the slot that
            // opened it, leaving the tooltip stuck.
            var background = InventoryTheme.CreateImage("Background", panel, InventoryTheme.PanelFill, raycast: false);
            InventoryTheme.Fill(background.rectTransform);
            InventoryTheme.CreateFrame("Border", panel, InventoryTheme.SlotBorderFilled);

            titleText = InventoryTheme.CreateText("Title", panel, "", 19, InventoryTheme.TitleText, TextAnchor.UpperLeft, FontStyle.Bold);
            InventoryTheme.Anchor(titleText.rectTransform,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                new Vector2(PaddingX, -PaddingY), new Vector2(-PaddingX * 2f, TitleHeight));

            var dividerImage = InventoryTheme.CreateImage("Divider", panel, InventoryTheme.Divider, raycast: false);
            divider = dividerImage.rectTransform;
            InventoryTheme.Anchor(divider,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                new Vector2(PaddingX, -(PaddingY + TitleHeight)), new Vector2(-PaddingX * 2f, 1f));

            // Weight, stack, total: the most rows any item can need.
            rows = new Row[3];
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = new Row(panel, i);
            }

            contextText = InventoryTheme.CreateText("Context", panel, "", 12, InventoryTheme.BreadcrumbDim, TextAnchor.LowerLeft);
            InventoryTheme.Anchor(contextText.rectTransform,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f),
                new Vector2(PaddingX, PaddingY), new Vector2(-PaddingX * 2f, ContextHeight));

            panel.gameObject.SetActive(false);
        }

        private void ShowInternal(GearItem item, int count, string context)
        {
            if (panel == null)
            {
                return;
            }

            titleText.text = item.Name;
            contextText.text = InventoryTheme.Spaced(context);

            activeRows = 0;
            SetRow("WEIGHT", $"{item.Weight:0.##}");

            if (count > 1)
            {
                SetRow("STACK", $"x{count}");
                SetRow("TOTAL", $"{item.Weight * count:0.##}");
            }

            for (int i = activeRows; i < rows.Length; i++)
            {
                rows[i].SetActive(false);
            }

            float height = PaddingY + TitleHeight + DividerGap
                           + activeRows * RowHeight
                           + ContextHeight + PaddingY;
            panel.sizeDelta = new Vector2(Width, height);

            panel.gameObject.SetActive(true);
            panel.SetAsLastSibling();
            Reposition();
        }

        private void SetRow(string label, string value)
        {
            Row row = rows[activeRows];
            row.SetActive(true);
            row.Set(label, value, PaddingY + TitleHeight + DividerGap + activeRows * RowHeight);
            activeRows++;
        }

        private void HideInternal()
        {
            if (panel != null)
            {
                panel.gameObject.SetActive(false);
            }
        }

        private void LateUpdate()
        {
            if (panel != null && panel.gameObject.activeSelf)
            {
                Reposition();
            }
        }

        /// <summary>
        /// Follows the cursor, flipping across it near an edge so the panel never runs off
        /// screen - a tooltip clipped by the screen edge is worse than one on the other side.
        /// </summary>
        private void Reposition()
        {
            if (Mouse.current == null || canvasRect == null)
            {
                return;
            }

            Vector2 screenPoint = Mouse.current.position.ReadValue();
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 local))
            {
                return;
            }

            Vector2 size = panel.sizeDelta;
            Rect canvas = canvasRect.rect;

            bool flipX = local.x + CursorGap + size.x > canvas.xMax;
            bool flipY = local.y - CursorGap - size.y < canvas.yMin;

            panel.pivot = new Vector2(flipX ? 1f : 0f, flipY ? 0f : 1f);
            panel.anchoredPosition = new Vector2(
                local.x + (flipX ? -CursorGap : CursorGap),
                local.y + (flipY ? CursorGap : -CursorGap));
        }

        private void OnDisable()
        {
            HideInternal();
        }

        /// <summary>One label/value line, e.g. "WEIGHT  3".</summary>
        private class Row
        {
            private readonly Text label;
            private readonly Text value;

            public Row(RectTransform parent, int index)
            {
                label = InventoryTheme.CreateText($"RowLabel{index}", parent, "", 13, InventoryTheme.BodyText, TextAnchor.UpperLeft);
                value = InventoryTheme.CreateText($"RowValue{index}", parent, "", 13, InventoryTheme.ValueText, TextAnchor.UpperRight);
            }

            public void SetActive(bool active)
            {
                label.gameObject.SetActive(active);
                value.gameObject.SetActive(active);
            }

            public void Set(string labelText, string valueText, float topOffset)
            {
                label.text = InventoryTheme.Spaced(labelText);
                value.text = valueText;

                InventoryTheme.Anchor(label.rectTransform,
                    new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                    new Vector2(PaddingX, -topOffset), new Vector2(-PaddingX * 2f, RowHeight));

                InventoryTheme.Anchor(value.rectTransform,
                    new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                    new Vector2(PaddingX, -topOffset), new Vector2(-PaddingX * 2f, RowHeight));
            }
        }
    }
}
