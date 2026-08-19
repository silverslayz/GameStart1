using System.Collections;
using UnityEngine;
using GameStart.Interaction;
using GameStart.Skills;

namespace GameStart.Gathering
{
    public class ResourceNode : MonoBehaviour, IInteractable, IConditionalInteractable
    {
        [SerializeField] private string resourceName = "Iron Ore";
        [SerializeField] private int yieldPerGather = 1;
        [SerializeField] private float respawnTime = 30f;
        [SerializeField] private float gatherXpAmount = 5f;

        public bool IsDepleted { get; private set; }

        public string InteractionPrompt => IsDepleted ? $"{resourceName} (depleted)" : $"Gather {resourceName}";

        public bool CanInteract => !IsDepleted;

        public void Interact(GameObject interactor)
        {
            if (IsDepleted)
            {
                return;
            }

            var resources = interactor.GetComponent<PlayerResources>();
            var skills = interactor.GetComponent<PlayerSkills>();

            resources?.AddResource(resourceName, yieldPerGather);
            skills?.AddXp(SkillType.Gathering, gatherXpAmount);

            IsDepleted = true;
            StartCoroutine(RespawnAfterDelay());
        }

        private IEnumerator RespawnAfterDelay()
        {
            yield return new WaitForSeconds(respawnTime);
            IsDepleted = false;
        }
    }
}
