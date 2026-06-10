using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    [Header("Hide when Settings/Credits is open")]
    public GameObject[] hideOnPanelOpen;

    void Start()
    {
        if (DifficultyManager.instance == null)
        {
            GameObject dm = new GameObject("DifficultyManager");
            dm.AddComponent<DifficultyManager>();
        }

        if (AudioSettings.instance == null)
        {
            GameObject ams = new GameObject("AudioSettings");
            ams.AddComponent<AudioSettings>();
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
        SetMainMenuVisible(false);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        SetMainMenuVisible(true);
    }

    public void OpenCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
        SetMainMenuVisible(false);
    }

    public void CloseCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
        SetMainMenuVisible(true);
    }

    void SetMainMenuVisible(bool visible)
    {
        if (hideOnPanelOpen == null) return;

        foreach (GameObject go in hideOnPanelOpen)
        {
            if (go != null) go.SetActive(visible);
        }
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