using UnityEngine;
using GameStart.Interaction;
using GameStart.Flow;

namespace GameStart.Narrative
{
    public class TutorialDialogueNpc : MonoBehaviour, IInteractable
    {
        [SerializeField] private LoreReaderUI reader;
        [SerializeField] private string npcName = "Haven Elder";

        private int lineIndex;

        private void Awake()
        {
            // Lives on a scene canvas, which a prefab cannot reference; a prefab instance
            // would otherwise start with this null and the NPC would say nothing.
            reader = SceneLink.Resolve(reader);
        }

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
