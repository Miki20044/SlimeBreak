using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject settingsPanel;

    void Start()
    {
        if (DifficultyManager.instance == null)
        {
            GameObject dm = new GameObject("DifficultyManager");
            dm.AddComponent<DifficultyManager>();
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void SetNormal()
    {
        DifficultyManager.instance.current = DifficultyManager.Difficulty.Normal;
    }

    public void SetHard()
    {
        DifficultyManager.instance.current = DifficultyManager.Difficulty.Hard;
    }

    public void SetHell()
    {
        DifficultyManager.instance.current = DifficultyManager.Difficulty.Hell;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}