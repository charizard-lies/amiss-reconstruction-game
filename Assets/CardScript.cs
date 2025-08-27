using System.Collections.Generic;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    public int removedId;
    public GameObject nodePrefab;
    public GameObject edgePrefab;
    public int activeNodeLayer; //ActiveNode
    public int inactiveNodeLayer; //0
    public bool isActive;
    public bool isVisible;
    public Dictionary<int, NodeScript> nodeMap = new Dictionary<int, NodeScript>();
    public List<EdgeScript> allEdges = new List<EdgeScript>();

    private GraphData cardData;
    private LevelScript levelManager;
    private System.Random rng = new System.Random();
    private Dictionary<int, AnchorScript> initialNodeAnchorMap = new Dictionary<int, AnchorScript>();
    private void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public void Initialize(int id, GraphData data, int activeLayer, int inactiveLayer, LevelScript level)
    {
        removedId = id;
        cardData = data;
        activeNodeLayer = activeLayer;
        inactiveNodeLayer = inactiveLayer;
        isActive = false;
        isVisible = false;
        levelManager = level;
    }

    public void Build()
    {
        if (nodeMap.Count > 0 || allEdges.Count > 0)
        {
            Debug.LogWarning("Build() called on non-empty card. Did you forget ResetCard()?");
            return;
        }

        HashSet<(int, int)> createdEdges = new HashSet<(int, int)>();

        var validAnchors = levelManager.allAnchors.FindAll(anchor => anchor.id != removedId);
        //error check
        if (validAnchors.Count < cardData.nodeIds.Count)
        {
            Debug.LogError("Not enough anchors for nodes!");
            return;
        }

        Shuffle(validAnchors);

        int counter = 0;
        foreach (int id in cardData.nodeIds)
        {
            AnchorScript assignedAnchor = validAnchors[counter];
            initialNodeAnchorMap[id] = assignedAnchor;  // store initial anchor

            Vector3 nodePos = validAnchors[counter].transform.position;
            nodePos.z = -1;
            GameObject nodeObj = Instantiate(nodePrefab, nodePos, Quaternion.identity, transform);
            counter++;

            NodeScript node = nodeObj.GetComponent<NodeScript>();
            node.Initialize(id, removedId, activeNodeLayer, inactiveNodeLayer);
            nodeMap[id] = node;

            assignedAnchor.Attach(node);
            node.snappedAnchor = assignedAnchor;
        }

        foreach (var pair in cardData.edges)
        {
            int nodeFrom = pair.fromNodeId;
            int nodeTo = pair.toNodeId;

            int nodeA = Mathf.Min(nodeFrom, nodeTo);
            int nodeB = Mathf.Max(nodeFrom, nodeTo);

            if (createdEdges.Contains((nodeA, nodeB))) continue;

            GameObject edgeObj = Instantiate(edgePrefab, transform);
            EdgeScript edge = edgeObj.GetComponent<EdgeScript>();
            edge.Initialize(nodeMap[nodeA], nodeMap[nodeB]);

            allEdges.Add(edge);
            createdEdges.Add((nodeA, nodeB));
        }

        if (nodeMap == null)
        {
            Debug.Log("nodemap is null");
        }

        ToggleVisible(false);
    }

    public void ToggleActive(bool makeActive)
    {
        Debug.Log($"card {removedId} is being set to active({makeActive})!!!!");
        isActive = makeActive;
        float alpha = makeActive ? 1f : 0.1f;
        SetCardAlpha(gameObject, alpha);

        if (makeActive)
        {
            gameObject.layer = activeNodeLayer;
            foreach (Transform child in transform)
            {
                child.gameObject.layer = activeNodeLayer;
            }
        }
        else
        {
            gameObject.layer = inactiveNodeLayer;
            foreach (Transform child in transform)
            {
                child.gameObject.layer = inactiveNodeLayer;
            }
        }
    }

    public void ToggleVisible(bool makeVisible)
    {
        Debug.Log($"card {removedId} is being set to visible({makeVisible})!!!!");
        isVisible = makeVisible;
        float alpha = makeVisible ? 0.1f : 0f;
        SetCardAlpha(gameObject, alpha);

        if (isActive && !makeVisible)
        {
            ToggleActive(false);
        }
    }

    private void SetCardAlpha(GameObject card, float alpha)
    {
        SpriteRenderer[] sprites = card.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in sprites)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        LineRenderer[] edges = card.GetComponentsInChildren<LineRenderer>();
        foreach (var edge in edges)
        {
            Color c = edge.startColor;
            c.a = alpha;
            edge.startColor = c;
            edge.endColor = c;
        }
    }
    public void ResetCard()
    {
        foreach (var kvp in nodeMap)
        {
            int nodeId = kvp.Key;
            NodeScript node = kvp.Value;

            if (initialNodeAnchorMap.TryGetValue(nodeId, out AnchorScript anchor))
            {
                // Detach from current anchor if any
                if (node.snappedAnchor != null)
                    node.snappedAnchor.Detach(node);

                // Move node to initial anchor position and attach
                node.transform.position = new Vector3(anchor.transform.position.x, anchor.transform.position.y, node.transform.position.z);
                anchor.Attach(node);
                node.snappedAnchor = anchor;
            }
            else
            {
                Debug.LogWarning($"No initial anchor found for node {nodeId} during ResetCard.");
            }
        }

        // Optionally reset visibility/active states, alpha, etc.
        ToggleVisible(false);
        ToggleActive(false);
    }
}
