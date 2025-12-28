using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;

public class CardButtonScript : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject nodeUIPrefab;
    public GameObject edgeUIPrefab;

    [Header("Managers")]
    public LevelScript levelManager;
    private LevelUI UIManager;

    [Header("Layout")]
    public RectTransform pictureArea;
    public float normalLineWidth;
    public float activeLineWidth;
    private Sprite normalCardSprite;
    private Sprite activeCardSprite;
    private Color normalGraphColor;
    private Color activeGraphColor;

    [Header("Other")]
    public int cardId;
    public bool isSnapping = false;
    public Dictionary<int, NodeScript> idToNodeScriptMap;
    private Button cardButton;

    public void Initiate(LevelScript level, LevelUI UI, int id)
    {
        levelManager = level;
        UIManager = UI;
        cardId = id;

        normalLineWidth = UIManager.normalLineWidth;
        activeLineWidth = UIManager.activeLineWidth;
        normalCardSprite = UIManager.normalCardSprite;
        activeCardSprite = UIManager.activeCardSprite;
        normalGraphColor = UIManager.normalGraphColor;
        activeGraphColor = UIManager.activeGraphColor;
    }

    public void DrawCardAfterFrame()
    {
        StartCoroutine(DrawCardWhenReady());
    }

    private IEnumerator DrawCardWhenReady()
    {
        yield return null;
        DrawCard();
    }

    public void DrawCard()
    {
        ClearChildren(pictureArea);
        bool isActive = levelManager.removedId == cardId;

        if (isActive) gameObject.GetComponent<Image>().sprite = activeCardSprite;
        else gameObject.GetComponent<Image>().sprite = normalCardSprite;

        List<Vector3> cardNodePosMap = levelManager.ReturnNodePosMap(cardId);

        foreach (var node in levelManager.graphData.nodes.Values)
        {
            if(node.id == cardId) continue;
            GameObject nodeObj = Instantiate(nodeUIPrefab, pictureArea);
            nodeObj.GetComponent<RectTransform>().anchoredPosition = GraphToCardPos(cardNodePosMap[node.id]);
            
            nodeObj.GetComponent<Image>().color = isActive ? activeGraphColor : normalGraphColor;
        }

        foreach (var edge in levelManager.graphData.edges)
        {
            if(edge.fromNodeId == cardId || edge.toNodeId == cardId) continue;

            var edgeObj = Instantiate(edgeUIPrefab, pictureArea);

            DrawUILine(edgeObj.GetComponent<RectTransform>(),
                       GraphToCardPos(cardNodePosMap[edge.fromNodeId]),
                       GraphToCardPos(cardNodePosMap[edge.toNodeId]),
                       isActive ? activeLineWidth: normalLineWidth);

            edgeObj.GetComponent<Image>().color = isActive ? activeGraphColor : normalGraphColor;
        }
    }

    void ClearChildren(RectTransform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }

    Vector2 GraphToCardPos(Vector3 localPos)
    {
        float parentWidth = levelManager.initRadius * 2f;
        float pictureAreaWidth = Math.Min(pictureArea.rect.width, pictureArea.rect.height);
        Vector3 positionOnCard = localPos / parentWidth * (pictureAreaWidth * 0.5f);

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
