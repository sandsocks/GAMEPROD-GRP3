using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class StartCutsceneEnd : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "MainMenu";

    void Start()
    {
        videoPlayer.loopPointReached += OnCutsceneEnd;
    }

    void Update()
    {
        if (Input.anyKeyDown)
            SceneManager.LoadScene(nextSceneName);
    }

    void OnCutsceneEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
