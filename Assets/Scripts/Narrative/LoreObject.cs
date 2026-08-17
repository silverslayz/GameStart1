using UnityEngine;
using GameStart.Interaction;

namespace GameStart.Narrative
{
    public class LoreObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string entryTitle;
        [SerializeField, TextArea(3, 10)] private string entryBody;
        [SerializeField] private LoreReaderUI reader;

        public string InteractionPrompt => $"Read: {entryTitle}";

        public void SetEntry(LoreEntry entry)
        {
            entryTitle = entry.Title;
            entryBody = entry.Body;
        }

        public void Interact(GameObject interactor)
        {
            reader?.Show(new LoreEntry(entryTitle, entryBody));
        }
    }
}
