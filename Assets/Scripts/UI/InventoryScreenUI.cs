using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using GameStart.Class;

namespace GameStart.UI
{
    /// <summary>
    /// Full-screen sci-fi inventory built entirely in code: header, inventory grid,
    /// character panel with live model preview and equipment slots, and a detail panel
    /// driven by whatever is selected.
    ///
    /// Built at runtime rather than authored in the scene because the previous inventory was
    /// hand-wired across 26 scene objects, which made any layout change a YAML edit. This
    /// supersedes the scene-authored <see cref="InventoryUI"/>, which it disables on startup.
    /// </summary>
    public class InventoryScreenUI : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private PlayerEquipment equipment;

        /// <summary>Optional: assign the camera look script so it stops while the screen is open.</summary>
        [SerializeField] private Behaviour cameraLookController;

        private PlayerInput playerInput;

        private const int ReferenceWidth = 1920;
        private const int ReferenceHeight = 1080;

        /// <summary>Fallback grid size used only if the inventory can't be resolved at build time.</summary>
        private const int DefaultSlotCount = 20;

        private const float ColumnGap = 52f;
        private const float BodyTop = -172f;
        private const int EquipSlotSize = 66;
        private const float CharacterPanelWidth = 430f;

        private GameObject root;
        private readonly List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
        private readonly List<EquipmentSlotUI> equipUIs = new List<EquipmentSlotUI>();

        private InventorySlotUI selectedSlot;
        private EquipmentSlotUI selectedEquip;
        private bool isOpen;

        private CharacterPreview preview;
        private InventoryDropZoneUI dropZone;

        private Text detailTitle;
        private Text detailSubtitle;
        private Text detailEmptyHint;
        private RectTransform detailBody;
        private Text weightValue;
        private Text stackValue;
        private Text totalWeightValue;

        /// <summary>
        /// Built in Start rather than Awake: the grid sizes itself from
        /// <see cref="PlayerInventory.MainSlots"/>, which that component only allocates in its
        /// own Awake. Building here guarantees every Awake has already run.
        /// </summary>
        private void Start()
        {
            EnsureBuilt();
        }

        private void EnsureBuilt()
        {
            // Guarded so a second call can't stack a duplicate canvas on top of the first.
            if (root != null)
            {
                return;
            }

            if (inventory == null)
            {
                inventory = FindAnyObjectByType<PlayerInventory>();
            }

            if (equipment == null && inventory != null)
            {
                equipment = inventory.GetComponent<PlayerEquipment>();
                if (equipment == null)
                {
                    // Added at runtime so the screen works without a scene edit.
                    equipment = inventory.gameObject.AddComponent<PlayerEquipment>();
                }
            }

            if (inventory != null)
            {
                playerInput = inventory.GetComponent<PlayerInput>();
            }

            DisableLegacyInventoryPanel();
            EnsureEventSystem();
            Build();

            // Set the closed state directly rather than via SetOpen: at startup the class
            // selection screen owns the cursor, and re-locking it here would break it.
            isOpen = false;
            root.SetActive(false);
        }

        private void OnEnable()
        {
            InventorySlotUI.SlotClicked += OnSlotClicked;
            EquipmentSlotUI.SlotClicked += OnEquipClicked;

            if (inventory != null)
            {
                inventory.InventoryChanged += Refresh;
            }

            if (equipment != null)
            {
                equipment.EquipmentChanged += Refresh;
            }
        }

        private void OnDisable()
        {
            InventorySlotUI.SlotClicked -= OnSlotClicked;
            EquipmentSlotUI.SlotClicked -= OnEquipClicked;

            if (inventory != null)
            {
                inventory.InventoryChanged -= Refresh;
            }

            if (equipment != null)
            {
                equipment.EquipmentChanged -= Refresh;
            }
        }

        /// <summary>
        /// The older inventory listens for the same key and would open a second panel
        /// underneath this one, so it is switched off wherever it still exists in a scene.
        /// </summary>
        private static void DisableLegacyInventoryPanel()
        {
            var legacy = FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
            if (legacy != null)
            {
                legacy.enabled = false;
            }
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                DontDestroyOnLoad(go);
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.iKey.wasPressedThisFrame)
            {
                SetOpen(!isOpen);
                return;
            }

            if (!isOpen)
            {
                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                SetOpen(false);
                return;
            }

            HandleGridNavigation(keyboard);
        }

        private void HandleGridNavigation(Keyboard keyboard)
        {
            int delta = 0;
            if (keyboard.rightArrowKey.wasPressedThisFrame) delta = 1;
            else if (keyboard.leftArrowKey.wasPressedThisFrame) delta = -1;
            else if (keyboard.downArrowKey.wasPressedThisFrame) delta = InventoryTheme.GridColumns;
            else if (keyboard.upArrowKey.wasPressedThisFrame) delta = -InventoryTheme.GridColumns;

            if (delta == 0 || slotUIs.Count == 0)
            {
                return;
            }

            int current = selectedSlot != null ? slotUIs.IndexOf(selectedSlot) : -1;
            int next = current < 0 ? 0 : Mathf.Clamp(current + delta, 0, slotUIs.Count - 1);
            Select(slotUIs[next]);
        }

        /// <summary>Selects the nth grid cell. Public so other input paths can drive the grid.</summary>
        public void SelectIndex(int index)
        {
            if (index >= 0 && index < slotUIs.Count)
            {
                Select(slotUIs[index]);
            }
        }

        public void SetOpen(bool open)
        {
            if (isOpen == open)
            {
                return;
            }

            isOpen = open;
            if (root != null)
            {
                root.SetActive(open);
            }

            // Gameplay runs with the cursor locked and hidden, so it has to be released for
            // the grid to be clickable at all. Same pattern the lore reader already uses.
            if (playerInput != null)
            {
                playerInput.enabled = !open;
            }

            if (cameraLookController != null)
            {
                cameraLookController.enabled = !open;
            }

            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = open;

            if (open)
            {
                Refresh();
            }
        }

        private void OnSlotClicked(InventorySlotUI slot)
        {
            if (isOpen && slotUIs.Contains(slot))
            {
                Select(slot);
            }
        }

        private void OnEquipClicked(EquipmentSlotUI slot)
        {
            if (isOpen && equipUIs.Contains(slot))
            {
                SelectEquip(slot);
            }
        }

        private void ClearSelection()
        {
            if (selectedSlot != null)
            {
                selectedSlot.SetSelected(false);
                selectedSlot = null;
            }

            if (selectedEquip != null)
            {
                selectedEquip.SetSelected(false);
                selectedEquip = null;
            }
        }

        private void Select(InventorySlotUI slot)
        {
            ClearSelection();
            selectedSlot = slot;
            if (selectedSlot != null)
            {
                selectedSlot.SetSelected(true);
            }

            RefreshDetail();
        }

        private void SelectEquip(EquipmentSlotUI slot)
        {
            ClearSelection();
            selectedEquip = slot;
            if (selectedEquip != null)
            {
                selectedEquip.SetSelected(true);
            }

            RefreshDetail();
        }

        private void Refresh()
        {
            if (inventory != null)
            {
                var slots = inventory.MainSlots;
                if (slots != null)
                {
                    for (int i = 0; i < slotUIs.Count && i < slots.Count; i++)
                    {
                        slotUIs[i].SetSlot(slots[i]);
                    }
                }
            }

            for (int i = 0; i < equipUIs.Count; i++)
            {
                equipUIs[i].Refresh();
            }

            RefreshDetail();
        }

        private void RefreshDetail()
        {
            if (detailTitle == null)
            {
                return;
            }

            GearItem item = default;
            int count = 0;
            string location = null;
            bool hasItem = false;
            bool anySelection = selectedSlot != null || selectedEquip != null;

            if (selectedSlot != null && inventory != null)
            {
                InventorySlot slot = inventory.GetSlot(selectedSlot.IsHotbarSlot, selectedSlot.SlotIndex);
                if (slot != null && !slot.IsEmpty)
                {
                    item = slot.Item;
                    count = slot.Count;
                    location = selectedSlot.IsHotbarSlot ? "HOTBAR" : "MAIN INVENTORY";
                    hasItem = true;
                }
            }
            else if (selectedEquip != null && equipment != null && equipment.IsEquipped(selectedEquip.SlotType))
            {
                item = equipment.GetEquipped(selectedEquip.SlotType);
                count = 1;
                location = "EQUIPPED  //  " + PlayerEquipment.DisplayName(selectedEquip.SlotType).ToUpperInvariant();
                hasItem = true;
            }

            detailEmptyHint.gameObject.SetActive(!hasItem);
            detailBody.gameObject.SetActive(hasItem);
            detailTitle.gameObject.SetActive(hasItem);
            detailSubtitle.gameObject.SetActive(hasItem);

            if (!hasItem)
            {
                detailEmptyHint.text = InventoryTheme.Spaced(anySelection ? "EMPTY SLOT" : "SELECT A SLOT");
                return;
            }

            detailTitle.text = count > 1
                ? $"{count}X  //  {item.Name.ToUpperInvariant()}"
                : item.Name.ToUpperInvariant();
            detailSubtitle.text = location;

            weightValue.text = $"{item.Weight:0.##}";
            stackValue.text = inventory != null ? $"{count} / {inventory.MaxStackSize}" : count.ToString();
            totalWeightValue.text = $"{item.Weight * count:0.##}";
        }

        // ---------------------------------------------------------------- building

        private void Build()
        {
            root = new GameObject("InventoryScreen", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Above the HUD and class-select canvases, which would otherwise draw over the screen.
            canvas.sortingOrder = 1000;

            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = 0.5f;

            var backdrop = InventoryTheme.CreateImage("Backdrop", root.transform, InventoryTheme.Backdrop);
            InventoryTheme.Fill(backdrop.rectTransform);

            var content = InventoryTheme.CreateRect("Content", root.transform);
            content.anchorMin = Vector2.zero;
            content.anchorMax = Vector2.one;
            content.offsetMin = new Vector2(96f, 56f);
            content.offsetMax = new Vector2(-96f, -48f);

            float gridWidth = InventoryTheme.GridColumns * InventoryTheme.SlotSize
                              + (InventoryTheme.GridColumns - 1) * InventoryTheme.SlotGap;

            BuildHeader(content);
            BuildGrid(content, gridWidth);
            BuildCharacterPanel(content, gridWidth + ColumnGap);
            BuildDetailPanel(content, gridWidth + ColumnGap + CharacterPanelWidth + ColumnGap);
            BuildFooter(content);

            // Parented to the canvas root rather than the content rect, and built last, so
            // it floats over every panel instead of being clipped by the one it started in.
            // Lives on the root object, so closing the screen deactivates it and its
            // OnDisable clears any tooltip left showing under the cursor.
            var tooltip = root.AddComponent<InventoryTooltipUI>();
            tooltip.Build(root.GetComponent<RectTransform>());
        }

        private void BuildHeader(RectTransform parent)
        {
            var header = InventoryTheme.CreateRect("Header", parent);
            InventoryTheme.Anchor(header, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                Vector2.zero, new Vector2(0f, 150f));

            var breadcrumb = InventoryTheme.CreateText("Breadcrumb", header, "AETHERFALL  //  ", 20,
                InventoryTheme.BreadcrumbDim, TextAnchor.UpperLeft);
            InventoryTheme.Anchor(breadcrumb.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(4f, -6f), new Vector2(400f, 26f));

            var breadcrumbActive = InventoryTheme.CreateText("BreadcrumbActive", header, "INVENTORY", 20,
                InventoryTheme.Accent, TextAnchor.UpperLeft);
            InventoryTheme.Anchor(breadcrumbActive.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(148f, -6f), new Vector2(400f, 26f));

            var title = InventoryTheme.CreateText("Title", header, InventoryTheme.Spaced("INVENTORY"), 58,
                InventoryTheme.TitleText, TextAnchor.UpperLeft, FontStyle.Bold);
            InventoryTheme.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -34f), new Vector2(900f, 74f));

            // Transparent hit area carries the Button; the glyph itself stays non-raycasting
            // so the whole square is clickable rather than just the stroke of the ×.
            var closeHit = InventoryTheme.CreateImage("Close", header, new Color(1f, 1f, 1f, 0f));
            InventoryTheme.Anchor(closeHit.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -30f), new Vector2(56f, 56f));

            var closeGlyph = InventoryTheme.CreateText("Glyph", closeHit.transform, "×", 52,
                InventoryTheme.BreadcrumbDim, TextAnchor.MiddleCenter);
            InventoryTheme.Fill(closeGlyph.rectTransform);

            var closeButton = closeHit.gameObject.AddComponent<Button>();
            closeButton.targetGraphic = closeHit;
            var colors = closeButton.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.08f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0.16f);
            closeButton.colors = colors;
            closeButton.onClick.AddListener(() => SetOpen(false));

            var rule = InventoryTheme.CreateImage("Rule", header, InventoryTheme.Divider, raycast: false);
            InventoryTheme.Anchor(rule.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 6f), new Vector2(0f, 1f));
        }

        private void BuildGrid(RectTransform parent, float width)
        {
            var slots = inventory != null ? inventory.MainSlots : null;
            int slotCount = slots != null ? slots.Count : DefaultSlotCount;
            int columns = InventoryTheme.GridColumns;
            int rows = Mathf.CeilToInt(slotCount / (float)columns);
            float height = rows * InventoryTheme.SlotSize + (rows - 1) * InventoryTheme.SlotGap;

            const float pad = 16f;
            var panel = InventoryTheme.CreateImage("GridPanel", parent, InventoryTheme.PanelFill, raycast: false);
            InventoryTheme.Anchor(panel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(-pad, BodyTop + pad), new Vector2(width + pad * 2f, height + pad * 2f));
            InventoryTheme.CreateFrame("PanelBorder", panel.transform, InventoryTheme.Divider, 1f);

            var label = InventoryTheme.CreateText("GridLabel", parent, InventoryTheme.Spaced("CARRIED"), 17,
                InventoryTheme.SectionLabel, TextAnchor.UpperLeft, FontStyle.Bold);
            InventoryTheme.Anchor(label.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, BodyTop + pad + 26f), new Vector2(300f, 22f));

            var gridRect = InventoryTheme.CreateRect("Grid", parent);
            InventoryTheme.Anchor(gridRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, BodyTop), new Vector2(width, height));

            var grid = gridRect.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(InventoryTheme.SlotSize, InventoryTheme.SlotSize);
            grid.spacing = new Vector2(InventoryTheme.SlotGap, InventoryTheme.SlotGap);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.childAlignment = TextAnchor.UpperLeft;

            for (int i = 0; i < slotCount; i++)
            {
                slotUIs.Add(BuildSlot(gridRect, i));
            }

            BuildDropZone(parent, width, BodyTop - height - 34f);
        }

        /// <summary>
        /// Explicit discard target. Everything else snaps back, so items can only leave the
        /// inventory through this one deliberate gesture.
        /// </summary>
        private void BuildDropZone(RectTransform parent, float width, float y)
        {
            var zone = new GameObject("DropZone", typeof(RectTransform), typeof(Image));
            zone.transform.SetParent(parent, false);
            var zoneRect = zone.GetComponent<RectTransform>();
            InventoryTheme.Anchor(zoneRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, y), new Vector2(width, 52f));

            var bg = zone.GetComponent<Image>();
            bg.color = InventoryTheme.SlotEmpty;

            var frame = InventoryTheme.CreateFrame("Border", zone.transform, InventoryTheme.SlotBorderEmpty);

            var label = InventoryTheme.CreateText("Label", zone.transform,
                InventoryTheme.Spaced("DROP HERE"), 15, InventoryTheme.BodyText, TextAnchor.MiddleCenter);
            InventoryTheme.Fill(label.rectTransform);

            var hint = InventoryTheme.CreateText("Hint", parent, "Drag an item here to drop it  ·  hold SHIFT for the whole stack", 13,
                InventoryTheme.HintText, TextAnchor.UpperLeft);
            InventoryTheme.Anchor(hint.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, y - 56f), new Vector2(width + 60f, 20f));

            dropZone = zone.AddComponent<InventoryDropZoneUI>();
            dropZone.Configure(inventory, inventory != null ? inventory.transform : null, bg, frame, label);
        }

        private InventorySlotUI BuildSlot(Transform parent, int index)
        {
            var cell = new GameObject($"Slot{index}", typeof(RectTransform), typeof(Image), typeof(Outline));
            cell.transform.SetParent(parent, false);

            var outline = cell.GetComponent<Outline>();
            outline.effectColor = Color.clear;
            outline.effectDistance = new Vector2(2f, -2f);

            var name = InventoryTheme.CreateText("Name", cell.transform, "", 13,
                InventoryTheme.ValueText, TextAnchor.UpperLeft);
            InventoryTheme.Fill(name.rectTransform, 6f);

            var count = InventoryTheme.CreateText("Count", cell.transform, "", 15,
                InventoryTheme.Accent, TextAnchor.LowerRight, FontStyle.Bold);
            InventoryTheme.Fill(count.rectTransform, 5f);

            // Added last so Awake sees the Image/Outline it expects.
            var slotUI = cell.AddComponent<InventorySlotUI>();
            slotUI.Configure(inventory, false, index);
            slotUI.BindLabels(name, count);
            return slotUI;
        }

        private void BuildCharacterPanel(RectTransform parent, float left)
        {
            const float pad = 16f;
            const float panelHeight = 430f;

            var panel = InventoryTheme.CreateImage("CharacterPanel", parent, InventoryTheme.PanelFill, raycast: false);
            InventoryTheme.Anchor(panel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(left - pad, BodyTop + pad), new Vector2(CharacterPanelWidth + pad * 2f, panelHeight));
            InventoryTheme.CreateFrame("PanelBorder", panel.transform, InventoryTheme.Divider, 1f);

            var label = InventoryTheme.CreateText("EquipLabel", parent, InventoryTheme.Spaced("EQUIPMENT"), 17,
                InventoryTheme.SectionLabel, TextAnchor.UpperLeft, FontStyle.Bold);
            InventoryTheme.Anchor(label.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(left, BodyTop + pad + 26f), new Vector2(300f, 22f));

            // Live model render sits between the two columns of gear slots.
            float portraitWidth = CharacterPanelWidth - (EquipSlotSize + 12f) * 2f;
            var portrait = InventoryTheme.CreateRect("Portrait", parent);
            InventoryTheme.Anchor(portrait, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(left + EquipSlotSize + 12f, BodyTop - 12f), new Vector2(portraitWidth, panelHeight - 60f));

            BuildPortrait(portrait, Mathf.RoundToInt(portraitWidth), Mathf.RoundToInt(panelHeight - 60f));

            // Left column reads head-to-toe; right column carries weapons and extras.
            var leftSlots = new[]
            {
                EquipmentSlotType.Head, EquipmentSlotType.Chest, EquipmentSlotType.Legs, EquipmentSlotType.Boots
            };
            var rightSlots = new[]
            {
                EquipmentSlotType.MainHand, EquipmentSlotType.OffHand, EquipmentSlotType.Cloak, EquipmentSlotType.Accessory
            };

            for (int i = 0; i < leftSlots.Length; i++)
            {
                float y = BodyTop - 14f - i * (EquipSlotSize + 26f);
                equipUIs.Add(BuildEquipSlot(parent, leftSlots[i], left, y));
            }

            for (int i = 0; i < rightSlots.Length; i++)
            {
                float y = BodyTop - 14f - i * (EquipSlotSize + 26f);
                equipUIs.Add(BuildEquipSlot(parent, rightSlots[i], left + CharacterPanelWidth - EquipSlotSize, y));
            }
        }

        private void BuildPortrait(RectTransform holder, int width, int height)
        {
            var backing = InventoryTheme.CreateImage("PortraitBacking", holder, new Color(0.055f, 0.075f, 0.106f, 0.9f), raycast: false);
            InventoryTheme.Fill(backing.rectTransform);

            Transform model = FindPlayerModel();
            if (model == null)
            {
                var missing = InventoryTheme.CreateText("NoModel", holder, InventoryTheme.Spaced("NO MODEL"), 14,
                    InventoryTheme.BreadcrumbDim, TextAnchor.MiddleCenter);
                InventoryTheme.Fill(missing.rectTransform);
                return;
            }

            preview = gameObject.AddComponent<CharacterPreview>();
            if (!preview.Build(model, width * 2, height * 2))
            {
                return;
            }

            var raw = new GameObject("PortraitRender", typeof(RectTransform), typeof(RawImage));
            raw.transform.SetParent(holder, false);
            var rawImage = raw.GetComponent<RawImage>();
            rawImage.texture = preview.Texture;
            rawImage.raycastTarget = false;
            InventoryTheme.Fill(rawImage.rectTransform);
        }

        /// <summary>The visual model is the Animator-bearing child of the player, not the root.</summary>
        private Transform FindPlayerModel()
        {
            if (inventory == null)
            {
                return null;
            }

            var animator = inventory.GetComponentInChildren<Animator>(true);
            return animator != null ? animator.transform : null;
        }

        private EquipmentSlotUI BuildEquipSlot(RectTransform parent, EquipmentSlotType type, float x, float y)
        {
            var label = InventoryTheme.CreateText($"{type}Label", parent, PlayerEquipment.DisplayName(type), 13,
                InventoryTheme.BodyText, TextAnchor.LowerLeft);
            InventoryTheme.Anchor(label.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, y + 18f), new Vector2(160f, 18f));

            var cell = new GameObject($"Equip{type}", typeof(RectTransform), typeof(Image));
            cell.transform.SetParent(parent, false);
            var cellRect = cell.GetComponent<RectTransform>();
            InventoryTheme.Anchor(cellRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, y), new Vector2(EquipSlotSize, EquipSlotSize));

            var bg = cell.GetComponent<Image>();
            bg.color = InventoryTheme.SlotEmpty;

            var frame = InventoryTheme.CreateFrame("Border", cell.transform, InventoryTheme.SlotBorderEmpty);

            var itemLabel = InventoryTheme.CreateText("Item", cell.transform, "", 12,
                InventoryTheme.ValueText, TextAnchor.MiddleCenter);
            InventoryTheme.Fill(itemLabel.rectTransform, 4f);

            var slotUI = cell.AddComponent<EquipmentSlotUI>();
            slotUI.Configure(inventory, equipment, type, bg, frame, itemLabel);
            return slotUI;
        }

        private void BuildDetailPanel(RectTransform parent, float left)
        {
            var panel = InventoryTheme.CreateRect("Detail", parent);
            panel.anchorMin = new Vector2(0f, 0f);
            panel.anchorMax = new Vector2(1f, 1f);
            panel.offsetMin = new Vector2(left, 64f);
            panel.offsetMax = new Vector2(0f, BodyTop);

            var accentBar = InventoryTheme.CreateImage("AccentBar", panel, InventoryTheme.Accent, raycast: false);
            InventoryTheme.Anchor(accentBar.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(3f, 190f));

            detailEmptyHint = InventoryTheme.CreateText("EmptyHint", panel, InventoryTheme.Spaced("SELECT A SLOT"), 20,
                InventoryTheme.BreadcrumbDim, TextAnchor.UpperLeft);
            InventoryTheme.Anchor(detailEmptyHint.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(26f, -4f), new Vector2(600f, 28f));

            detailTitle = InventoryTheme.CreateText("DetailTitle", panel, "", 34,
                InventoryTheme.TitleText, TextAnchor.UpperLeft, FontStyle.Bold);
            InventoryTheme.Anchor(detailTitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(26f, 0f), new Vector2(760f, 44f));

            detailSubtitle = InventoryTheme.CreateText("DetailSubtitle", panel, "", 18,
                InventoryTheme.BreadcrumbDim, TextAnchor.UpperLeft);
            InventoryTheme.Anchor(detailSubtitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(26f, -48f), new Vector2(600f, 24f));

            detailBody = InventoryTheme.CreateRect("DetailBody", panel);
            detailBody.pivot = new Vector2(0f, 1f);
            detailBody.anchorMin = new Vector2(0f, 1f);
            detailBody.anchorMax = new Vector2(1f, 1f);
            detailBody.offsetMin = new Vector2(26f, -292f);
            detailBody.offsetMax = new Vector2(-26f, -92f);

            var logisticsLabel = InventoryTheme.CreateText("LogisticsLabel", detailBody,
                InventoryTheme.Spaced("LOGISTICS"), 19, InventoryTheme.SectionLabel, TextAnchor.UpperLeft, FontStyle.Bold);
            InventoryTheme.Anchor(logisticsLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(400f, 26f));

            var logisticsRule = InventoryTheme.CreateImage("LogisticsRule", detailBody, InventoryTheme.Divider, raycast: false);
            InventoryTheme.Anchor(logisticsRule.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -34f), new Vector2(0f, 1f));

            weightValue = BuildStatRow(detailBody, "Weight", -52f);
            stackValue = BuildStatRow(detailBody, "Stack", -82f);
            totalWeightValue = BuildStatRow(detailBody, "Total weight", -112f);
        }

        private Text BuildStatRow(RectTransform parent, string label, float y)
        {
            var labelText = InventoryTheme.CreateText($"{label}Label", parent, label, 18,
                InventoryTheme.BodyText, TextAnchor.UpperLeft);
            InventoryTheme.Anchor(labelText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, y), new Vector2(300f, 24f));

            var valueText = InventoryTheme.CreateText($"{label}Value", parent, "-", 18,
                InventoryTheme.ValueText, TextAnchor.UpperLeft);
            InventoryTheme.Anchor(valueText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(240f, y), new Vector2(300f, 24f));

            return valueText;
        }

        private void BuildFooter(RectTransform parent)
        {
            var footer = InventoryTheme.CreateRect("Footer", parent);
            InventoryTheme.Anchor(footer, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                Vector2.zero, new Vector2(0f, 40f));

            var rule = InventoryTheme.CreateImage("FooterRule", footer, InventoryTheme.Divider, raycast: false);
            InventoryTheme.Anchor(rule.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                Vector2.zero, new Vector2(0f, 1f));

            var hints = InventoryTheme.CreateText("Hints", footer,
                "LEFT CLICK  Select        DRAG  Move or equip        DRAG TO DROP ZONE  Discard        I / ESC  Close        ARROWS  Navigate", 17,
                InventoryTheme.HintText, TextAnchor.MiddleLeft);
            InventoryTheme.Anchor(hints.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -4f), Vector2.zero);
            hints.rectTransform.offsetMin = new Vector2(2f, 0f);
            hints.rectTransform.offsetMax = new Vector2(-2f, -6f);
        }
    }
}
