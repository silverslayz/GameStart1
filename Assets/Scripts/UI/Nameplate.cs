using UnityEngine;
using UnityEngine.UI;

namespace GameStart.UI
{
    /// <summary>
    /// Floating name label above a character, so friendly NPCs can be told apart without
    /// walking into interaction range.
    ///
    /// Built in code and billboarded to the camera each frame. World-space canvases do not
    /// follow the view on their own, so without this the label is readable from exactly one
    /// angle and edge-on from everywhere else.
    /// </summary>
    public class Nameplate : MonoBehaviour
    {
        [SerializeField] private string label = "NPC";
        [SerializeField] private Color colour = new Color(0.55f, 0.75f, 1f);
        [SerializeField] private float heightOffset = 0.35f;

        [Tooltip("Beyond this distance the plate hides, so a crowded hub isn't a wall of text.")]
        [SerializeField] private float visibleDistance = 22f;

        private Canvas canvas;
        private Text text;
        private Transform view;
        private float baseHeight;

        public string Label
        {
            get => label;
            set
            {
                label = value;
                if (text != null) text.text = value;
            }
        }

        private void Start()
        {
            Build();
        }

        private void Build()
        {
            if (canvas != null)
            {
                return;
            }

            // Sit above the character's actual silhouette rather than a guessed offset.
            float top = 2f;
            var renderer = GetComponentInChildren<Renderer>(true);
            if (renderer != null)
            {
                top = renderer.bounds.max.y - transform.position.y;
            }
            baseHeight = top + heightOffset;

            var go = new GameObject("Nameplate", typeof(RectTransform), typeof(Canvas));
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, baseHeight, 0f);

            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            // Sized in pixels and scaled to world units. Authoring the rect directly in
            // metres made a 36pt font about 6 metres tall - the label filled the screen.
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(220f, 50f);
            rect.localScale = Vector3.one * 0.0045f;

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);

            text = textGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = label;
            text.fontSize = 32;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = colour;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // An outline keeps light text readable against the pale sky and grass.
            var outline = textGo.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);
        }

        private void LateUpdate()
        {
            if (canvas == null)
            {
                return;
            }

            if (view == null)
            {
                Camera cam = Camera.main;
                if (cam == null) return;
                view = cam.transform;
            }

            float distance = Vector3.Distance(view.position, transform.position);
            bool visible = distance <= visibleDistance;
            if (canvas.enabled != visible)
            {
                canvas.enabled = visible;
            }

            if (!visible)
            {
                return;
            }

            // Face the camera, upright. Copying the camera's rotation keeps the text level
            // rather than tilting with a look-at when the camera pitches.
            canvas.transform.rotation = view.rotation;
        }

        public void Configure(string newLabel, Color newColour)
        {
            label = newLabel;
            colour = newColour;
            if (text != null)
            {
                text.text = newLabel;
                text.color = newColour;
            }
        }
    }
}
