using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject endScreen;
    public GameObject sleepScreen;
    public GameObject winScreen;

    void Awake()
    {
        instance = this;
    }

    public void GameOver()
    {
        Time.timeScale = 0f;

        if (endScreen != null)
            endScreen.SetActive(true);
    }

    public void SleepGameOver()
    {
        Time.timeScale = 0f;

        if (sleepScreen != null)
            sleepScreen.SetActive(true);
    }

    public void WinGame()
    {
        Time.timeScale = 0f;

        if (winScreen != null)
            winScreen.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}