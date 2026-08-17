using System.Collections.Generic;
using GameStart.Class;
using GameStart.Skills;

namespace GameStart.Crafting
{
    public static class CraftingRecipeCatalog
    {
        private static readonly List<CraftingRecipe> Recipes = new List<CraftingRecipe>
        {
            new CraftingRecipe(
                "Iron Dagger",
                new GearItem("Iron Dagger", 2f),
                new[] { new ResourceCost("Iron Ore", 3) },
                new SkillRequirement(SkillType.Crafting, 1),
                10f),
            new CraftingRecipe(
                "Iron Buckler",
                new GearItem("Iron Buckler", 4f),
                new[] { new ResourceCost("Iron Ore", 5) },
                new SkillRequirement(SkillType.Crafting, 3),
                15f),
        };

        public static IReadOnlyList<CraftingRecipe> All => Recipes;

        public static CraftingRecipe? FindByName(string recipeName)
        {
            foreach (CraftingRecipe recipe in Recipes)
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
