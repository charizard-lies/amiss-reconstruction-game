using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

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
    public float normalLineWidth;
    public float activeLineWidth;
    public Sprite normalCardSprite;
    public Sprite activeCardSprite;
    public Color normalGraphColor;
    public Color activeGraphColor;
    public Transform cardContentArea;
    public ScrollRect scrollRect;
    public RawImage stringImage;

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
        for (int i = 0; i < cardContentArea.childCount; i++)
        {
            Destroy(cardContentArea.GetChild(i).gameObject);
        }

        cardButtons.Clear();

        for (int i = 0; i < levelManager.graphData.nodes.Count(); i++)
        {
            GameObject cardUIObj = Instantiate(cardUIPrefab, cardContentArea);
            cardButtons.Add(cardUIObj);

            CardButtonScript cardButtonScript = cardUIObj.GetComponent<CardButtonScript>();
            cardButtonScript.Initiate(levelManager, this, i);
            cardButtonScript.DrawCardAfterFrame();

            // Button cardButton = cardUIObj.GetComponentInChildren<Button>();
            // cardButton.onClick.AddListener(() => levelManager.deck.ToggleActiveCard(index));
        }


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

    void Update()
    {
        float textureAspect = (float)stringImage.texture.width / stringImage.texture.height;
        float panelWidth   = stringImage.rectTransform.rect.width;
        float panelHeight  = stringImage.rectTransform.rect.height;
        float tileWidth = textureAspect * panelHeight;

        stringImage.uvRect = new Rect(0, 0, panelWidth / tileWidth, 1);

        float scrollX = scrollRect.content.anchoredPosition.x;
        Rect uv = stringImage.uvRect;
        uv.x = -scrollX / tileWidth;
        stringImage.uvRect = uv;
    }

}
