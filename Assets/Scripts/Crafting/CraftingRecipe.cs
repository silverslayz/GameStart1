using GameStart.Class;
using GameStart.Skills;

namespace GameStart.Crafting
{
    public readonly struct CraftingRecipe
    {
        public readonly string RecipeName;
        public readonly GearItem ResultItem;
        public readonly ResourceCost[] Ingredients;
        public readonly SkillRequirement Requirement;
        public readonly float CraftXp;

        public CraftingRecipe(string recipeName, GearItem resultItem, ResourceCost[] ingredients, SkillRequirement requirement, float craftXp)
        {
            RecipeName = recipeName;
            ResultItem = resultItem;
            Ingredients = ingredients;
            Requirement = requirement;
            CraftXp = craftXp;
        }
    }
}
