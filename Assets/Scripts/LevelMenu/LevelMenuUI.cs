using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelMenuUI : MonoBehaviour
{
    [Header("References")]
    public Transform levelGridParent;
    public GameObject levelButtonPrefab; // assign in Inspector

    [Header("Settings")]
    public int totalLevels;

    public void Start()
    {
        if (GameManager.Instance == null)
        {
            SceneManager.LoadScene("PersistentManager", LoadSceneMode.Additive);
        }
        GenerateLevelButtons();
    }

    public void GenerateLevelButtons()
    {
        for (int i = 1; i <= totalLevels; i++)
        {
            GameObject buttonObj = Instantiate(levelButtonPrefab, levelGridParent.transform);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = $"{i}";

            string levelIndex = i.ToString();
            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => OnLevelSelected(levelIndex));
        }
    }

    public void OnLevelSelected(string index)
    {
        Debug.Log($"Selected Level {index}");
        GameManager.Instance.LoadLevelByIndex(index);
    }

    public void Back()
    {
        GameManager.Instance.LoadMainMenu();
    }
}
