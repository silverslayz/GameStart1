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

        private Text[] objectiveRows;
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
            }
        }

        private void OnDisable()
        {
            if (questLog != null)
            {
                questLog.ObjectiveChanged -= OnObjectiveChanged;
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

            objectiveRows = new Text[questLog.Objectives.Count];

            for (int i = 0; i < questLog.Objectives.Count; i++)
            {
                GameObject rowGo = new GameObject("Objective_" + i, typeof(RectTransform));
                rowGo.transform.SetParent(objectivesContainer, false);
                var rt = rowGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(0f, -i * 28f);
                rt.sizeDelta = new Vector2(380f, 26f);

                var text = rowGo.AddComponent<Text>();
                text.fontSize = 16;
                text.color = Color.white;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                objectiveRows[i] = text;
                RefreshRow(i);
            }
        }

        private void OnObjectiveChanged(int index)
        {
            RefreshRow(index);
        }

        private void RefreshRow(int index)
        {
            if (objectiveRows == null || index < 0 || index >= objectiveRows.Length || objectiveRows[index] == null)
            {
                return;
            }

            QuestObjective objective = questLog.Objectives[index];
            string status = objective.IsComplete ? " (Complete)" : "";
            objectiveRows[index].text = $"- {objective.Description}: {objective.CurrentCount}/{objective.TargetCount}{status}";
        }
    }
}
