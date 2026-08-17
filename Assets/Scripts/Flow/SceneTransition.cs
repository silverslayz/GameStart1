using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameStart.Flow
{
    // Static entry point so any UI button can trigger a faded scene load
    // without needing a scene-placed reference wired to it.
    public static class SceneTransition
    {
        private const float FadeDuration = 0.35f;
        private static SceneTransitionRunner runner;

        public static void LoadScene(string sceneName)
        {
            EnsureRunner();
            runner.StartCoroutine(FadeAndLoad(sceneName));
        }

        private static void EnsureRunner()
        {
            if (runner != null)
            {
                return;
            }

            var go = new GameObject("SceneTransitionRunner");
            Object.DontDestroyOnLoad(go);
            runner = go.AddComponent<SceneTransitionRunner>();
            runner.BuildFadeCanvas();
        }

        private static IEnumerator FadeAndLoad(string sceneName)
        {
            yield return runner.Fade(0f, 1f, FadeDuration);
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return runner.Fade(1f, 0f, FadeDuration);
        }
    }

    internal class SceneTransitionRunner : MonoBehaviour
    {
        private CanvasGroup fadeGroup;

        public void BuildFadeCanvas()
        {
            var canvasGo = new GameObject("FadeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            fadeGroup = canvasGo.GetComponent<CanvasGroup>();
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;

            var imgGo = new GameObject("Black", typeof(RectTransform), typeof(Image));
            imgGo.transform.SetParent(canvasGo.transform, false);
            var rt = imgGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            imgGo.GetComponent<Image>().color = Color.black;
        }

        public IEnumerator Fade(float from, float to, float duration)
        {
            if (fadeGroup == null)
            {
                yield break;
            }

            fadeGroup.blocksRaycasts = true;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                fadeGroup.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }

            fadeGroup.alpha = to;
            fadeGroup.blocksRaycasts = to > 0.5f;
        }
    }
}
