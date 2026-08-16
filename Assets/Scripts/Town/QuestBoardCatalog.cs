using System.Collections.Generic;
using GameStart.UI;

namespace GameStart.Town
{
    public static class QuestBoardCatalog
    {
        public static readonly IReadOnlyList<QuestObjective> AvailableQuests = new List<QuestObjective>
        {
            new QuestObjective { Description = "Defeat an Apex Boss", TargetCount = 1, CurrentCount = 0 },
            new QuestObjective { Description = "Reach Combat level 10", TargetCount = 1, CurrentCount = 0 },
            new QuestObjective { Description = "Claim a house in Haven", TargetCount = 1, CurrentCount = 0 },
        };
    }
}
