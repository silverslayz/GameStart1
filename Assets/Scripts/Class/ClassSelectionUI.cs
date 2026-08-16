using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using GameStart.Player;

namespace GameStart.Class
{
    public class ClassSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject confirmationPanel;
        [SerializeField] private Text confirmationLabel;
        [SerializeField] private GameObject playerHud;
        [SerializeField] private float confirmationDuration = 1.5f;
        [SerializeField] private Toggle hardModeToggle;

        [SerializeField] private PlayerClassSelection classSelection;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Behaviour cameraLookController;

        private void Start()
        {
            if (playerHud != null)
            {
                playerHud.SetActive(false);
            }

            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }

            ShowPanel();
        }

        private void ShowPanel()
        {
            panel.SetActive(true);

            if (playerInput != null)
            {
                playerInput.enabled = false;
            }

            if (cameraLookController != null)
            {
                cameraLookController.enabled = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void RestartForNewRun()
        {
            if (playerHud != null)
            {
                playerHud.SetActive(false);
            }

            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }

            ShowPanel();
        }

        public void ChooseWarrior() => Choose(PlayerClassType.Warrior);
        public void ChooseRanger() => Choose(PlayerClassType.Ranger);
        public void ChooseMage() => Choose(PlayerClassType.Mage);

        private void Choose(PlayerClassType classType)
        {
            GameSessionSettings.HardModeEnabled = hardModeToggle != null && hardModeToggle.isOn;
            classSelection.SelectClass(classType);
            panel.SetActive(false);
            StartCoroutine(ShowConfirmationThenEnterGame(classType));
        }

        private IEnumerator ShowConfirmationThenEnterGame(PlayerClassType classType)
        {
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(true);
                if (confirmationLabel != null)
                {
                    confirmationLabel.text = $"Welcome, {classType}!";
                }

                yield return new WaitForSeconds(confirmationDuration);
                confirmationPanel.SetActive(false);
            }

            if (playerHud != null)
            {
                playerHud.SetActive(true);
            }

            if (playerInput != null)
            {
                playerInput.enabled = true;
            }

            if (cameraLookController != null)
            {
                cameraLookController.enabled = true;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
