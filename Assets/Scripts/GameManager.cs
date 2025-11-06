using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool selectedDailyLevel;
    public int selectedLevelId;

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
            SceneManager.LoadScene("LevelMenu");
        }
    }

    public void LoadDailyLevel(int id)
    {
        selectedLevelId = id;
        selectedDailyLevel = true;
        SceneManager.LoadScene("LevelScene");
    }

    public void LoadLevelByIndex(int id)
    {
        selectedLevelId = id;
        selectedDailyLevel = false;
        SceneManager.LoadScene("LevelScene");
    }

    public void LoadLevelMenu()
    {
        SceneManager.LoadScene("LevelMenu");
    }
    
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
