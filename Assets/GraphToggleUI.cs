using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// I control all active, visible, and invisible layers and their editabilty.
// I control buttons that toggle between these layers.

public class GraphToggleUI : MonoBehaviour
{
    public TextMeshProUGUI solvedLabel;

    [Header("UI References")]
    public GameObject buttonPrefab;     // Your TMP button prefab
    public Transform buttonParent;      // ToggleList

    [Header("Graph References")]
    public DeckScript deckManager;
    public LevelScript levelManager;

    private int currentLayerIndex = -1;
    private List<GameObject> cardButtons = new List<GameObject>();

    public void AddSolvedLabel()
    {
        GameObject labelObj = new GameObject("SolvedLabel");
        labelObj.transform.SetParent(buttonParent, false);

        solvedLabel = labelObj.AddComponent<TextMeshProUGUI>();
        solvedLabel.text = "Unsolved";
        solvedLabel.fontSize = 24;
        solvedLabel.alignment = TextAlignmentOptions.Center;
    }
    public void InitButtons(GraphData graphData)
    {
        AddSolvedLabel();
        //create 2 buttons, 1 to increase visible layers by 1 and the other to decrease by 1
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

        submitButton.onClick.AddListener(() => levelManager.checkGraph());
        plusButton.onClick.AddListener(() => RequestAddVisibleCard());
        minusButton.onClick.AddListener(() => RequestMinusVisibleCard());

        UpdateCardButtons();
    }

    public void UpdateCardButtons()
    {
        // Remove old buttons (skip the first two: + and -)
        for (int i = 4; i < buttonParent.childCount; i++)
        {
            Destroy(buttonParent.GetChild(i).gameObject);
        }

        cardButtons.Clear();

        foreach (var card in deckManager.visibleCards)
        {
            int index = card.removedId;

            GameObject btnObj = Instantiate(buttonPrefab, buttonParent);
            cardButtons.Add(btnObj);

            TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = "Card " + index;

            Button cardButton = btnObj.GetComponentInChildren<Button>();

            cardButton.onClick.AddListener(() => deckManager.ToggleActiveCard(index));

        }
    }

    public void UpdateSolved(bool solved)
    {
        if (solved) solvedLabel.text = "Solved";
        else solvedLabel.text = "Unsolved";
    }

    void RequestAddVisibleCard()
    {
        deckManager.AddVisibleCard();
        UpdateCardButtons();
    }

    void RequestMinusVisibleCard()
    {
        deckManager.MinusVisibleCard();
        UpdateCardButtons();
    }
}
