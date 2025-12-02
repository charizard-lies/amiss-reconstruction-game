using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//I build your graph from data and control the parameters regarding behaviour of the graph elements.
public class DeckScript : MonoBehaviour
{
    //inherit
    private LevelScript levelManager;
    private GraphData graphData;
    public LevelState levelState;

    //attributes
    public List<CardScript> allCards = new List<CardScript>();
    private List<GameObject> overlayEdges = new List<GameObject>();

    //prefab
    private GameObject cardPrefab;
    private GameObject edgePrefab;

    //dynamic
    public CardScript activeCard;

    public void Initialize(LevelScript level)
    {
        levelManager = level;
        graphData = levelManager.graphData;
        cardPrefab = level.cardPrefab;
        edgePrefab = level.edgePrefab;
        levelState = SaveManager.CurrentState;
    }

    public void Build()
    {
        for (int i = 0; i < graphData.nodeIds.Count; i++)
        {
            //change to prefab
            GameObject card = Instantiate(cardPrefab, transform);
            card.name = $"Card_{i}";

            CardScript cardScript = card.GetComponent<CardScript>();
            cardScript.Initialize(i, graphData.GraphReduce(i), levelManager);

            cardScript.Build();
            allCards.Add(cardScript);
            card.SetActive(false);
        }
        ToggleActiveCard(levelState.activeCardId);
    }

    public void ResetDeck()
    {
        foreach (CardScript card in allCards)
        {
            card.ResetCard();
        }
        ToggleActiveCard(levelState.activeCardId);
    }

    public void RedrawOverlayGraph()
    {
        foreach (GameObject oldEdge in overlayEdges)
        {
            Destroy(oldEdge);
        }
        overlayEdges.Clear();

        List<CardScript> cardsToOverlay = allCards.Where(n => n != activeCard && n.isVisible).ToList();
        GraphData overlayGraph = levelManager.BuildOverlayGraph(cardsToOverlay);

        foreach (var edge in overlayGraph.edges)
        {
            GameObject edgeObj = Instantiate(edgePrefab, transform);
            overlayEdges.Add(edgeObj);
            EdgeScript edgeScript = edgeObj.GetComponent<EdgeScript>();
            edgeScript.Initialize(levelManager.anchorMap[edge.fromNodeId].transform, levelManager.anchorMap[edge.toNodeId].transform, levelManager.overlayEdgeWidth, new Color(1f, 1f, 1f, levelManager.overlayEdgeAlpha));
        }
    }

    public void ToggleActiveCard(int id)
    {
        Debug.Log($"toggle card {id} active");
        if (activeCard) activeCard.ToggleActive(false);

        CardScript newActiveCard = allCards.First(card => card.removedId == id);
        levelState.activeCardId = id;

        if (!newActiveCard.isVisible)
        {
            ToggleVisibleCard(id, true);
            GameObject cardButtonObj = levelManager.UIManager.cardButtons.First(card => card.GetComponent<CardButtonScript>().cardId == id);
            CardButtonScript cardButtonScript = cardButtonObj.GetComponent<CardButtonScript>();
            cardButtonScript.Slide(true);
        }

        newActiveCard.ToggleActive(true);
        activeCard = newActiveCard;

        levelManager.UIManager.UpdateCardButtons();
        RedrawOverlayGraph();
    }
    
    public void ToggleVisibleCard(int id, bool makeVisible)
    {
        CardScript cardToToggle = allCards.First(card => card.removedId == id);

        if (cardToToggle.isActive && !makeVisible)
        {
            CardScript cardToReplaceActive = allCards.First(card => card.isVisible);
            ToggleActiveCard(cardToReplaceActive.removedId);
        }

        levelState.idToCardStatesMap[id].isVisible = makeVisible;

        cardToToggle.isVisible = makeVisible;
        levelManager.UIManager.UpdateSolved(levelManager.CheckGraphSolved());
        RedrawOverlayGraph();
        return;
    }

    public void AddVisibleCard()
    {
        CardScript cardToShow = allCards.First(card => !card.isVisible);
        ToggleVisibleCard(cardToShow.removedId, true);
    }

    public void MinusVisibleCard()
    {
        CardScript cardToHide = allCards.First(card => card.isVisible);
        ToggleVisibleCard(cardToHide.removedId, false);
        cardToHide.ResetCard();
    }
}
