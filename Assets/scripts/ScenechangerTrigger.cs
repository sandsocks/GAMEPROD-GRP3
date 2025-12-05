using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeTrigger : MonoBehaviour
{
    public string MenuScene = "Main Menu";
    public string CreditScene = "Credits";
    public void MainMenus()
    {
        SceneManager.LoadScene(MenuScene);
    }
    public void GoToCredits()
    {
        SceneManager.LoadScene(CreditScene);
    }
}
