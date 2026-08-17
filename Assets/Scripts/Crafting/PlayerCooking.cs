using System;
using UnityEngine;
using GameStart.Gathering;
using GameStart.Player;
using GameStart.Skills;

namespace GameStart.Crafting
{
    [RequireComponent(typeof(PlayerSkills))]
    [RequireComponent(typeof(PlayerResources))]
    [RequireComponent(typeof(PlayerNeeds))]
    public class PlayerCooking : MonoBehaviour
    {
        public event Action<CookingRecipe> RecipeCooked;

        private PlayerSkills skills;
        private PlayerResources resources;
        private PlayerNeeds needs;

        private void Awake()
        {
            skills = GetComponent<PlayerSkills>();
            resources = GetComponent<PlayerResources>();
            needs = GetComponent<PlayerNeeds>();
        }

        public bool CanCook(CookingRecipe recipe)
        {
            return skills.MeetsRequirement(recipe.Requirement)
                && resources.GetAmount(recipe.Ingredient.ResourceName) >= recipe.Ingredient.Amount;
        }

        public bool TryCook(CookingRecipe recipe)
        {
            if (!CanCook(recipe))
            {
                return false;
            }

            resources.TryConsume(recipe.Ingredient.ResourceName, recipe.Ingredient.Amount);
            needs.Eat(recipe.HungerRestored);
            skills.AddXp(SkillType.Survival, recipe.CookXp);
            RecipeCooked?.Invoke(recipe);
            return true;
        }
    }
}
