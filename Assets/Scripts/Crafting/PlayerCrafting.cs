using System;
using UnityEngine;
using GameStart.Class;
using GameStart.Gathering;
using GameStart.Skills;

namespace GameStart.Crafting
{
    [RequireComponent(typeof(PlayerSkills))]
    [RequireComponent(typeof(PlayerResources))]
    [RequireComponent(typeof(PlayerInventory))]
    public class PlayerCrafting : MonoBehaviour
    {
        public event Action<CraftingRecipe> RecipeCrafted;

        private PlayerSkills skills;
        private PlayerResources resources;
        private PlayerInventory inventory;

        private void Awake()
        {
            skills = GetComponent<PlayerSkills>();
            resources = GetComponent<PlayerResources>();
            inventory = GetComponent<PlayerInventory>();
        }

        public bool CanCraft(CraftingRecipe recipe)
        {
            if (!skills.MeetsRequirement(recipe.Requirement))
            {
                return false;
            }

            foreach (ResourceCost cost in recipe.Ingredients)
            {
                if (resources.GetAmount(cost.ResourceName) < cost.Amount)
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryCraft(CraftingRecipe recipe)
        {
            if (!CanCraft(recipe))
            {
                return false;
            }

            foreach (ResourceCost cost in recipe.Ingredients)
            {
                resources.TryConsume(cost.ResourceName, cost.Amount);
            }

            inventory.AddItem(recipe.ResultItem);
            skills.AddXp(SkillType.Crafting, recipe.CraftXp);
            RecipeCrafted?.Invoke(recipe);
            return true;
        }
    }
}
