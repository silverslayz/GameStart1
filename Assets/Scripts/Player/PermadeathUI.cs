using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameStart.Player
{
    public class PermadeathUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text messageText;
        [SerializeField] private PlayerInput playerInput;

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
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

            if (playerInput != null)
            {
                playerInput.enabled = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
