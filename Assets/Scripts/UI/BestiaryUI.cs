using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using GameStart.Combat;

namespace GameStart.UI
{
    public class BestiaryUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform rowContainer;
        [SerializeField] private PlayerBestiary bestiary;

        private const float RowHeight = 26f;
        private const int WeaknessThreshold = 5;

        private readonly Dictionary<string, Text> rows = new Dictionary<string, Text>();
        private bool isOpen;

        private void OnEnable()
        {
            if (bestiary != null)
            {
                bestiary.KillRecorded += OnKillRecorded;
            }
        }

        private void OnDisable()
        {
            if (bestiary != null)
            {
                bestiary.KillRecorded -= OnKillRecorded;
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
            {
                isOpen = !isOpen;
                if (isOpen)
                {
                    RefreshAll();
                }

                if (panel != null)
                {
                    panel.SetActive(isOpen);
                }
            }
        }

        private void OnKillRecorded(string monsterId, int count)
        {
            RefreshRow(monsterId, count);
        }

        private void RefreshAll()
        {
            if (bestiary == null || rowContainer == null)
            {
                return;
            }

            foreach (var pair in bestiary.KillCounts)
            {
                RefreshRow(pair.Key, pair.Value);
            }
        }

        private void RefreshRow(string monsterId, int count)
        {
            if (rowContainer == null)
            {
                return;
            }

            if (!rows.TryGetValue(monsterId, out Text text))
            {
                GameObject rowGo = new GameObject("Bestiary_" + monsterId, typeof(RectTransform));
                rowGo.transform.SetParent(rowContainer, false);
                var rt = rowGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(0f, -rows.Count * RowHeight);
                rt.sizeDelta = new Vector2(420f, 24f);

                text = rowGo.AddComponent<Text>();
                text.fontSize = 16;
                text.color = Color.white;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                rows[monsterId] = text;
            }

            bool discovered = count >= WeaknessThreshold;
            string status = discovered ? "Weakness discovered (+25% dmg)" : $"{count}/{WeaknessThreshold} to discover weakness";
            text.text = $"{monsterId}: {count} kills - {status}";
            text.color = discovered ? new Color(1f, 0.85f, 0.3f) : Color.white;
        }
    }
}
