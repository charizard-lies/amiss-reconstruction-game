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
    private LevelUI UIManager;
    private Sprite normalCardSprite;
    private Sprite activeCardSprite;
    private Color normalGraphColor;
    private Color activeGraphColor;

    public float horizontalPadding;
    public float lineWidth;

    public int cardId;
    private LevelScript levelManager;
    private CardScript cardToDraw;
    private RectTransform cardRectTransform;

    public void Initiate(LevelScript level, LevelUI UI, int id)
    {
        cardId = id;
        levelManager = level;
        cardToDraw = level.deck.visibleCards.First(card => card.removedId == id);
        cardRectTransform = GetComponent<RectTransform>();
        UIManager = UI;
        normalCardSprite = UIManager.normalCardSprite;
        activeCardSprite = UIManager.activeCardSprite;
        normalGraphColor = UIManager.normalGraphColor;
        activeGraphColor = UIManager.activeGraphColor;

    }

    public void DrawCardSafe(bool isActive)
    {
        StartCoroutine(DrawCardWhenReady(isActive));
    }

    private IEnumerator DrawCardWhenReady(bool isActive)
    {
        yield return null;
        DrawCard(isActive);
    }

    private void DrawCard(bool isActive)
    {
        ClearChildren(cardRectTransform);

        if (isActive) gameObject.GetComponent<Image>().sprite = activeCardSprite;
        else gameObject.GetComponent<Image>().sprite = normalCardSprite;

        foreach (NodeScript node in cardToDraw.nodeMap.Values)
        {
            if (!node.snappedAnchor) continue;
            
            GameObject nodeObj = Instantiate(nodeUIPrefab, cardRectTransform);
            nodeObj.GetComponent<RectTransform>().anchoredPosition = WorldToUIPos(node.transform.position);
            nodeObj.GetComponent<Image>().color = isActive ? activeGraphColor : normalGraphColor;
        }

        foreach (var edge in cardToDraw.allEdges)
        {
            if (!edge.PointA.GetComponent<NodeScript>().snappedAnchor || !edge.PointB.GetComponent<NodeScript>().snappedAnchor)
                continue;
            var edgeObj = Instantiate(edgeUIPrefab, cardRectTransform);
            float adjustedLineWidth = isActive ? lineWidth * 2 : lineWidth;
            DrawUILine(edgeObj.GetComponent<RectTransform>(),
                       WorldToUIPos(edge.PointA.position),
                       WorldToUIPos(edge.PointB.position),
                       adjustedLineWidth);
            edgeObj.GetComponent<Image>().color = isActive ? activeGraphColor : normalGraphColor;
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

        return new Vector2(positionOnCard.x, positionOnCard.y);
    }

    void DrawUILine(RectTransform line, Vector2 start, Vector2 end, float width)
    {
        Vector2 dir = (end - start).normalized;
        float dist = Vector2.Distance(start, end);
        line.sizeDelta = new Vector2(dist, width);
        line.anchoredPosition = (start + end) / 2f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        line.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
