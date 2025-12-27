using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using Unity.VisualScripting;

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
    public Transform cardContentArea;
    public ScrollRect scrollRect;
    public RawImage stringImage;

    public float normalLineWidth;
    public float activeLineWidth;
    public Sprite normalCardSprite;
    public Sprite activeCardSprite;
    public Color normalGraphColor;
    public Color activeGraphColor;

    [Header("Graph References")]
    public LevelScript levelManager;
    public List<GameObject> cardButtons = new List<GameObject>();

    private void Start()
    {
        if (GameManager.Instance.selectedDailyLevel)
        {
            int dayIndex = (DateTime.Now.Date - GameManager.Instance.startDate.Date).Days + 1;
            levelLabel.text = "Daily Level #" + dayIndex;
        }
        else levelLabel.text = "Level " + GameManager.Instance.selectedLevelId;
    }
    
    public void DrawCardButtons()
    {
        for (int i = 0; i < cardContentArea.childCount; i++)
        {
            Destroy(cardContentArea.GetChild(i).gameObject);
        }

        cardButtons.Clear();

        for (int i = 0; i < levelManager.graphData.nodes.Count(); i++)
        {
            int index = i;
            GameObject cardUIObj = Instantiate(cardUIPrefab, cardContentArea);
            cardButtons.Add(cardUIObj);

            CardButtonScript cardButtonScript = cardUIObj.GetComponent<CardButtonScript>();
            cardButtonScript.Initiate(levelManager, this, index);
            cardButtonScript.DrawCardAfterFrame();

            Button cardButton = cardUIObj.GetComponentInChildren<Button>();
            cardButton.onClick.AddListener(() => levelManager.SetActiveCard(index));
        }
    }

    
    public void UpdateCards()
    {
        foreach (var cardUIObj in cardButtons)
        {
            CardButtonScript cardButtonScript = cardUIObj.GetComponent<CardButtonScript>();
            cardButtonScript.DrawCardAfterFrame();
        }
    }

    public void ShowWinMenu()
    {
        winMenu.SetActive(true);
        pauseBlocker.SetActive(true);
    }

    public void AdmirePuzzle()
    {
        winMenu.SetActive(false);
        pauseBlocker.SetActive(false);
    }

    public void Pause()
    {
        pauseBlocker.SetActive(true);

        if(levelManager.gameAdmiring) winMenu.SetActive(true);
        else pauseMenu.SetActive(true);
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        pauseBlocker.SetActive(false);
    }

    public void TryRestart()
    {
        confirmRestartMenu.SetActive(true);
        winMenu.SetActive(false);
        pauseBlocker.SetActive(true);
    }

    public void CancelRestart()
    {
        confirmRestartMenu.SetActive(false);
        
        if(!levelManager.gameAdmiring && levelManager.gameWon) winMenu.SetActive(true);
        else pauseBlocker.SetActive(false);
    }

    public void Restart()
    {
        confirmRestartMenu.SetActive(false);
        pauseBlocker.SetActive(false);

        // levelManager.levelState.solved = false;
        // SaveManager.Save(GameManager.Instance.selectedLevelId);
    }

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
