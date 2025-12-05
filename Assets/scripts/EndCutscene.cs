using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class EndCutscene : MonoBehaviour
{
    public string mainMenuSceneName = "Main Menu";
    private VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += OnCutsceneFinished;
    }

    private void OnCutsceneFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
