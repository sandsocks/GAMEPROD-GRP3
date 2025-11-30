using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    IEnumerator LoadSceneRoutine(string sceneName)
    {
        // Fade in
        yield return StartCoroutine(Fade(1));

        // Load scene async
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // Wait until almost done
        while (op.progress < 0.9f)
            yield return null;

        // Finish loading
        op.allowSceneActivation = true;

        // Wait one frame after scene loads
        yield return null;

        // Fade out
        yield return StartCoroutine(Fade(0));
    }

    IEnumerator Fade(float target)
    {
        float start = canvasGroup.alpha;
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
    }
}
