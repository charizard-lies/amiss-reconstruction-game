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
    public Transform buttonParent;
    public GameObject pauseMenu;
    public GameObject pauseBlocker;

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
    public DeckScript deckManager;

    private int currentLayerIndex = -1;
    public List<GameObject> cardButtons = new List<GameObject>();

    public void InitButtons(GraphData graphData)
    {
        AddSolvedLabel();

        GameObject submitButtonObj = Instantiate(buttonPrefab, buttonParent);

        TextMeshProUGUI submitLabel = submitButtonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (submitLabel != null) submitLabel.text = "Submit";
        Button submitButton = submitButtonObj.GetComponentInChildren<Button>();

        submitButton.onClick.AddListener(() => UpdateSolved(levelManager.CheckGraphSolved()));

        CreateCardButtons();
        Resume();
    }

    public void AddSolvedLabel()
    {
        GameObject labelObj = new GameObject("SolvedLabel");
        labelObj.transform.SetParent(buttonParent, false);

        solvedLabel = labelObj.AddComponent<TextMeshProUGUI>();
        solvedLabel.text = "Unsolved";
        solvedLabel.fontSize = 24;
        solvedLabel.alignment = TextAlignmentOptions.Center;
    }

    public void CreateCardButtons()
    {
        // Remove old buttons (skip the first two: + and -)
        for (int i = 0; i < cardContentArea.childCount; i++)
        {
            Destroy(cardContentArea.GetChild(i).gameObject);
        }

        cardButtons.Clear();

        foreach (var card in deckManager.allCards)
        {
            int index = card.removedId;

            GameObject cardWrapperObj = Instantiate(cardUIPrefab, cardContentArea);
            GameObject cardObj = cardWrapperObj.transform.GetChild(0).gameObject;
            cardButtons.Add(cardObj);

            CardButtonScript cardButtonScript = cardObj.GetComponent<CardButtonScript>();
            cardButtonScript.Initiate(levelManager, this, index);
            bool cardIsActive = deckManager.activeCard.removedId == index;
            cardButtonScript.DrawCardSafe(cardIsActive);

            Button cardButton = cardObj.GetComponentInChildren<Button>();
            cardButton.onClick.AddListener(() => deckManager.ToggleActiveCard(index));

            Debug.Log($"card {cardButtonScript.cardId}: visible is {card.isVisible}");
            cardObj.GetComponent<RectTransform>().anchoredPosition = card.isVisible ? cardButtonScript.topPos : cardButtonScript.bottomPos;
            // dragScript.SnapCard(dragScript.topPos);
        }
    }

    public void UpdateCardButtons()
    {
        foreach (var cardButtonObj in cardButtons)
        {
            for (int i=0; i < cardButtonObj.transform.childCount; i++)
            {
                Destroy(cardButtonObj.transform.GetChild(i).gameObject);
            }
            CardButtonScript cardButtonScript = cardButtonObj.GetComponent<CardButtonScript>();
            int index = cardButtonScript.cardId;

            bool cardIsActive = deckManager.activeCard.removedId == index;
            cardButtonScript.DrawCardSafe(cardIsActive);

            Button cardButton = cardButtonObj.GetComponentInChildren<Button>();
            cardButton.onClick.AddListener(() => deckManager.ToggleActiveCard(index));
            // dragScript.SnapCard(dragScript.topPos);
        }
    }

    public void UpdateSolved(bool solved)
    {
        if (solved) solvedLabel.text = "Solved";
        else solvedLabel.text = "Unsolved";
    }

    private void RequestAddVisibleCard()
    {
        deckManager.AddVisibleCard();
        UpdateCardButtons();
    }

    private void RequestMinusVisibleCard()
    {
        deckManager.MinusVisibleCard();
        UpdateCardButtons();
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        pauseBlocker.SetActive(true);
        levelManager.gamePaused = true;
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
}
