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

        /// <summary>Progress earned during play. Anything that celebrates a completion listens here.</summary>
        public event Action<int> ObjectiveChanged;

        /// <summary>
        /// Progress restored from a save. Deliberately separate from ObjectiveChanged:
        /// loading a finished objective is not the player finishing it, and treating the
        /// two alike fires completion rewards on every load.
        /// </summary>
        public event Action<int> ObjectiveRestored;

        /// <summary>Raised after progress is wiped, so completion triggers can arm again.</summary>
        public event Action ObjectivesReset;

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

        /// <summary>Restores an objective's progress from saved data, adding it if it doesn't already exist.</summary>
        public void SetObjectiveProgress(string description, int targetCount, int currentCount)
        {
            int index = FindObjectiveIndex(description);
            if (index >= 0)
            {
                objectives[index].TargetCount = targetCount;
                objectives[index].CurrentCount = currentCount;
                ObjectiveRestored?.Invoke(index);
            }
            else
            {
                AddQuest(new QuestObjective { Description = description, TargetCount = targetCount, CurrentCount = currentCount });
            }
        }

        /// <summary>Wipes progress on every objective, for New Game and for restarting a run.</summary>
        public void ResetObjectives()
        {
            foreach (QuestObjective objective in objectives)
            {
                objective.CurrentCount = 0;
            }

            ObjectivesReset?.Invoke();
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
