using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// I control all active, visible, and invisible layers and their editabilty.
// I control buttons that toggle between these layers.

public class LevelUI : MonoBehaviour
{
    public TextMeshProUGUI solvedLabel;

    [Header("UI References")]
    public GameObject buttonPrefab;
    public GameObject pauseMenu;
    public GameObject pauseBlocker;
    public GameObject winMenu;

    [Header("CardUI")]
    public GameObject cardUIPrefab;
    public Sprite normalCardSprite;
    public Sprite activeCardSprite;
    public Color normalGraphColor;
    public Color activeGraphColor;
    public Transform cardContentArea;
    public ScrollRect scrollRect;

    [Header("Graph References")]
    public LevelScript levelManager;
    public List<GameObject> cardButtons = new List<GameObject>();
    public void CreateCardButtons()
    {
        for (int i = 0; i < cardContentArea.childCount; i++)
        {
            Destroy(cardContentArea.GetChild(i).gameObject);
        }

        cardButtons.Clear();

        foreach (var card in levelManager.deck.allCards)
        {
            int index = card.removedId;

            GameObject cardWrapperObj = Instantiate(cardUIPrefab, cardContentArea);
            GameObject cardObj = cardWrapperObj.transform.GetChild(0).gameObject;
            cardButtons.Add(cardObj);

            CardButtonScript cardButtonScript = cardObj.GetComponent<CardButtonScript>();
            cardButtonScript.Initiate(levelManager, this, index);
            cardButtonScript.DrawCardAfterFrame();

            Button cardButton = cardObj.GetComponentInChildren<Button>();
            cardButton.onClick.AddListener(() => levelManager.deck.ToggleActiveCard(index));

            cardObj.GetComponent<RectTransform>().anchoredPosition = card.isVisible ? cardButtonScript.topPos : cardButtonScript.bottomPos;
        }
    }

    public void UpdateCardButtons()
    {
        foreach (var cardButtonObj in cardButtons)
        {
            for (int i = 0; i < cardButtonObj.transform.childCount; i++)
            {
                Destroy(cardButtonObj.transform.GetChild(i).gameObject);
            }
            CardButtonScript cardButtonScript = cardButtonObj.GetComponent<CardButtonScript>();
            cardButtonScript.DrawCardAfterFrame();
        }
    }

    public void UpdateSolved(bool solved)
    {
        if (solved)
        {
            winMenu.SetActive(true);
            pauseBlocker.SetActive(true);
        }
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        pauseBlocker.SetActive(true);
        levelManager.gamePaused = true;

        SaveManager.Save(levelManager.levelIndex);
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        pauseBlocker.SetActive(false);
        levelManager.gamePaused = false;
    }

    public void Quit()
    {
        if (levelManager.daily) GameManager.Instance.LoadMainMenu();
        else GameManager.Instance.LoadLevelMenu();
    }

    public void OpenLevelMenu()
    {
        GameManager.Instance.LoadLevelMenu();
    }

    public void OpenMainMenu()
    {
        GameManager.Instance.LoadMainMenu();
    }

    public void DeleteSave()
    {
        SaveManager.Delete(levelManager.levelIndex);
    }
}
