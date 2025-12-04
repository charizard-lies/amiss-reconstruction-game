using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;

[System.Serializable]
public class NodeEntry
{
    public int id;
    public NodeScript node;
}

public class CardButtonScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Prefabs")]
    public GameObject nodeUIPrefab;
    public GameObject edgeUIPrefab;

    [Header("Managers")]
    public LevelScript levelManager;
    private LevelUI UIManager;

    [Header("Layout")]
    public float horizontalPadding;
    public float lineWidth;

    private Sprite normalCardSprite;
    private Sprite activeCardSprite;
    private Color normalGraphColor;
    private Color activeGraphColor;


    [Header("Swipe Settings")]
    public float maxOffset = 50f; // how far the card can move upward
    public float snapSpeed = 10f; // smoothing for snapping animation
    public float dragThreshold = 10f;
    public Vector2 bottomPos;
    public Vector2 topPos;
    private Vector2 pointerInitialPos;


    [Header("Swipe Events")]
    public UnityEvent OnSnapTop = new UnityEvent();
    public UnityEvent OnSnapBottom = new UnityEvent();


    [Header("Other")]
    public int cardId;
    public bool isSnapping = false;
    public CardScript card;
    public Dictionary<int, NodeScript> idToNodeScriptMap;
    private RectTransform rect;
    private Vector2 targetPos;
    private Button cardButton;

    public void Initiate(LevelScript level, LevelUI UI, int id)
    {
        cardId = id;
        levelManager = level;
        card = level.deck.allCards.First(card => card.removedId == id);
        idToNodeScriptMap = card.nodeMap;

        rect = GetComponent<RectTransform>();
        cardButton = GetComponent<Button>();
        UIManager = UI;
        bottomPos = rect.anchoredPosition;
        topPos = new Vector2(bottomPos.x, bottomPos.y + maxOffset);

        normalCardSprite = UIManager.normalCardSprite;
        activeCardSprite = UIManager.activeCardSprite;
        normalGraphColor = UIManager.normalGraphColor;
        activeGraphColor = UIManager.activeGraphColor;

        cardButton.onClick.AddListener(() => level.deck.ToggleActiveCard(cardId));
        OnSnapTop.AddListener(() =>
            UIManager.levelManager.deck.ToggleVisibleCard(cardId, true)
        );
        OnSnapBottom.AddListener(() =>
            UIManager.levelManager.deck.ToggleVisibleCard(cardId, false)
        );
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

    private void DrawCard()
    {
        ClearChildren(rect);
        bool isActive = SaveManager.CurrentState.activeCardId == cardId;

        if (isActive) gameObject.GetComponent<Image>().sprite = activeCardSprite;
        else gameObject.GetComponent<Image>().sprite = normalCardSprite;

        foreach (NodeScript node in idToNodeScriptMap.Values)
        {
            if (!node.snappedAnchor) continue;

            GameObject nodeObj = Instantiate(nodeUIPrefab, rect);
            nodeObj.GetComponent<RectTransform>().anchoredPosition = WorldToUIPos(node.transform.position);
            nodeObj.GetComponent<Image>().color = isActive ? activeGraphColor : normalGraphColor;
        }

        foreach (var edge in card.allEdges)
        {
            if (!edge.PointA.GetComponent<NodeScript>().snappedAnchor || !edge.PointB.GetComponent<NodeScript>().snappedAnchor)
                continue;
            var edgeObj = Instantiate(edgeUIPrefab, rect);
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

    Vector2 WorldToUIPos(Vector3 worldPos)
    {
        Vector3 parentPosition = levelManager.transform.position;
        Vector3 positionRelativeToParent = worldPos - parentPosition;
        float parentwidth = levelManager.initRadius * 2f;
        Vector3 positionProportionalToParent = positionRelativeToParent / parentwidth;
        float cardBoxLength = Math.Min(rect.rect.width, rect.rect.height) - horizontalPadding * 2;
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (card.isActive) return;
        pointerInitialPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (card.isActive) return;

        if (Vector2.Distance(pointerInitialPos, eventData.position) > dragThreshold) cardButton.interactable = false;

        Vector2 drag = eventData.delta;
        float newY = rect.anchoredPosition.y + drag.y;
        newY = Mathf.Clamp(newY, bottomPos.y, topPos.y);

        rect.anchoredPosition = new Vector2(bottomPos.x, newY);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (card.isActive) return;
        isSnapping = true;
        cardButton.interactable = true;

        float currentY = rect.anchoredPosition.y;
        float halfway = (topPos.y + bottomPos.y) / 2f;

        if (currentY > halfway)
        {
            targetPos = topPos;
        }
        else
        {
            targetPos = bottomPos;
            OnSnapBottom?.Invoke();
        }
    }


    public void Slide(bool slideUp)
    {
        targetPos = slideUp ? topPos : bottomPos;
        isSnapping = true;
    }

    private void SnapCardStep(Vector2 targetPosition)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPosition, Time.deltaTime * snapSpeed);

        if (Vector2.Distance(rect.anchoredPosition, targetPosition) < 0.1f)
        {
            if (targetPosition == topPos)
            {
                OnSnapTop?.Invoke();
            }
            else
            {
                OnSnapBottom?.Invoke();
            }
            rect.anchoredPosition = targetPosition;
            isSnapping = false;
        }
    }

    void Update()
    {
        if (!isSnapping) return;
        SnapCardStep(targetPos);
    }
}
