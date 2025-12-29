using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public GameObject dailyPuzzleButton;
    public GameObject levelMenuButton;
    public GameObject settingMenuButton;

    void Start()
    {
        if (GameManager.Instance == null)
        {
            SceneManager.LoadScene("PersistentManager", LoadSceneMode.Additive);
        }
    }

    public void OpenDailyPuzzle()
    {
        GameManager.Instance.LoadDailyLevel();
    }

    public void OpenLevelMenu()
    {
        GameManager.Instance.LoadLevelMenu();
    }

    public void OpenSettings()
    {
        return;
    }
}
