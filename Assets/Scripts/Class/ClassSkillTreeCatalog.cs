using System.Collections.Generic;
using GameStart.Skills;

namespace GameStart.Class
{
    public static class ClassSkillTreeCatalog
    {
        private static readonly Dictionary<PlayerClassType, SkillType[]> BaseTrees = new Dictionary<PlayerClassType, SkillType[]>
        {
            { PlayerClassType.Warrior, new[] { SkillType.Combat, SkillType.Survival, SkillType.HousingTown } },
            { PlayerClassType.Ranger, new[] { SkillType.Combat, SkillType.Gathering, SkillType.Survival, SkillType.HousingTown } },
            { PlayerClassType.Mage, new[] { SkillType.Combat, SkillType.Crafting, SkillType.Survival, SkillType.HousingTown } },
        };

        public static IReadOnlyList<SkillType> GetBaseTree(PlayerClassType classType) => BaseTrees[classType];
    }
}
