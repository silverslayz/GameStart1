using UnityEngine;
using UnityEngine.InputSystem;

namespace GameStart.Class
{
    public class ClassSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private PlayerClassSelection classSelection;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Behaviour cameraLookController;

        private void Start()
        {
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

        public void ChooseWarrior() => Choose(PlayerClassType.Warrior);
        public void ChooseRanger() => Choose(PlayerClassType.Ranger);
        public void ChooseMage() => Choose(PlayerClassType.Mage);

        private void Choose(PlayerClassType classType)
        {
            classSelection.SelectClass(classType);
            panel.SetActive(false);

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
