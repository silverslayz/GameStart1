using UnityEngine;
using GameStart.Interaction;

namespace GameStart.Crafting
{
    public class CraftingStation : MonoBehaviour, IInteractable, IConditionalInteractable
    {
        [SerializeField] private string recipeName = "Iron Dagger";

        public string InteractionPrompt
        {
            get
            {
                CraftingRecipe? recipe = CraftingRecipeCatalog.FindByName(recipeName);
                if (recipe == null)
                {
                    return "Craft (unknown recipe)";
                }

                ResourceCost cost = recipe.Value.Ingredients[0];
                return $"Craft {recipe.Value.RecipeName} ({cost.Amount} {cost.ResourceName})";
            }
        }

        public bool CanInteract => CraftingRecipeCatalog.FindByName(recipeName) != null;

        public void Interact(GameObject interactor)
        {
            CraftingRecipe? recipe = CraftingRecipeCatalog.FindByName(recipeName);
            if (recipe == null)
            {
                return;
            }

            var crafting = interactor.GetComponent<PlayerCrafting>();
            crafting?.TryCraft(recipe.Value);
        }
    }
}
