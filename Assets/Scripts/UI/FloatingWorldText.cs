using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameStart.UI
{
    /// <summary>
    /// Shared plumbing for any UI text pinned to a point in the world: one overlay canvas,
    /// one projection loop, one pool. <see cref="FloatingCombatText"/> (damage numbers) and
    /// <see cref="InteractPromptText"/> (the prompt over the current interactable) both ride
    /// on this rather than each standing up their own canvas.
    ///
    /// Self-bootstrapping like <see cref="Flow.SceneTransition"/>, so nothing needs to be
    /// placed in the shared scene and runtime-spawned objects work without wiring.
    /// </summary>
    internal static class FloatingWorldText
    {
        private static FloatingWorldTextRunner runner;

        public static FloatingWorldTextRunner Runner
        {
            get
            {
                if (runner == null)
                {
                    var go = new GameObject("FloatingWorldText");
                    Object.DontDestroyOnLoad(go);
                    runner = go.AddComponent<FloatingWorldTextRunner>();
                }

                return runner;
            }
        }

        /// <summary>
        /// A point just above whatever the target actually renders, so text clears the
        /// object's head instead of erupting from its pivot (usually at its feet).
        /// Returns a world position; <paramref name="verticalOffset"/> is that height
        /// relative to the transform, for callers that need to re-derive it as the target moves.
        /// </summary>
        public static Vector3 AnchorAbove(Transform target, float padding, out float verticalOffset)
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
                verticalOffset = 1.8f;
                return target.position + Vector3.up * verticalOffset;
            }

            verticalOffset = bounds.max.y + padding - target.position.y;
            return new Vector3(bounds.center.x, bounds.max.y + padding, bounds.center.z);
        }
    }

    /// <summary>
    /// Owns the overlay canvas and drives every live label from a single LateUpdate - after
    /// the camera has moved, so labels don't lag a frame behind what they're pinned to.
    /// </summary>
    internal class FloatingWorldTextRunner : MonoBehaviour
    {
        private const float Lifetime = 0.9f;
        private const float RiseDistance = 0.85f;
        private const float HorizontalScatter = 0.28f;
        private const float PopInPortion = 0.15f;
        private const float PopScale = 1.35f;
        private const float FadeStart = 0.55f;
        private const int RisingFontSize = 34;
        private const int PromptFontSize = 26;

        private readonly List<RisingText> rising = new List<RisingText>();

        private RectTransform canvasRect;

        private FloatingLabel promptLabel;
        private Transform promptTarget;
        private float promptOffset;
        private string promptContent;

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

        // ---- rising text (damage numbers) ----

        public void SpawnRising(Vector3 worldPosition, string content, Color color, float sizeScale)
        {
            RisingText entry = Rent();
            entry.Origin = worldPosition;
            entry.Scatter = Random.Range(-HorizontalScatter, HorizontalScatter);
            entry.SizeScale = sizeScale;
            entry.Elapsed = 0f;
            entry.InUse = true;
            entry.Label.SetContent(content, color);

            // Place it this frame so it never flashes at the canvas centre before LateUpdate.
            StepRising(entry, Camera.main, 0f);
        }

        private RisingText Rent()
        {
            foreach (RisingText pooled in rising)
            {
                if (!pooled.InUse)
                {
                    return pooled;
                }
            }

            var created = new RisingText { Label = new FloatingLabel(canvasRect, RisingFontSize, FontStyle.Bold) };
            rising.Add(created);
            return created;
        }

        // ---- interact prompt ----

        public void SetPrompt(Transform target, string content, Color color)
        {
            if (target == null || string.IsNullOrEmpty(content))
            {
                ClearPrompt();
                return;
            }

            if (promptLabel == null)
            {
                promptLabel = new FloatingLabel(canvasRect, PromptFontSize, FontStyle.Normal);
            }

            promptTarget = target;
            promptContent = content;
            FloatingWorldText.AnchorAbove(target, 0.35f, out promptOffset);
            promptLabel.SetContent(content, color);
            StepPrompt(Camera.main);
        }

        /// <summary>
        /// Retargets nothing, but refreshes the wording - a resource node that just went
        /// depleted, or a stack whose count changed, is still the same target with a
        /// different thing to say.
        /// </summary>
        public void UpdatePromptContent(string content, Color color)
        {
            if (promptTarget == null || promptLabel == null || content == promptContent)
            {
                return;
            }

            if (string.IsNullOrEmpty(content))
            {
                ClearPrompt();
                return;
            }

            promptContent = content;
            promptLabel.SetContent(content, color);
        }

        public void ClearPrompt()
        {
            promptTarget = null;
            promptContent = null;
            promptLabel?.SetVisible(false);
        }

        // ---- driving ----

        private void LateUpdate()
        {
            Camera camera = Camera.main;
            float delta = Time.deltaTime;

            foreach (RisingText entry in rising)
            {
                if (entry.InUse)
                {
                    StepRising(entry, camera, delta);
                }
            }

            StepPrompt(camera);
        }

        private void StepRising(RisingText entry, Camera camera, float delta)
        {
            entry.Elapsed += delta;
            float progress = entry.Elapsed / Lifetime;

            if (progress >= 1f || camera == null)
            {
                entry.InUse = false;
                entry.Label.SetVisible(false);
                return;
            }

            Vector3 worldPoint = entry.Origin
                + Vector3.up * (RiseDistance * progress)
                + camera.transform.right * (entry.Scatter * progress);

            if (!Project(camera, worldPoint, out Vector2 local))
            {
                entry.Label.SetVisible(false);
                return;
            }

            entry.Label.SetVisible(true);
            entry.Label.Rect.anchoredPosition = local;

            float pop = progress < PopInPortion
                ? Mathf.Lerp(PopScale, 1f, progress / PopInPortion)
                : 1f;
            entry.Label.Rect.localScale = Vector3.one * (pop * entry.SizeScale);

            float alpha = progress < FadeStart
                ? 1f
                : 1f - Mathf.InverseLerp(FadeStart, 1f, progress);
            entry.Label.SetAlpha(alpha);
        }

        private void StepPrompt(Camera camera)
        {
            if (promptLabel == null)
            {
                return;
            }

            // The target can be destroyed out from under us (a gathered node, a picked-up
            // item), which Unity reports as a null Transform rather than an event.
            if (promptTarget == null || camera == null)
            {
                ClearPrompt();
                return;
            }

            Vector3 worldPoint = promptTarget.position + Vector3.up * promptOffset;
            if (!Project(camera, worldPoint, out Vector2 local))
            {
                promptLabel.SetVisible(false);
                return;
            }

            promptLabel.SetVisible(true);
            promptLabel.Rect.anchoredPosition = local;
            promptLabel.Rect.localScale = Vector3.one;
            promptLabel.SetAlpha(1f);
        }

        /// <summary>World point to canvas-local point. False when it isn't in front of the camera.</summary>
        private bool Project(Camera camera, Vector3 worldPoint, out Vector2 local)
        {
            Vector3 screenPoint = camera.WorldToScreenPoint(worldPoint);
            if (screenPoint.z <= 0f)
            {
                // Behind the camera: WorldToScreenPoint mirrors the result, which would
                // park the text on the opposite side of the screen.
                local = default;
                return false;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out local);
            return true;
        }

        /// <summary>A pooled rising label plus the animation state driving it.</summary>
        private class RisingText
        {
            public FloatingLabel Label;
            public bool InUse;
            public Vector3 Origin;
            public float Scatter;
            public float SizeScale;
            public float Elapsed;
        }
    }

    /// <summary>
    /// One label's visuals. A plain class rather than a MonoBehaviour, so the runner can
    /// drive every live label in one loop instead of paying for an Update each.
    /// </summary>
    internal class FloatingLabel
    {
        private static readonly Color OutlineColor = new Color(0f, 0f, 0f, 0.85f);

        private readonly Text text;
        private readonly Outline outline;

        private Color baseColor;

        public FloatingLabel(RectTransform parent, int fontSize, FontStyle style)
        {
            var go = new GameObject("FloatingLabel", typeof(RectTransform), typeof(Text), typeof(Outline));
            go.transform.SetParent(parent, false);

            Rect = go.GetComponent<RectTransform>();
            Rect.anchorMin = new Vector2(0.5f, 0.5f);
            Rect.anchorMax = new Vector2(0.5f, 0.5f);
            Rect.pivot = new Vector2(0.5f, 0.5f);
            Rect.sizeDelta = new Vector2(420f, 60f);

            text = go.GetComponent<Text>();
            text.font = InventoryTheme.Font;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            outline = go.GetComponent<Outline>();
            // The world behind the text can be any brightness, so it carries its own contrast.
            outline.effectColor = OutlineColor;
            outline.effectDistance = new Vector2(2f, -2f);

            go.SetActive(false);
        }

        public RectTransform Rect { get; }

        public void SetContent(string content, Color color)
        {
            baseColor = color;
            text.text = content;
            text.color = color;
            Rect.localScale = Vector3.one;
            SetVisible(true);
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
