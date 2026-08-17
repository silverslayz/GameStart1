using UnityEngine;
using GameStart.Interaction;

namespace GameStart.Narrative
{
    public class TutorialDialogueNpc : MonoBehaviour, IInteractable
    {
        [SerializeField] private LoreReaderUI reader;
        [SerializeField] private string npcName = "Haven Elder";

        private int lineIndex;

        public string InteractionPrompt => "Talk";

        public void Interact(GameObject interactor)
        {
            var lines = LoreLibrary.TutorialGiverDialogue;
            if (lines.Count == 0)
            {
                return;
            }

            string line = lines[lineIndex % lines.Count];
            lineIndex++;

            reader?.Show(new LoreEntry(npcName, line));
        }
    }
}
