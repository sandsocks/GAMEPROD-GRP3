using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI")]
    public Slider progressBar;
    public TextMeshProUGUI loadingText;

    [Header("Timing")]
    public float fakeLoadDuration = 2.5f; // Seconds minimum loading screen time

    void Start()
    {
        StartCoroutine(LoadAsyncOperation());
    }

    IEnumerator LoadAsyncOperation()
    {
        string sceneToLoad = PlayerPrefs.GetString("NextScene");
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        float timer = 0f;

        while (!operation.isDone)
        {
            timer += Time.deltaTime;

            // Smooth fake loading curve
            float fakeProgress = Mathf.Clamp01(timer / fakeLoadDuration);

            // Real loading progress
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Combine real + fake progress
            float displayProgress = Mathf.Min(fakeProgress, realProgress);

            progressBar.value = displayProgress;
            loadingText.text = "Loading... " + (displayProgress * 100f).ToString("F0") + "%";

            // Both real load & fake timer must be done
            if (displayProgress >= 1f)
            {
                loadingText.text = "Starting...";
                yield return new WaitForSeconds(0.5f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
