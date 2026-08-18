using UnityEngine;
using GameStart.Interaction;

namespace GameStart.UI
{
    /// <summary>
    /// The floating label over whatever the player can currently interact with.
    /// Shares <see cref="FloatingWorldText"/>'s canvas and projection with damage numbers.
    /// </summary>
    public static class InteractPromptText
    {
        /// <summary>The cyan the rest of the game's UI already uses for highlights.</summary>
        public static readonly Color PromptColor = new Color(0.35f, 0.80f, 1f, 1f);

        /// <summary>
        /// Points the prompt at <paramref name="interactable"/>, or clears it when that's
        /// null. Every interactable in the game is a Component, which is what gives the
        /// label a transform to follow - the interface itself has no position.
        /// </summary>
        public static void SetTarget(IInteractable interactable)
        {
            if (interactable is Component component && component != null)
            {
                FloatingWorldText.Runner.SetPrompt(component.transform, interactable.InteractionPrompt, PromptColor);
                return;
            }

            Clear();
        }

        /// <summary>
        /// Keeps the wording current for a target that hasn't changed - a node that just
        /// went depleted still reads "Gather Iron Ore" otherwise.
        /// </summary>
        public static void Refresh(IInteractable interactable)
        {
            if (interactable is Component component && component != null)
            {
                FloatingWorldText.Runner.UpdatePromptContent(interactable.InteractionPrompt, PromptColor);
            }
        }

        public static void Clear()
        {
            FloatingWorldText.Runner.ClearPrompt();
        }
    }
}
