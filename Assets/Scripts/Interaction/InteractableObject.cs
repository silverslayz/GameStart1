using UnityEngine;
using UnityEngine.Events;

namespace GameStart.Interaction
{
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionPrompt = "Interact";
        [SerializeField] private UnityEvent<GameObject> onInteract;

        public string InteractionPrompt => interactionPrompt;
        public UnityEvent<GameObject> OnInteractEvent => onInteract;

        public void Interact(GameObject interactor)
        {
            onInteract?.Invoke(interactor);
        }
    }
}
