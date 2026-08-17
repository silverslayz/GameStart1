using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using GameStart.Audio;

namespace GameStart.Narrative
{
    public class LoreReaderUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Behaviour cameraLookController;

        public void Show(LoreEntry entry)
        {
            if (panel == null)
            {
                return;
            }

            if (titleText != null) titleText.text = entry.Title;
            if (bodyText != null) bodyText.text = entry.Body;

            panel.SetActive(true);

            if (playerInput != null) playerInput.enabled = false;
            if (cameraLookController != null) cameraLookController.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Close()
        {
            SfxPlayer.Play(SfxLibrary.UIClick);

            if (panel != null)
            {
                panel.SetActive(false);
            }

            if (playerInput != null) playerInput.enabled = true;
            if (cameraLookController != null) cameraLookController.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
