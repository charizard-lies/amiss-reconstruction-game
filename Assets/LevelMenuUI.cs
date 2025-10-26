using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelMenuUI : MonoBehaviour
{
    public GameObject levelButtonPrefab; // assign in Inspector
    public int totalLevels = 16;

    void Start()
    {
        if (GameManager.Instance == null)
        {
            SceneManager.LoadScene("PersistentManager", LoadSceneMode.Additive);
        }
        GenerateLevelButtons();
    }

    void GenerateLevelButtons()
    {
        for (int i = 1; i <= totalLevels; i++)
        {
            GameObject buttonObj = Instantiate(levelButtonPrefab, transform);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = $"Level {i}";

            int levelIndex = i; 
            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => OnLevelSelected(levelIndex));
        }
    }

    void OnLevelSelected(int index)
    {
        Debug.Log($"Selected Level {index}");
        GameManager.Instance.LoadLevelByIndex(index);
    }
}
