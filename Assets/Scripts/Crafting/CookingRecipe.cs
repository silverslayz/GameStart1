using GameStart.Skills;

namespace GameStart.Crafting
{
    public readonly struct CookingRecipe
    {
        public readonly string RecipeName;
        public readonly ResourceCost Ingredient;
        public readonly float HungerRestored;
        public readonly SkillRequirement Requirement;
        public readonly float CookXp;

        public CookingRecipe(string recipeName, ResourceCost ingredient, float hungerRestored, SkillRequirement requirement, float cookXp)
        {
            RecipeName = recipeName;
            Ingredient = ingredient;
            HungerRestored = hungerRestored;
            Requirement = requirement;
            CookXp = cookXp;
        }
    }
}
