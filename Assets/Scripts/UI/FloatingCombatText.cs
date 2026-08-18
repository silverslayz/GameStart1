using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameStart.Combat;

namespace GameStart.UI
{
    /// <summary>
    /// Static entry point for world-anchored floating text (damage numbers for now).
    /// Self-bootstrapping like <see cref="Flow.SceneTransition"/>: any script can call it
    /// without a serialized reference, and nothing has to be placed in the shared scene.
    /// </summary>
    public static class FloatingCombatText
    {
        public static readonly Color DamageColor = new Color(1f, 0.96f, 0.88f, 1f);

        /// <summary>
        /// The same gold BestiaryUI uses for "Weakness discovered", so the number and the
        /// bestiary entry that explains it read as the same idea.
        /// </summary>
        public static readonly Color WeaknessDamageColor = new Color(1f, 0.85f, 0.3f, 1f);

        /// <summary>
        /// Weakness hits are only 25% bigger in value, which is easy to miss between two
        /// numbers a moment apart - so they're drawn larger as well as recolored, and stay
        /// distinguishable for players who can't rely on the color difference.
        /// </summary>
        private const float WeaknessSizeScale = 1.3f;

        private static FloatingCombatTextRunner runner;

        /// <summary>Spawns a damage number above the target's visible bounds.</summary>
        public static void ShowDamage(Transform target, float amount, DamageFlavor flavor = DamageFlavor.Normal)
        {
            if (target == null)
            {
                return;
            }

            ShowDamage(AnchorAbove(target), amount, flavor);
        }

        /// <summary>Spawns a damage number rising from a world position.</summary>
        public static void ShowDamage(Vector3 worldPosition, float amount, DamageFlavor flavor = DamageFlavor.Normal)
        {
            // Damage is tracked as a float but reads as a whole number, and any landed hit
            // should show at least 1 rather than a "0" that looks like the swing whiffed.
            int rounded = Mathf.Max(1, Mathf.RoundToInt(amount));
            bool weakness = flavor == DamageFlavor.Weakness;

            Show(worldPosition,
                rounded.ToString(),
                weakness ? WeaknessDamageColor : DamageColor,
                weakness ? WeaknessSizeScale : 1f);
        }

        public static void Show(Vector3 worldPosition, string content, Color color, float sizeScale = 1f)
        {
            EnsureRunner();
            runner.Spawn(worldPosition, content, color, sizeScale);
        }

        /// <summary>
        /// Picks a spawn point just above whatever the target actually renders, so numbers
        /// clear the monster's head instead of erupting from its pivot (usually its feet).
        /// </summary>
        private static Vector3 AnchorAbove(Transform target)
        {
            Bounds bounds = default;
            bool hasBounds = false;

            foreach (var renderer in target.GetComponentsInChildren<Renderer>())
            {
                // Renderers get disabled while a monster is "dead", but their bounds stay
                // valid, so a killing blow's number still lands in the right place.
                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
            {
                return target.position + Vector3.up * 1.8f;
            }

            return new Vector3(bounds.center.x, bounds.max.y + 0.25f, bounds.center.z);
        }

        private static void EnsureRunner()
        {
            if (runner != null)
            {
                return;
            }

            var go = new GameObject("FloatingCombatText");
            Object.DontDestroyOnLoad(go);
            runner = go.AddComponent<FloatingCombatTextRunner>();
        }
    }

    /// <summary>
    /// Owns the overlay canvas and drives every live label from a single LateUpdate,
    /// after the camera has moved, so labels don't lag a frame behind their target.
    /// </summary>
    internal class FloatingCombatTextRunner : MonoBehaviour
    {
        private const float Lifetime = 0.9f;
        private const float RiseDistance = 0.85f;
        private const float HorizontalScatter = 0.28f;
        private const float PopInPortion = 0.15f;
        private const float PopScale = 1.35f;
        private const float FadeStart = 0.55f;
        private const int FontSize = 34;

        private readonly List<FloatingLabel> labels = new List<FloatingLabel>();

        private RectTransform canvasRect;

        private void Awake()
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Above the HUD, below SceneTransition's fade so a scene change still covers it.
            canvas.sortingOrder = 500;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasRect = canvasGo.GetComponent<RectTransform>();
        }

        public void Spawn(Vector3 worldPosition, string content, Color color, float sizeScale)
        {
            FloatingLabel label = Rent();
            label.Activate(worldPosition, content, color, Random.Range(-HorizontalScatter, HorizontalScatter), sizeScale);

            // Place it this frame so it never flashes at the canvas centre before LateUpdate.
            Step(label, Camera.main, 0f);
        }

        private FloatingLabel Rent()
        {
            foreach (FloatingLabel pooled in labels)
            {
                if (!pooled.InUse)
                {
                    return pooled;
                }
            }

            var created = new FloatingLabel(canvasRect, FontSize);
            labels.Add(created);
            return created;
        }

        private void LateUpdate()
        {
            Camera camera = Camera.main;
            float delta = Time.deltaTime;

            foreach (FloatingLabel label in labels)
            {
                if (label.InUse)
                {
                    Step(label, camera, delta);
                }
            }
        }

        private void Step(FloatingLabel label, Camera camera, float delta)
        {
            label.Elapsed += delta;
            float progress = label.Elapsed / Lifetime;

            if (progress >= 1f || camera == null)
            {
                label.Deactivate();
                return;
            }

            Vector3 worldPoint = label.Origin
                + Vector3.up * (RiseDistance * progress)
                + camera.transform.right * (label.Scatter * progress);

            Vector3 screenPoint = camera.WorldToScreenPoint(worldPoint);
            if (screenPoint.z <= 0f)
            {
                // Behind the camera: WorldToScreenPoint mirrors the result, which would
                // park the number on the opposite side of the screen.
                label.SetVisible(false);
                return;
            }

            label.SetVisible(true);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint);
            label.Rect.anchoredPosition = localPoint;

            float pop = progress < PopInPortion
                ? Mathf.Lerp(PopScale, 1f, progress / PopInPortion)
                : 1f;
            label.Rect.localScale = Vector3.one * (pop * label.SizeScale);

            float alpha = progress < FadeStart
                ? 1f
                : 1f - Mathf.InverseLerp(FadeStart, 1f, progress);
            label.SetAlpha(alpha);
        }
    }

    /// <summary>
    /// One pooled label. A plain class rather than a MonoBehaviour, so the runner can
    /// drive every live number in one loop instead of paying for an Update each.
    /// </summary>
    internal class FloatingLabel
    {
        private static readonly Color OutlineColor = new Color(0f, 0f, 0f, 0.85f);

        private readonly Text text;
        private readonly Outline outline;

        private Color baseColor;

        public FloatingLabel(RectTransform parent, int fontSize)
        {
            var go = new GameObject("DamageNumber", typeof(RectTransform), typeof(Text), typeof(Outline));
            go.transform.SetParent(parent, false);

            Rect = go.GetComponent<RectTransform>();
            Rect.anchorMin = new Vector2(0.5f, 0.5f);
            Rect.anchorMax = new Vector2(0.5f, 0.5f);
            Rect.pivot = new Vector2(0.5f, 0.5f);
            Rect.sizeDelta = new Vector2(220f, 60f);

            text = go.GetComponent<Text>();
            text.font = InventoryTheme.Font;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            outline = go.GetComponent<Outline>();
            // The world behind a number can be any brightness, so it carries its own contrast.
            outline.effectColor = OutlineColor;
            outline.effectDistance = new Vector2(2f, -2f);

            go.SetActive(false);
        }

        public RectTransform Rect { get; }
        public bool InUse { get; private set; }
        public Vector3 Origin { get; private set; }
        public float Scatter { get; private set; }
        public float SizeScale { get; private set; }
        public float Elapsed { get; set; }

        public void Activate(Vector3 origin, string content, Color color, float scatter, float sizeScale)
        {
            Origin = origin;
            Scatter = scatter;
            SizeScale = sizeScale;
            Elapsed = 0f;
            InUse = true;
            baseColor = color;

            text.text = content;
            text.color = color;
            Rect.localScale = Vector3.one * sizeScale;
            SetVisible(true);
        }

        public void Deactivate()
        {
            InUse = false;
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (text.gameObject.activeSelf != visible)
            {
                text.gameObject.SetActive(visible);
            }
        }

        public void SetAlpha(float alpha)
        {
            text.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            outline.effectColor = new Color(OutlineColor.r, OutlineColor.g, OutlineColor.b, OutlineColor.a * alpha);
        }
    }
}
