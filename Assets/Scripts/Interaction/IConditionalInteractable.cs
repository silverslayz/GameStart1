namespace GameStart.Interaction
{
    /// <summary>
    /// Opt-in companion to <see cref="IInteractable"/> for things that stay in range and
    /// keep describing themselves while temporarily doing nothing - a depleted resource
    /// node, a house that needs no repair, a station with an unknown recipe. Their
    /// Interact() already returns early; this just lets the UI stop advertising a
    /// keypress that wouldn't do anything.
    ///
    /// Interactables that are always actionable simply don't implement it.
    /// </summary>
    public interface IConditionalInteractable
    {
        bool CanInteract { get; }
    }
}
