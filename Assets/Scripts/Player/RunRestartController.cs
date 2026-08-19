using UnityEngine;
using GameStart.Class;
using GameStart.Skills;
using GameStart.Dungeons;
using GameStart.Economy;
using GameStart.Flow;

namespace GameStart.Player
{
    public class RunRestartController : MonoBehaviour
    {
        [SerializeField] private Vector3 respawnPoint = Vector3.zero;
        [SerializeField] private PermadeathUI permadeathUi;
        [SerializeField] private ClassSelectionUI classSelectionUi;

        private CharacterController controller;
        private PlayerHealth health;
        private PlayerNeeds needs;
        private PlayerInventory inventory;
        private PlayerSkills skills;
        private PlayerSkillTree skillTree;
        private PlayerClassSelection classSelection;
        private PlayerDungeonProgress dungeonProgress;
        private PlayerCurrency currency;
        private UI.QuestLog questLog;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            health = GetComponent<PlayerHealth>();
            needs = GetComponent<PlayerNeeds>();
            inventory = GetComponent<PlayerInventory>();
            skills = GetComponent<PlayerSkills>();
            skillTree = GetComponent<PlayerSkillTree>();
            classSelection = GetComponent<PlayerClassSelection>();
            dungeonProgress = GetComponent<PlayerDungeonProgress>();
            currency = GetComponent<PlayerCurrency>();
            questLog = GetComponent<UI.QuestLog>();

            // Both live on scene canvases, so a prefab instance starts with these null.
            permadeathUi = SceneLink.Resolve(permadeathUi);
            classSelectionUi = SceneLink.Resolve(classSelectionUi);
        }

        public void RestartRun()
        {
            if (controller != null)
            {
                controller.enabled = false;
                transform.position = respawnPoint;
                controller.enabled = true;
            }
            else
            {
                transform.position = respawnPoint;
            }

            health.Revive();
            needs?.RestoreFull();
            inventory?.Clear();
            skills?.ResetAllSkills();
            skillTree?.ResetTree();
            classSelection?.ResetSelection();
            dungeonProgress?.ResetProgress();
            currency?.ResetGems();
            // Matches StartNewGame: a restarted run is a new character, so it starts the
            // tutorial quest from zero rather than inheriting the dead run's progress.
            questLog?.ResetObjectives();

            permadeathUi?.Hide();
            classSelectionUi?.RestartForNewRun();
        }
    }
}
