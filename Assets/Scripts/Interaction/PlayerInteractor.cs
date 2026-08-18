using System;
using UnityEngine;
using UnityEngine.InputSystem;
using GameStart.UI;

namespace GameStart.Interaction
{
    public class PlayerInteractor : MonoBehaviour
    {
        private const string InteractActionName = "Interact";

        [SerializeField] private float interactRange = 3f;

        public event Action<IInteractable> NearbyInteractableChanged;

        public IInteractable CurrentInteractable { get; private set; }

        private PlayerInput playerInput;
        private string cachedScheme;
        private string cachedKeyHint = string.Empty;

        private void Awake()
        {
            // Same GameObject as the PlayerInput driving OnInteract via Send Messages.
            playerInput = GetComponent<PlayerInput>();
        }

        private void Update()
        {
            IInteractable found = FindNearbyInteractable();
            if (found != CurrentInteractable)
            {
                CurrentInteractable = found;
                NearbyInteractableChanged?.Invoke(CurrentInteractable);

                // Driven from the change itself rather than polled, so the label swaps the
                // moment the target does and disappears the moment there isn't one.
                InteractPromptText.SetTarget(CurrentInteractable, InteractKeyHint());
            }
            else if (CurrentInteractable != null)
            {
                InteractPromptText.Refresh(CurrentInteractable, InteractKeyHint());
            }
        }

        /// <summary>
        /// How the interact button is currently labelled - "E" on keyboard, the face
        /// button on a pad. Read from the live bindings rather than hardcoded, so it
        /// follows the device the player is actually holding, and will keep up with
        /// rebinding when that lands.
        /// </summary>
        private string InteractKeyHint()
        {
            if (playerInput == null || playerInput.actions == null)
            {
                return string.Empty;
            }

            string scheme = playerInput.currentControlScheme;
            if (scheme == cachedScheme)
            {
                // Recomputed only when the player switches device: this runs every frame
                // an interactable is in range, and the lookup allocates a string.
                return cachedKeyHint;
            }

            cachedScheme = scheme;
            cachedKeyHint = string.Empty;

            InputAction action = playerInput.actions.FindAction(InteractActionName);
            if (action != null)
            {
                cachedKeyHint = string.IsNullOrEmpty(scheme)
                    ? action.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions)
                    : action.GetBindingDisplayString(InputBinding.MaskByGroup(scheme),
                        InputBinding.DisplayStringOptions.DontIncludeInteractions);
            }

            return cachedKeyHint;
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
