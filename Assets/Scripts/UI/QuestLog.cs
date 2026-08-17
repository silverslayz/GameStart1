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
        public event Action<int> QuestAdded;

        public IReadOnlyList<QuestObjective> Objectives => objectives;

        public bool HasQuest(string description)
        {
            foreach (QuestObjective objective in objectives)
            {
                if (objective.Description == description)
                {
                    return true;
                }
            }

            return false;
        }

        public int FindObjectiveIndex(string description)
        {
            for (int i = 0; i < objectives.Count; i++)
            {
                if (objectives[i].Description == description)
                {
                    return i;
                }
            }

            return -1;
        }

        public void AddQuest(QuestObjective objective)
        {
            objectives.Add(objective);
            QuestAdded?.Invoke(objectives.Count - 1);
        }

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
