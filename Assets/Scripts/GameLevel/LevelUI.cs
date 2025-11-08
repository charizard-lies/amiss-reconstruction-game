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

    [Header("CardUI")]
    public GameObject cardUIPrefab;
    public Transform cardContentArea;
    public ScrollRect scrollRect;

    [Header("Graph References")]
    public DeckScript deckManager;
    public LevelScript levelManager;

    private int currentLayerIndex = -1;
    private List<GameObject> cardButtons = new List<GameObject>();

    public void InitButtons(GraphData graphData)
    {
        AddSolvedLabel();

        GameObject submitButtonObj = Instantiate(buttonPrefab, buttonParent);
        GameObject plusButtonObj = Instantiate(buttonPrefab, buttonParent);
        GameObject minusButtonObj = Instantiate(buttonPrefab, buttonParent);

        TextMeshProUGUI submitLabel = submitButtonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (submitLabel != null) submitLabel.text = "Submit";
        TextMeshProUGUI plusLabel = plusButtonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (plusLabel != null) plusLabel.text = "+";
        TextMeshProUGUI minusLabel = minusButtonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (minusLabel != null) minusLabel.text = "-";

        Button submitButton = submitButtonObj.GetComponentInChildren<Button>();
        Button plusButton = plusButtonObj.GetComponentInChildren<Button>();
        Button minusButton = minusButtonObj.GetComponentInChildren<Button>();

        submitButton.onClick.AddListener(() => UpdateSolved(levelManager.CheckGraphSolved()));
        plusButton.onClick.AddListener(() => RequestAddVisibleCard());
        minusButton.onClick.AddListener(() => RequestMinusVisibleCard());

        UpdateCardButtons();
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

    public void UpdateCardButtons()
    {
        // Remove old buttons (skip the first two: + and -)
        for (int i = 0; i < cardContentArea.childCount; i++)
        {
            Destroy(cardContentArea.GetChild(i).gameObject);
        }

        cardButtons.Clear();

        foreach (var card in deckManager.visibleCards)
        {
            int index = card.removedId;

            GameObject cardObj = Instantiate(cardUIPrefab, cardContentArea);
            cardButtons.Add(cardObj);

            CardButtonScript cardButtonScript = cardObj.GetComponent<CardButtonScript>();
            cardButtonScript.Initiate(levelManager, index);
            cardButtonScript.DrawCardSafe();

            Button cardButton = cardObj.GetComponentInChildren<Button>();
            cardButton.onClick.AddListener(() => deckManager.ToggleActiveCard(index));
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
    }
    
    public void Resume()
    {
        pauseMenu.SetActive(false);
    }
}
