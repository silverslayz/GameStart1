using System;
using UnityEngine;
using UnityEngine.InputSystem;
using GameStart.UI;

namespace GameStart.Interaction
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float interactRange = 3f;

        public event Action<IInteractable> NearbyInteractableChanged;

        public IInteractable CurrentInteractable { get; private set; }

        private void Update()
        {
            IInteractable found = FindNearbyInteractable();
            if (found != CurrentInteractable)
            {
                CurrentInteractable = found;
                NearbyInteractableChanged?.Invoke(CurrentInteractable);

                // Driven from the change itself rather than polled, so the label swaps the
                // moment the target does and disappears the moment there isn't one.
                InteractPromptText.SetTarget(CurrentInteractable);
            }
            else if (CurrentInteractable != null)
            {
                InteractPromptText.Refresh(CurrentInteractable);
            }
        }

        private void OnDisable()
        {
            // Death, a full-screen menu, a scene swap: whatever disabled us, the prompt
            // shouldn't be left hanging over a target the player can no longer reach.
            CurrentInteractable = null;
            InteractPromptText.Clear();
        }

        private IInteractable FindNearbyInteractable()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * (interactRange * 0.5f), interactRange * 0.5f);
            foreach (Collider hit in hits)
            {
                var interactable = hit.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    return interactable;
                }
            }

            return null;
        }

        // Called automatically by PlayerInput (Behavior: Send Messages)
        public void OnInteract(InputValue value)
        {
            if (!value.isPressed)
            {
                return;
            }

            CurrentInteractable?.Interact(gameObject);
        }
    }
}
