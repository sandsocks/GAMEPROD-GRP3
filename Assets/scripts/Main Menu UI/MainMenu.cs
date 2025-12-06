using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
    
        PlayerPrefs.SetString("NextScene", "Chapter 1 Revamp");

    
        SceneManager.LoadScene("LoadingScreen");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
