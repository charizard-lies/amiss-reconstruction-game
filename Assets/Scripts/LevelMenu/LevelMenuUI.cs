using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelMenuUI : MonoBehaviour
{
    [Header("References")]
    public Transform levelGridParent;
    public GameObject levelButtonPrefab;
    public Sprite uncompletedBoxSprite;
    public Color uncompletedTextColor;
    public Sprite completedBoxSprite;
    public Color completedTextColor;

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
        // for (int i = 1; i <= totalLevels; i++)
        // {
        //     GameObject buttonObj = Instantiate(levelButtonPrefab, levelGridParent.transform);
            
        //     LevelState levelState = SaveManager.Load(i.ToString());
        //     if(levelState == null || !levelState.solved) 
        //     {
        //         buttonObj.GetComponent<Image>().sprite = uncompletedBoxSprite;
        //         buttonObj.GetComponentInChildren<TextMeshProUGUI>().color = uncompletedTextColor;
        //     }
        //     else 
        //     {
        //         buttonObj.GetComponent<Image>().sprite = completedBoxSprite;
        //         buttonObj.GetComponentInChildren<TextMeshProUGUI>().color = completedTextColor;
        //     }
            
        //     buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = $"{i}";

        //     string levelIndex = i.ToString();
        //     Button button = buttonObj.GetComponent<Button>();
        //     button.onClick.AddListener(() => OnLevelSelected(levelIndex));
        // }
    }

    public void OnLevelSelected(string index)
    {
        GameManager.Instance.LoadLevelByIndex(index);
    }

    public void Back()
    {
        GameManager.Instance.LoadMainMenu();
    }
}
