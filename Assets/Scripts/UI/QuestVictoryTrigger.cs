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
        private bool wasComplete;

        private void Awake()
        {
            // Lives on a scene canvas, so a prefab instance starts with this null.
            victorySequence = SceneLink.Resolve(victorySequence);
        }

        private void Start()
        {
            // Whatever the scene asset or a loaded save already says is the baseline, not
            // something the player just did. The scene ships this objective at 10/10, which
            // is why the sequence played the moment a run started.
            wasComplete = IsObjectiveComplete();
            hasFired = wasComplete;
        }

        private void OnEnable()
        {
            if (questLog != null)
            {
                // ObjectiveChanged only - not ObjectiveRestored. Loading a save with this
                // objective already done must not replay the victory sequence.
                questLog.ObjectiveChanged += OnObjectiveChanged;
                questLog.ObjectivesReset += OnObjectivesReset;
            }
        }

        private void OnDisable()
        {
            if (questLog != null)
            {
                questLog.ObjectiveChanged -= OnObjectiveChanged;
                questLog.ObjectivesReset -= OnObjectivesReset;
            }
        }

        private void OnObjectivesReset()
        {
            // Progress was wiped, so a new run can earn this again.
            hasFired = false;
            wasComplete = false;
        }

        private bool IsObjectiveComplete()
        {
            if (questLog == null || objectiveIndex < 0 || objectiveIndex >= questLog.Objectives.Count)
            {
                return false;
            }

            QuestObjective objective = questLog.Objectives[objectiveIndex];

            // A target of zero counts as complete the moment it exists, which is never what
            // a collect-N objective means.
            return objective.TargetCount > 0 && objective.IsComplete;
        }

        private void OnObjectiveChanged(int index)
        {
            if (index != objectiveIndex)
            {
                return;
            }

            bool isComplete = IsObjectiveComplete();

            // Fire on the crossing, not on the state: something already complete before the
            // player touched it has nothing to celebrate.
            bool justCompleted = isComplete && !wasComplete;
            wasComplete = isComplete;

            if (hasFired || !justCompleted)
            {
                return;
            }

            hasFired = true;
            QuestObjective objective = questLog.Objectives[index];
            victorySequence.Show($"Objective Complete!\n{objective.Description}");
        }
    }
}
