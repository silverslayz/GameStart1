using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using GameStart.Audio;

namespace GameStart.UI
{
    public class VictorySequenceUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text messageText;
        [SerializeField] private Button continueButton;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Behaviour cameraLookController;

        private void Awake()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(Continue);
            }
        }

        public void Show(string message)
        {
            if (panel == null)
            {
                return;
            }

            if (messageText != null)
            {
                messageText.text = message;
            }

            panel.SetActive(true);
            SfxPlayer.Play(SfxLibrary.Victory);

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

        public void Continue()
        {
            SfxPlayer.Play(SfxLibrary.UIClick);

            if (panel != null)
            {
                panel.SetActive(false);
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
