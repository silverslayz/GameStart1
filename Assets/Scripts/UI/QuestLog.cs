using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameStart.UI
{
    public class QuestLog : MonoBehaviour
    {
        [SerializeField]
        private List<QuestObjective> objectives = new List<QuestObjective>
        {
            new QuestObjective { Description = "Collect F-rank gems from monsters near the starting town", TargetCount = 10, CurrentCount = 0 }
        };

        public event Action<int> ObjectiveChanged;

        public IReadOnlyList<QuestObjective> Objectives => objectives;

        public void AddProgress(int objectiveIndex, int amount)
        {
            if (objectiveIndex < 0 || objectiveIndex >= objectives.Count || amount <= 0)
            {
                return;
            }

            QuestObjective objective = objectives[objectiveIndex];
            objective.CurrentCount = Mathf.Min(objective.TargetCount, objective.CurrentCount + amount);
            ObjectiveChanged?.Invoke(objectiveIndex);
        }
    }
}
