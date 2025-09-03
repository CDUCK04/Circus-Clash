using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    private bool paused = false;

    public void OnPause()
    {
        paused = !paused;
        pauseMenu.SetActive(paused);
        Time.timeScale = paused ? 0 : 1;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

}
