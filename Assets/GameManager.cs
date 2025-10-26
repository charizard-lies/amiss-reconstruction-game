using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
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
        if(SceneManager.GetActiveScene().name == "PersistentManager")
        {
            SceneManager.LoadScene("LevelMenu");
        }
    }

    public void LoadLevelByIndex(int id)
    {
        selectedLevelId = id;
        SceneManager.LoadScene("LevelScene");
    }
}
