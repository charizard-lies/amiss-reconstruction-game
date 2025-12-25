using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;
using NUnit.Framework;

public class LevelUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelLabel;
    public GameObject buttonPrefab;
    public GameObject pauseMenu;
    public GameObject pauseBlocker;
    public GameObject winMenu;
    public GameObject confirmRestartMenu;

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

    [Header("Other")]
    public bool hasShownWin;

    private void Start()
    {
        // if (GameManager.Instance.selectedDailyLevel)
        // {
        //     int dayIndex = (DateTime.Now.Date - GameManager.Instance.startDate.Date).Days + 1;
        //     levelLabel.text = "Daily Level #" + dayIndex;
        // }
        // else levelLabel.text = "Level " + GameManager.Instance.selectedLevelId;
    }
    
    public void CreateCardButtons()
    {
        // for (int i = 0; i < cardContentArea.childCount; i++)
        // {
        //     Destroy(cardContentArea.GetChild(i).gameObject);
        // }

        // cardButtons.Clear();

        // foreach (var card in levelManager.deck.allCards)
        // {
        //     int index = card.removedId;

        //     GameObject cardWrapperObj = Instantiate(cardUIPrefab, cardContentArea);
        //     GameObject cardObj = cardWrapperObj.transform.GetChild(0).gameObject;
        //     cardButtons.Add(cardObj);

        //     CardButtonScript cardButtonScript = cardObj.GetComponent<CardButtonScript>();
        //     cardButtonScript.Initiate(levelManager, this, index);
        //     cardButtonScript.DrawCardAfterFrame();

        //     Button cardButton = cardObj.GetComponentInChildren<Button>();
        //     cardButton.onClick.AddListener(() => levelManager.deck.ToggleActiveCard(index));

        //     cardObj.GetComponent<RectTransform>().anchoredPosition = card.isVisible ? cardButtonScript.topPos : cardButtonScript.bottomPos;
        // }


    //     //shift this out!! VVV
    //     if(SaveManager.CurrentState.solved){
    //         ShowWinMenu();
    //     }
    //     else
    //     {
    //         hasShownWin = false;
    //     }
    //     //shift this out ^^^^^
    }

    // public void UpdateCardButtons()
    // {
    //     foreach (var cardButtonObj in cardButtons)
    //     {
    //         for (int i = 0; i < cardButtonObj.transform.childCount; i++)
    //         {
    //             Destroy(cardButtonObj.transform.GetChild(i).gameObject);
    //         }
    //         CardButtonScript cardButtonScript = cardButtonObj.GetComponent<CardButtonScript>();
    //         cardButtonScript.DrawCardAfterFrame();
    //     }
    // }

    // public void UpdateSolved(bool solved)
    // {
    //     if (!solved || hasShownWin) return;
        
    //     levelManager.levelState.solved = true;
    //     SaveManager.Save(GameManager.Instance.selectedLevelId);
    //     ShowWinMenu();
    // }

    // public void ShowWinMenu()
    // {
    //     winMenu.SetActive(true);
    //     pauseBlocker.SetActive(true);
    //     hasShownWin = true;
    //     levelManager.gamePaused = true;

    //     SaveManager.Save(levelManager.levelIndex);
    // }

    // public void AdmirePuzzle()
    // {
    //     winMenu.SetActive(false);
    //     pauseBlocker.SetActive(false);
    // }

    // public void TryRestart()
    // {
    //     if (!hasShownWin)
    //     {
    //         Restart();
    //         return;
    //     } 

    //     confirmRestartMenu.SetActive(true);
    // }

    // public void Restart()
    // {
    //     levelManager.Restart();
    //     confirmRestartMenu.SetActive(false);
    //     winMenu.SetActive(false);
    //     pauseBlocker.SetActive(false);
    //     levelManager.gamePaused = false;

    //     levelManager.levelState.solved = false;
    //     SaveManager.Save(GameManager.Instance.selectedLevelId);
    // }

    // public void CancelRestart()
    // {
    //     confirmRestartMenu.SetActive(false);
    // }

    // public void Pause()
    // {
    //     pauseMenu.SetActive(true);
    //     pauseBlocker.SetActive(true);
    //     levelManager.gamePaused = true;

    //     SaveManager.Save(levelManager.levelIndex);
    // }

    // public void Resume()
    // {
    //     pauseMenu.SetActive(false);
    //     pauseBlocker.SetActive(false);
    //     levelManager.gamePaused = false;
    // }

    // public void Quit()
    // {
    //     if (levelManager.daily) GameManager.Instance.LoadMainMenu();
    //     else GameManager.Instance.LoadLevelMenu();
    // }

    // public void DeleteSave()
    // {
    //     SaveManager.Delete(levelManager.levelIndex);
    // }

}
