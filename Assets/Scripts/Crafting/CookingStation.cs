using UnityEngine;
using GameStart.Interaction;

namespace GameStart.Crafting
{
    public class CookingStation : MonoBehaviour, IInteractable
    {
        [SerializeField] private string recipeName = "Cooked Meat";

        public string InteractionPrompt
        {
            get
            {
                CookingRecipe? recipe = CookingRecipeCatalog.FindByName(recipeName);
                if (recipe == null)
                {
                    return "Cook (unknown recipe)";
                }

                ResourceCost ingredient = recipe.Value.Ingredient;
                return $"Cook {recipe.Value.RecipeName} ({ingredient.Amount} {ingredient.ResourceName})";
            }
        }

        public void Interact(GameObject interactor)
        {
            CookingRecipe? recipe = CookingRecipeCatalog.FindByName(recipeName);
            if (recipe == null)
            {
                return;
            }

            var cooking = interactor.GetComponent<PlayerCooking>();
            cooking?.TryCook(recipe.Value);
        }
    }
}
