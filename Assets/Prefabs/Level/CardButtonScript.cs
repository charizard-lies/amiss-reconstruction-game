using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using System;

public class CardButtonScript : MonoBehaviour
{
    public GameObject nodeUIPrefab;
    public GameObject edgeUIPrefab;
    public float horizontalPadding;

    private int cardId;
    private LevelScript levelManager;
    private CardScript cardToDraw;
    private RectTransform cardRectTransform;

    public void Initiate(LevelScript level, int id)
    {
        cardId = id;
        levelManager = level;
        cardToDraw = level.deck.visibleCards.First(card => card.removedId == id);
        cardRectTransform = GetComponent<RectTransform>();
    }

    public void DrawCardSafe()
    {
        StartCoroutine(DrawCardWhenReady());
    }

    private IEnumerator DrawCardWhenReady()
    {
        yield return null;
        DrawCard();
    }

    private void DrawCard()
    {
        ClearChildren(cardRectTransform);

        foreach (NodeScript node in cardToDraw.nodeMap.Values)
        {
            if (!node.snappedAnchor) continue;
            
            Debug.Log(node.nodeId + ":\n");
            GameObject nodeObj = Instantiate(nodeUIPrefab, cardRectTransform);
            nodeObj.GetComponent<RectTransform>().anchoredPosition = WorldToUIPos(node.transform.position);
        }

        foreach (var edge in cardToDraw.allEdges)
        {
            if (!edge.PointA.GetComponent<NodeScript>().snappedAnchor || !edge.PointB.GetComponent<NodeScript>().snappedAnchor)
                continue;
            var edgeObj = Instantiate(edgeUIPrefab, cardRectTransform);
            DrawUILine(edgeObj.GetComponent<RectTransform>(),
                       WorldToUIPos(edge.PointA.position),
                       WorldToUIPos(edge.PointB.position));
        }
    }

    void ClearChildren(RectTransform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }

    Vector2 WorldToUIPos(Vector3 worldPos) {
        Vector3 parentPosition = levelManager.transform.position;
        Vector3 positionRelativeToParent = worldPos - parentPosition;
        float parentwidth = levelManager.initRadius * 2f;
        Vector3 positionProportionalToParent = positionRelativeToParent / parentwidth;
        float cardBoxLength = Math.Min(cardRectTransform.rect.width, cardRectTransform.rect.height) - horizontalPadding*2;
        Vector3 positionOnCard = positionProportionalToParent * cardBoxLength;

        Debug.Log($"({positionRelativeToParent.x},{positionRelativeToParent.y})");
        Debug.Log($"*({cardRectTransform.rect.width}, {cardRectTransform.rect.height})");
        return new Vector2(positionOnCard.x, positionOnCard.y);
    }

    void DrawUILine(RectTransform line, Vector2 start, Vector2 end)
    {
        Vector2 dir = (end - start).normalized;
        float dist = Vector2.Distance(start, end);
        line.sizeDelta = new Vector2(dist, 2f);
        line.anchoredPosition = (start + end) / 2f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        line.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
