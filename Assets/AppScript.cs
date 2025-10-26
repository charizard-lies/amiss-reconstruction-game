using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int selectedLevelId;
    public GraphData currentLevelData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // public void LoadLevel(GraphData data)
    // {
    //     currentLevelData = data;
    //     selectedLevelId = data.levelNumber;
    //     UnityEngine.SceneManagement.SceneManager.LoadScene("GameLevel");
    // }
}
