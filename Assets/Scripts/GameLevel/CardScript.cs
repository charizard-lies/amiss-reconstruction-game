using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardScript : MonoBehaviour
{    //inherit
    private LevelScript levelManager;

    //attributes
    public int removedId;
    public Dictionary<int, NodeScript> nodeMap = new Dictionary<int, NodeScript>();
    public List<EdgeScript> allEdges = new List<EdgeScript>();
    public CardState cardState;

    //prefab
    private GameObject nodePrefab;
    private GameObject edgePrefab;

    //data
    private GraphData cardData;

    //dynamic
    public bool isActive;
    public bool isVisible;

    public void Initialize(int id, GraphData data, LevelScript level)
    {
        removedId = id;
        cardData = data;
        levelManager = level;
        nodePrefab = level.nodePrefab;
        edgePrefab = level.edgePrefab;

        if (SaveManager.CurrentState == null) return;
        if (SaveManager.CurrentState.idToCardStatesMap == null) return;

        cardState = SaveManager.CurrentState.idToCardStatesMap[removedId];
        

        isActive = false;
        isVisible = cardState.isVisible;
    }

    public void Build()
    {
        foreach (NodeState node in cardState.nodes) SpawnNode(node.nodeId);
        SpawnEdges(levelManager.edgeColor);
    }

    private void SpawnNode(int nodeId)
    {
        NodeState nodeState = cardState.nodes.First(node => node.nodeId == nodeId);
        Vector3 nodePos;
        AnchorScript spawnAnchor = null;

        if (nodeState.GetSnappedAnchorId() != null)
        {
            List<AnchorScript> anchors = levelManager.allAnchors;
            spawnAnchor = anchors.First(anchor => anchor.id == nodeState.GetSnappedAnchorId());
            nodePos = spawnAnchor.transform.position;
        }
        else nodePos = nodeState.pos;

        GameObject nodeObj = Instantiate(nodePrefab, nodePos, Quaternion.identity, transform);
        NodeScript node = nodeObj.GetComponent<NodeScript>();
        node.Initialize(nodeId, levelManager, this);
        nodeMap[nodeId] = node;

        if (spawnAnchor)
        {
            node.SnapToAnchor(spawnAnchor);
        }
    }

    private void SpawnEdges(Color edgeColor)
    {
        HashSet<(int, int)> createdEdges = new HashSet<(int, int)>();

        foreach (var pair in cardData.edges)
        {
            int nodeFromId = Mathf.Min(pair.fromNodeId, pair.toNodeId);
            int nodeToId = Mathf.Max(pair.fromNodeId, pair.toNodeId);

            if (createdEdges.Contains((nodeFromId, nodeToId))) continue;

            GameObject edgeObj = Instantiate(edgePrefab);
            EdgeScript edge = edgeObj.GetComponent<EdgeScript>();
            edge.Initialize(nodeMap[nodeFromId].transform, nodeMap[nodeToId].transform, levelManager.activeEdgeWidth, edgeColor);
            edgeObj.transform.SetParent(transform);

            allEdges.Add(edge);
            createdEdges.Add((nodeFromId, nodeToId));
        }

    }
    
    public void ToggleActive(bool makeActive)
    {
        isActive = makeActive;

        if (makeActive)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    public void ResetCard()
    {
        foreach (var kvp in nodeMap)
        {
            int nodeId = kvp.Key;
            NodeScript node = kvp.Value;

            if (cardState.nodeAnchorIdMap != null)
            {
                if (node.snappedAnchor != null)
                    node.snappedAnchor.Detach(node);

                AnchorScript originalAnchor = levelManager.anchorMap[cardState.nodeAnchorIdMap[nodeId]];
                node.transform.position = new Vector3(originalAnchor.transform.position.x, originalAnchor.transform.position.y, node.transform.position.z);
                originalAnchor.Attach(node);
                node.snappedAnchor = originalAnchor;
            }
            else
            {
                Debug.LogWarning($"No initial anchor found for node {nodeId} during ResetCard.");
            }
        }

        ToggleActive(false);
    }
}
