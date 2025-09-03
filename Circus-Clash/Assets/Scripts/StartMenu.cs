using UnityEngine;
using UnityEngine.SceneManagement;
public class StartMenu : MonoBehaviour
{
    public void OnStart()
    {
        SceneManager.LoadScene(1);
    }

    public void OnTutorial()
    {
        SceneManager.LoadScene(2);
    }

    public void OnQuit()
    {
        Application.Quit();
    }

}
