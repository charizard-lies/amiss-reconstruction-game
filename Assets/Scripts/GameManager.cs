using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public DateTime startDate = new DateTime(2026, 1, 6);
    public bool selectedDailyLevel;
    public bool selectedTutorialLevel;
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
        selectedLevelId = DateTime.Now.ToString("ddMMyyyy");
        selectedDailyLevel = true;
        selectedTutorialLevel = false;
        SceneManager.LoadScene("LevelScene");
    }

    public void LoadTutorialLevel()
    {
        selectedLevelId = "Tutorial";
        selectedDailyLevel = false;
        selectedTutorialLevel = true;
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
        selectedTutorialLevel = false;
        SceneManager.LoadScene("LevelScene");
    }
}
