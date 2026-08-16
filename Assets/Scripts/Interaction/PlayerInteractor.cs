using System;
using UnityEngine;
using UnityEngine.InputSystem;

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
            }
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
