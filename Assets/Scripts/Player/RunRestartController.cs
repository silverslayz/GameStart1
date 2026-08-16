using UnityEngine;
using GameStart.Class;
using GameStart.Skills;
using GameStart.Dungeons;
using GameStart.Economy;

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

            permadeathUi?.Hide();
            classSelectionUi?.RestartForNewRun();
        }
    }
}
