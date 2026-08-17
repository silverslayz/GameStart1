using UnityEngine;
using UnityEngine.UI;

namespace GameStart.UI
{
    /// <summary>
    /// Shared palette and builder helpers for the code-built inventory screen.
    /// Kept in one place so the header, grid and detail panel can't drift apart visually.
    /// </summary>
    public static class InventoryTheme
    {
        // Cool blue-grey base with a warm amber accent, matching the sci-fi reference
        // while keeping the cyan highlight the rest of the game's UI already uses.
        // Fully opaque: at any alpha below 1 the HUD and other canvases behind this screen
        // stay legible through it, which reads as a rendering fault rather than depth.
        public static readonly Color Backdrop = new Color(0.035f, 0.051f, 0.075f, 1f);
        public static readonly Color PanelFill = new Color(0.071f, 0.094f, 0.129f, 0.92f);
        public static readonly Color Divider = new Color(1f, 1f, 1f, 0.15f);

        public static readonly Color TitleText = new Color(0.94f, 0.96f, 0.98f, 1f);
        public static readonly Color BreadcrumbDim = new Color(0.45f, 0.54f, 0.64f, 1f);
        public static readonly Color Accent = new Color(1f, 0.62f, 0.11f, 1f);
        public static readonly Color AccentSoft = new Color(1f, 0.62f, 0.11f, 0.35f);
        public static readonly Color Cyan = new Color(0.35f, 0.80f, 1f, 1f);

        public static readonly Color SectionLabel = new Color(0.86f, 0.90f, 0.94f, 1f);
        public static readonly Color BodyText = new Color(0.66f, 0.72f, 0.79f, 1f);
        public static readonly Color ValueText = new Color(0.88f, 0.92f, 0.96f, 1f);
        public static readonly Color HintText = new Color(0.55f, 0.63f, 0.72f, 1f);

        public static readonly Color SlotEmpty = new Color(0.106f, 0.137f, 0.184f, 1f);
        public static readonly Color SlotFilled = new Color(0.169f, 0.227f, 0.302f, 1f);
        public static readonly Color SlotBorderEmpty = new Color(1f, 1f, 1f, 0.15f);
        public static readonly Color SlotBorderFilled = new Color(0.42f, 0.62f, 0.78f, 0.85f);

        public const int SlotSize = 84;
        public const int SlotGap = 6;
        public const int GridColumns = 5;

        private static Font cachedFont;

        public static Font Font
        {
            get
            {
                if (cachedFont == null)
                {
                    // Unity 6 ships the old dynamic Arial under this name.
                    cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }

                return cachedFont;
            }
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        public static Image CreateImage(string name, Transform parent, Color color, bool raycast = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        public static Text CreateText(string name, Transform parent, string content, int size, Color color, TextAnchor anchor, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Font;
            text.text = content;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        /// <summary>Adds letter spacing by padding between characters - legacy Text has no tracking control.</summary>
        public static string Spaced(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var builder = new System.Text.StringBuilder(value.Length * 2);
            for (int i = 0; i < value.Length; i++)
            {
                builder.Append(value[i]);
                if (i < value.Length - 1)
                {
                    builder.Append(' ');
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Builds a hairline border out of four stretched strips. Legacy UI has no border
        /// primitive, and the Outline effect blurs at this thickness, so strips stay crisp.
        /// </summary>
        public static Image[] CreateFrame(string name, Transform parent, Color color, float thickness = 2f)
        {
            var root = CreateRect(name, parent);
            Fill(root);

            var strips = new Image[4];
            // top, bottom, left, right
            var mins = new[] { new Vector2(0f, 1f), Vector2.zero, Vector2.zero, new Vector2(1f, 0f) };
            var maxs = new[] { Vector2.one, new Vector2(1f, 0f), new Vector2(0f, 1f), Vector2.one };

            for (int i = 0; i < 4; i++)
            {
                var strip = CreateImage("Edge", root, color, raycast: false);
                var rect = strip.rectTransform;
                rect.anchorMin = mins[i];
                rect.anchorMax = maxs[i];
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                bool horizontal = i < 2;
                rect.sizeDelta = horizontal ? new Vector2(0f, thickness) : new Vector2(thickness, 0f);
                strips[i] = strip;
            }

            return strips;
        }

        public static void SetFrameColor(Image[] frame, Color color)
        {
            if (frame == null)
            {
                return;
            }

            for (int i = 0; i < frame.Length; i++)
            {
                if (frame[i] != null)
                {
                    frame[i].color = color;
                }
            }
        }

        /// <summary>Stretches a RectTransform to fill its parent, with an optional uniform inset.</summary>
        public static void Fill(RectTransform rect, float inset = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        /// <summary>Anchors a rect to a corner/edge of its parent with an explicit size.</summary>
        public static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
        }
    }
}
