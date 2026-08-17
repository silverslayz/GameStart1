using System.Collections.Generic;
using GameStart.Skills;

namespace GameStart.Crafting
{
    public static class CookingRecipeCatalog
    {
        private static readonly List<CookingRecipe> Recipes = new List<CookingRecipe>
        {
            new CookingRecipe(
                "Cooked Meat",
                new ResourceCost("Raw Meat", 1),
                25f,
                new SkillRequirement(SkillType.Survival, 1),
                6f),
        };

        public static IReadOnlyList<CookingRecipe> All => Recipes;

        public static CookingRecipe? FindByName(string recipeName)
        {
            foreach (CookingRecipe recipe in Recipes)
            {
                if (recipe.RecipeName == recipeName)
                {
                    return recipe;
                }
            }

            return null;
        }
    }
}
