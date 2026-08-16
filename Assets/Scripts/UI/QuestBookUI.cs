using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameStart.UI
{
    public class QuestBookUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform objectivesContainer;
        [SerializeField] private QuestLog questLog;

        private const float RowHeight = 28f;

        private readonly List<Text> objectiveRows = new List<Text>();
        private bool isOpen;

        private void Start()
        {
            BuildObjectiveRows();
        }

        private void OnEnable()
        {
            if (questLog != null)
            {
                questLog.ObjectiveChanged += OnObjectiveChanged;
                questLog.QuestAdded += OnQuestAdded;
            }
        }

        private void OnDisable()
        {
            if (questLog != null)
            {
                questLog.ObjectiveChanged -= OnObjectiveChanged;
                questLog.QuestAdded -= OnQuestAdded;
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
            {
                isOpen = !isOpen;
                if (panel != null)
                {
                    panel.SetActive(isOpen);
                }
            }
        }

        private void BuildObjectiveRows()
        {
            if (questLog == null || objectivesContainer == null)
            {
                return;
            }

            for (int i = 0; i < questLog.Objectives.Count; i++)
            {
                AddRow(i);
            }
        }

        private void OnQuestAdded(int index)
        {
            AddRow(index);
        }

        private void AddRow(int index)
        {
            GameObject rowGo = new GameObject("Objective_" + index, typeof(RectTransform));
            rowGo.transform.SetParent(objectivesContainer, false);
            var rt = rowGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(0f, -index * RowHeight);
            rt.sizeDelta = new Vector2(380f, 26f);

            var text = rowGo.AddComponent<Text>();
            text.fontSize = 16;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            objectiveRows.Add(text);
            RefreshRow(index);
        }

        private void OnObjectiveChanged(int index)
        {
            RefreshRow(index);
        }

        private void RefreshRow(int index)
        {
            if (index < 0 || index >= objectiveRows.Count || objectiveRows[index] == null)
            {
                return;
            }

            QuestObjective objective = questLog.Objectives[index];
            string status = objective.IsComplete ? " (Complete)" : "";
            objectiveRows[index].text = $"- {objective.Description}: {objective.CurrentCount}/{objective.TargetCount}{status}";
        }
    }
}
