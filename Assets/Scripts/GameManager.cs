using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool selectedDailyLevel;
    public string selectedLevelId;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "PersistentManager")
        {
            LoadMainMenu();
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

        public void LoadDailyLevel()
    {
        selectedLevelId = System.DateTime.Now.ToString("ddMMyyyy");
        selectedDailyLevel = true;
        SceneManager.LoadScene("LevelScene");
    }

    public void LoadLevelMenu()
    {
        SceneManager.LoadScene("LevelMenu");
    }

    public void LoadLevelByIndex(string id)
    {
        selectedLevelId = id;
        selectedDailyLevel = false;
        SceneManager.LoadScene("LevelScene");
    }
}
