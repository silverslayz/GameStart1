using UnityEngine;
using GameStart.Interaction;
using GameStart.Party;

namespace GameStart.Town
{
    public class PartyBoard : MonoBehaviour, IInteractable
    {
        private const string LocalPlayerId = "You";

        public string InteractionPrompt => "Party Formation Board";

        public void Interact(GameObject interactor)
        {
            var party = interactor.GetComponent<PartyManager>();
            if (party == null)
            {
                return;
            }

            if (party.IsInParty(LocalPlayerId))
            {
                party.Leave(LocalPlayerId);
            }
            else
            {
                party.Join(LocalPlayerId);
            }
        }
    }
}
