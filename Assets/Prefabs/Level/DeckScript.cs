using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//I build your graph from data and control the parameters regarding behaviour of the graph elements.
public class DeckScript : MonoBehaviour
{
    //inherit
    private LevelScript levelManager;
    private GraphData graphData;

    //attributes
    public List<CardScript> visibleCards = new List<CardScript>();
    public List<CardScript> invisibleCards = new List<CardScript>();
    private List<GameObject> overlayEdges = new List<GameObject>();

    //prefab
    private GameObject cardPrefab;
    private GameObject edgePrefab;

    //dynamic
    private CardScript activeCard;

    public void Initialize(LevelScript level)
    {
        levelManager = level;
        graphData = levelManager.graphData;
        cardPrefab = level.cardPrefab;
        edgePrefab = level.edgePrefab;
    }

    public void BuildDeck()
    {
        for (int i = 0; i < graphData.nodeIds.Count; i++)
        {
            //change to prefab
            GameObject card = Instantiate(cardPrefab, transform);
            card.name = $"Card_{i}";

            CardScript cardScript = card.GetComponent<CardScript>();
            cardScript.Initialize(i, graphData.GraphReduce(i), levelManager);

            cardScript.Build();
            invisibleCards.Add(cardScript);
        }

        AddVisibleCard();
        AddVisibleCard();
        AddVisibleCard();

        ToggleActiveCard(visibleCards[0].removedId);
    }

    public void RedrawOverlayGraph()
    {
        foreach (GameObject oldEdge in overlayEdges)
        {
            Destroy(oldEdge);
        }
        overlayEdges.Clear();

        List<CardScript> cardsToOverlay = visibleCards.Where(n => n != activeCard).ToList();
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
        //assumption: no invisible card will be toggled
        if (activeCard) activeCard.ToggleActive(false);
        foreach (CardScript card in visibleCards)
        {
            if (card.removedId == id)
            {
                //test
                if (!card.isVisible)
                {
                    Debug.LogWarning($"Invisible card ({card.removedId}) is being toggled to active");
                    return;
                }
                //test

                card.ToggleActive(true);
                activeCard = card;
                //Debug.Log($"card {card.removedId} made active");
            }
        }

        levelManager.UIManager.UpdateCardButtons();
        RedrawOverlayGraph();
    }
    
    private void ToggleVisibleCard(int id, bool makeVisible)
    {
        List<CardScript> temp = makeVisible ? invisibleCards : visibleCards;

        foreach (CardScript card in temp)
        {
            if (card.removedId == id)
            {
                if (!makeVisible)
                {
                    bool wasActive = (activeCard == card);
                    visibleCards.Remove(card);
                    invisibleCards.Add(card);

                    card.ToggleVisible(false);
                    //Debug.Log($"card {card.removedId} is turning invisible");

                    if (wasActive && visibleCards.Count > 0)
                    {
                        ToggleActiveCard(visibleCards[0].removedId);
                    }
                }
                else
                {
                    invisibleCards.Remove(card);
                    visibleCards.Add(card);

                    card.ToggleVisible(true);
                    //Debug.Log($"card {card.removedId} is turning visible");
                }
                RedrawOverlayGraph();
                return;
            }
        }
    }

    public void AddVisibleCard()
    {
        if (invisibleCards.Count == 0)
        {
            Debug.LogWarning("No invisible cards available to add.");
            return;
        }
        
        invisibleCards.Sort((a, b) => a.removedId.CompareTo(b.removedId));
        CardScript cardToShow = invisibleCards[0];
        
        ToggleVisibleCard(cardToShow.removedId, true);
    }

    public void MinusVisibleCard()
    {
        if (visibleCards.Count <= 1)
        {
            Debug.LogWarning("At least one card must remain visible.");
            return;
        }

        CardScript cardToHide = visibleCards[visibleCards.Count - 1];
        ToggleVisibleCard(cardToHide.removedId, false);

        //reset card
        cardToHide.ResetCard();
    }
}
