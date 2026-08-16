using UnityEngine;
using GameStart.Interaction;
using GameStart.Economy;
using GameStart.Class;

namespace GameStart.Town
{
    public class ShopNPC : MonoBehaviour, IInteractable
    {
        [SerializeField] private string itemName = "Traveler's Ration";
        [SerializeField] private float itemWeight = 1f;
        [SerializeField] private int priceInGems = 5;

        public string InteractionPrompt => $"Buy {itemName} ({priceInGems} gems)";

        public void Interact(GameObject interactor)
        {
            var currency = interactor.GetComponent<PlayerCurrency>();
            var inventory = interactor.GetComponent<PlayerInventory>();
            if (currency == null || inventory == null)
            {
                return;
            }

            if (!currency.TrySpendGems(priceInGems))
            {
                return;
            }

            inventory.AddItem(new GearItem(itemName, itemWeight));
        }
    }
}
