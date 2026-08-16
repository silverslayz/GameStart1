using UnityEngine;

namespace GameStart.Interaction
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }

        void Interact(GameObject interactor);
    }
}
