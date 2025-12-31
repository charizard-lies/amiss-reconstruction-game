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
    public static LevelUI Instance;

    [Header("UI References")]
    public TextMeshProUGUI levelLabel;
    public GameObject buttonPrefab;
    public GameObject pauseMenu;
    public GameObject pauseBlocker;
    public GameObject winMenu;
    public GameObject confirmRestartMenu;
    public GameObject drawToolBg;
    public GameObject swapToolBg;
    public Color normalToolColor;
    public Color selectedToolColor;

    [Header("CardUI")]
    public GameObject cardUIPrefab;
    public Transform cardContentArea;
    public ScrollRect scrollRect;
    public RawImage stringImage;

    public float normalLineWidth;
    public float activeLineWidth;
    public Sprite normalCardSprite;
    public Sprite correctCardSprite;
    public Sprite activeCardSprite;
    public Color normalGraphColor;
    public Color activeGraphColor;

    [Header("Graph References")]
    public LevelScript levelManager;
    public List<CardButtonScript> cardButtonScripts = new List<CardButtonScript>();

    void Awake()
    {
        if(Instance == null) Instance = this;
        else Debug.LogWarning("another levelUI exists?");
    }

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

        cardButtonScripts.Clear();

        for (int i = 0; i < levelManager.graphData.nodes.Count(); i++)
        {
            int index = i;
            GameObject cardUIObj = Instantiate(cardUIPrefab, cardContentArea);

            CardButtonScript cardButtonScript = cardUIObj.GetComponent<CardButtonScript>();
            cardButtonScript.Initiate(index);
            cardButtonScript.DrawCardAfterFrame();
            cardButtonScripts.Add(cardButtonScript);

            Button cardButton = cardUIObj.GetComponentInChildren<Button>();
            cardButton.onClick.AddListener(() => levelManager.SetActiveCard(index));
        }
    }
    
    public void UpdateCards()
    {
        foreach (var cardButtonScript in cardButtonScripts)
        {
            cardButtonScript.DrawCardAfterFrame();
        }
    }

    public void SetCardCorrect(int cardId, bool correct)
    {
        CardButtonScript scriptToEdit = cardButtonScripts.Find(cardButtonScript => cardButtonScript.cardId == cardId);
        scriptToEdit.isIncluded = correct;
        scriptToEdit.gameObject.GetComponent<Image>().sprite = correct ? correctCardSprite : normalCardSprite;
    }
    
    public void SelectDrawTool()
    {
        drawToolBg.GetComponent<Image>().color = selectedToolColor;
        swapToolBg.GetComponent<Image>().color = normalToolColor;
    }

    public void SelectSwapTool()
    {
        drawToolBg.GetComponent<Image>().color = normalToolColor;
        swapToolBg.GetComponent<Image>().color = selectedToolColor;
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
