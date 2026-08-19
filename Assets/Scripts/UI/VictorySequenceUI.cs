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

        private bool isShowing;

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

            isShowing = true;
            ReleaseCursor();
        }

        /// <summary>
        /// Re-asserted every frame while the panel is up rather than set once in Show().
        /// This panel appears mid-gameplay, where anything else that grabs the cursor
        /// after us would otherwise leave a visible panel the player cannot click - and
        /// a modal that owns the screen should own the cursor for as long as it's up.
        /// </summary>
        private void LateUpdate()
        {
            if (isShowing)
            {
                ReleaseCursor();
            }
        }

        private void ReleaseCursor()
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
            }

            if (!Cursor.visible)
            {
                Cursor.visible = true;
            }
        }

        public void Continue()
        {
            if (!isShowing)
            {
                // The button is wired twice - a persistent onClick in the scene plus the
                // listener added above - so a single click calls this twice.
                return;
            }

            isShowing = false;
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
