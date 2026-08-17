using UnityEngine;
using GameStart.Flow;

namespace GameStart.UI
{
    public class QuestVictoryTrigger : MonoBehaviour
    {
        [SerializeField] private QuestLog questLog;
        [SerializeField] private VictorySequenceUI victorySequence;
        [SerializeField] private int objectiveIndex;

        private bool hasFired;

        private void Awake()
        {
            // Lives on a scene canvas, so a prefab instance starts with this null.
            victorySequence = SceneLink.Resolve(victorySequence);
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

        private void OnObjectiveChanged(int index)
        {
            if (hasFired || index != objectiveIndex)
            {
                return;
            }

            QuestObjective objective = questLog.Objectives[index];
            if (!objective.IsComplete)
            {
                return;
            }

            hasFired = true;
            victorySequence.Show($"Objective Complete!\n{objective.Description}");
        }
    }
}
