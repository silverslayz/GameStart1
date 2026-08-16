using UnityEngine;
using GameStart.Interaction;
using GameStart.UI;

namespace GameStart.Town
{
    public class QuestBoard : MonoBehaviour, IInteractable
    {
        public string InteractionPrompt => "Read Quest Board";

        public void Interact(GameObject interactor)
        {
            var questLog = interactor.GetComponent<QuestLog>();
            if (questLog == null)
            {
                return;
            }

            foreach (QuestObjective quest in QuestBoardCatalog.AvailableQuests)
            {
                if (!questLog.HasQuest(quest.Description))
                {
                    questLog.AddQuest(new QuestObjective
                    {
                        Description = quest.Description,
                        TargetCount = quest.TargetCount,
                        CurrentCount = 0
                    });
                    return;
                }
            }
        }
    }
}
