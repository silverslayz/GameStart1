using UnityEngine;
using UnityEngine.UI;
using GameStart.Persistence;
using GameStart.Flow;

namespace GameStart.UI
{
    public class TitleScreenController : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject settingsPanel;

        private void Start()
        {
            if (continueButton != null)
            {
                continueButton.interactable = SaveSystem.IsSaveValid();
            }
        }

        public void OnNewGame()
        {
            GameFlow.PendingNewGame = true;
            SceneTransition.LoadScene(GameFlow.GameplaySceneName);
        }

        public void OnContinue()
        {
            GameFlow.PendingNewGame = false;
            SceneTransition.LoadScene(GameFlow.GameplaySceneName);
        }

        public void OnOpenSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        public void OnCloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        public void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
