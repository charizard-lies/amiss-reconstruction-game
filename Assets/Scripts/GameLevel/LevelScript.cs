using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
    [Header("Attributes")]
    public GraphData graphData;
    public float initRadius;
    public float nodeAttractionTime=0.1f;
    public Color edgeColor = Color.white;
    public float edgeWidth = 0.2f;
    public Transform levelParent;
    public string levelIndex;
    // public LevelState levelState;


    [Header("Prefabs")]
    public GameObject nodePrefab;
    public GameObject edgePrefab;

    [Header("Other")]
    public LevelUI UIManager;
    public List<Vector3> currNodePos;
    public List<NodeScript> nodeScripts;
    public List<char> colors;
    public List<Dictionary<int, EdgeScript>> drawnEdges = new List<Dictionary<int, EdgeScript>>();
    public EdgeScript currentDrawingEdge;


    public int removedId;
    public bool gamePaused;

    void Start()
    {
        // if (GameManager.Instance)
        // {
        //     levelIndex = GameManager.Instance.selectedLevelId;
        //     if (GameManager.Instance.selectedDailyLevel) graphData = Resources.Load<GraphData>($"Levels/Daily/Level{levelIndex}");
        //     else graphData = Resources.Load<GraphData>($"Levels/Normal/Level{levelIndex}");   
        // }

        // SaveManager.CurrentState = LoadLevelState();
        // levelState = SaveManager.CurrentState;
        
        graphData = Resources.Load<GraphData>($"Levels/Normal/Level1");   
        for(int i = 0; i < graphData.nodes.Count(); i++)
        {
            drawnEdges.Add(new Dictionary<int, EdgeScript>());
        }

        gamePaused = false;

        // UIManager.CreateCardButtons();
        BuildCard(removedId);
    }

    public void BuildCard(int removedId)
    {
        SetNodePos(removedId);
        ComputeWLColouring(removedId);
        
        for(int i = 0; i < graphData.nodes.Count(); i++)
        {
            nodeScripts.Add(BuildNode(i));
        } 
        BuildEdges();


    }
    
    private NodeScript BuildNode(int id)
    {
        GameObject nodeObj = Instantiate(nodePrefab, currNodePos[id], Quaternion.identity, transform);
        NodeScript nodeScript = nodeObj.GetComponent<NodeScript>();
        nodeScript.Initialize(id, this);
        return nodeScript;
    }

    private EdgeScript BuildEdge(int a, int? b)
    {
        GameObject edgeObj = Instantiate(edgePrefab, transform);
        EdgeScript edgeScript = edgeObj.GetComponent<EdgeScript>();
        if (b==null) edgeScript.Initialize(nodeScripts[a].transform, null, this);
        else edgeScript.Initialize(nodeScripts[a].transform, nodeScripts[b.Value].transform, this);

        return edgeScript;
    }

    private void BuildEdges()
    {
        foreach(GraphData.Edge edge in graphData.edges)
        {
            if(edge.fromNodeId == removedId || edge.toNodeId == removedId) continue;
            BuildEdge(edge.fromNodeId, edge.toNodeId);
        }
    }

    public void PenDown()
    {
        currentDrawingEdge = BuildEdge(removedId, null);
    }

    public void PenUp(int? nodeId)
    {
        if(nodeId == null || drawnEdges[removedId].TryGetValue(nodeId.Value, out _))
        {
            Destroy(currentDrawingEdge.gameObject);
            currentDrawingEdge = null;
            return;
        }
        
        currentDrawingEdge.PointB = nodeScripts[nodeId.Value].transform;
        drawnEdges[removedId][nodeId.Value] = currentDrawingEdge;
        currentDrawingEdge = null;

        Debug.Log(CheckGraph());
    }

    public void EraseEdge(int nodeId)
    {
        if(!drawnEdges[removedId].TryGetValue(nodeId, out _)) return;
        EdgeScript edgeToErase = drawnEdges[removedId][nodeId];
        if(!edgeToErase) return;

        Destroy(edgeToErase.gameObject);
        drawnEdges[removedId].Remove(nodeId);
        Debug.Log(CheckGraph());
        return;
    }

    public void Restart()
    {
        gamePaused = false;
        UIManager.hasShownWin = false;
    }

    public bool CheckGraph()
    {
        List<int> neighboursIds = graphData.nodes[removedId].adjacentNodeIds;
        List<char> neighbourLabels = neighboursIds.Select(id => colors[id]).ToList();

        List<char> drawnNeighbourLabels = new List<char>();
        foreach(EdgeScript edgeScript in drawnEdges[removedId].Values)
        {
            NodeScript connectedNode = edgeScript.PointB.gameObject.GetComponent<NodeScript>();
            drawnNeighbourLabels.Add(colors[connectedNode.id]);
        }

        return neighbourLabels.OrderBy(x=>x).SequenceEqual(drawnNeighbourLabels.OrderBy(x=>x));
    }

    public void ComputeWLColouring(int cardId)
    {
        colors.Clear();

        List<char> currColors = new List<char>();
        foreach(var node in graphData.nodes)
        {
            if(node.id == cardId) currColors.Add('a');
            else currColors.Add(IntToLetter(node.adjacentNodeIds.Count(neighbourId => neighbourId!=cardId)));
        }
        
        int oldSignatureCount=0;
        for(int i = 0; i < graphData.nodes.Count(); i++)
        {
            Dictionary<int,string> signatures = new Dictionary<int, string>();

            for(int j = 0; j < graphData.nodes.Count(); j++)
            {
                List<char> neighbourColors = graphData.nodes[i].adjacentNodeIds
                    .Where(id => id != cardId)
                    .Select(id => currColors[id])
                    .OrderBy(x => x)
                    .ToList();

                string signature = currColors[j] + "|" + string.Join(",", neighbourColors);
                signatures[j] = signature;
            }

            List<string> uniqueSignatures = signatures.Values
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            if (uniqueSignatures.Count() == oldSignatureCount) break;

            oldSignatureCount = uniqueSignatures.Count();
            for(int j = 0; j < graphData.nodes.Count(); j++)
            {
                if(j == removedId) continue;
                currColors[j] = IntToLetter(uniqueSignatures.IndexOf(signatures[j]));
                Debug.Log($"node {j} color: {currColors[j]}");
            }
        }

        colors = currColors;
    }

    private char IntToLetter(int num)
    {
        return (char)('a' + num);
    }

    private void Update()
    {
        if(gamePaused) return;

        if (Mouse.current.leftButton.wasReleasedThisFrame && currentDrawingEdge != null)
        {
            foreach(var nodeScript in nodeScripts)
            {
                if(nodeScript.id == removedId) continue;
                if(!nodeScript.MouseIsOver()) continue;

                PenUp(nodeScript.id);
            }
            if (currentDrawingEdge) PenUp(null);
        }
    }

    // private LevelState LoadLevelState()
    // {
    //     LevelState loaded = SaveManager.Load(levelIndex);
    //     if (loaded != null) return loaded;

    //     return CreateFreshLevelState();
    // }

    // private LevelState CreateFreshLevelState()
    // {
    //     LevelState state = new LevelState();
    //     state.levelIndex = levelIndex;
    //     state.activeCardId = graphData.nodeIds[0];
    //     state.solved = false;

    //     foreach (int nodeId in graphData.nodeIds)
    //     {
    //         state.idToCardStatesMap[nodeId] = CreateFreshCardState(nodeId);
    //     }
    //     state.EnsureList();

    //     return state;
    // }

    // private CardState CreateFreshCardState(int removedId)
    // {
    //     CardState state = new CardState();
    //     GraphData reducedGraphData = graphData.GraphReduce(removedId);
    //     List<int> shuffledAnchorIds = ShuffledAnchorIds(removedId);

    //     state.isVisible = true;
    //     state.nodeAnchorIdMap = reducedGraphData.nodeIds
    //     .Select((nodeId, i) => new { nodeId, value = shuffledAnchorIds[i] })
    //     .ToDictionary(x => x.nodeId, x => x.value);


    //     foreach(int nodeId in graphData.nodeIds)
    //     {
    //         if (nodeId == removedId) continue;

    //         NodeState nodeState = new NodeState();
    //         nodeState.nodeId = nodeId;
    //         nodeState.snapped = true;
    //         nodeState.snappedAnchorId = state.nodeAnchorIdMap[nodeId];
    //         nodeState.pos = anchorMap[state.nodeAnchorIdMap[nodeId]].transform.position;

    //         state.nodes.Add(nodeState);
    //     }

    //     return state;
    // }



    // public bool CheckGraphSolved()
    // {
    //     GraphData overlayGraph = BuildOverlayGraph(deck.allCards.Where(card => card.isVisible).ToList());
    //     bool overlayCorrect = CheckIsomorphism(graphData, overlayGraph);

    //     return overlayCorrect;
    // }

    // public bool CheckIsomorphism(GraphData g1, GraphData g2)
    // {
    //     int n = g1.nodeIds.Count;
    //     if (n != g2.nodeIds.Count || g1.edges.Count != g2.edges.Count)
    //         return false;

    //     // Normalize both graphs into adjacency sets
    //     var g1Adj = BuildAdjacencyList(g1);
    //     var g2Adj = BuildAdjacencyList(g2);

    //     // Generate all permutations of node indices [0...n-1]
    //     var permutations = GetPermutations(Enumerable.Range(0, n).ToList());

    //     foreach (var perm in permutations)
    //     {
    //         if (IsPermutationIsomorphic(g1Adj, g2Adj, perm))
    //             return true;
    //     }

    //     return false;
    // }

    // private static Dictionary<int, HashSet<int>> BuildAdjacencyList(GraphData g)
    // {
    //     var map = new Dictionary<int, int>();
    //     var sortedNodes = g.nodeIds.OrderBy(id => id).ToList();

    //     //map nodeid to index (ids may not be consecutive)
    //     for (int i = 0; i < sortedNodes.Count; i++)
    //         map[sortedNodes[i]] = i;

    //     //construct adjacency list
    //     var adj = new Dictionary<int, HashSet<int>>();
    //     for (int i = 0; i < sortedNodes.Count; i++)
    //         adj[i] = new HashSet<int>();

    //     foreach (var edge in g.edges)
    //     {
    //         int a = map[edge.fromNodeId];
    //         int b = map[edge.toNodeId];
    //         adj[a].Add(b);
    //         adj[b].Add(a); // undirected
    //     }

    //     return adj;
    // }
    // private static List<List<int>> GetPermutations(List<int> list)
    // {
    //     var result = new List<List<int>>();
    //     Permute(list, 0, result);
    //     return result;
    // }

    // private static void Permute(List<int> list, int start, List<List<int>> result)
    // {
    //     if (start >= list.Count)
    //     {
    //         result.Add(new List<int>(list));
    //         return;
    //     }

    //     for (int i = start; i < list.Count; i++)
    //     {
    //         //place every other number into the start position, including itself
    //         (list[start], list[i]) = (list[i], list[start]);
    //         //permute the rest of the list, keeping everything at and before start the same.
    //         Permute(list, start + 1, result);
    //         // backtrack
    //         (list[start], list[i]) = (list[i], list[start]); 
    //     }
    // }

    // private static bool IsPermutationIsomorphic(
    //     Dictionary<int, HashSet<int>> g1Adj,
    //     Dictionary<int, HashSet<int>> g2Adj,
    //     List<int> perm)
    // {
    //     int n = perm.Count;
    //     for (int i = 0; i < n; i++)
    //     {
    //         //find the neighbours of the same node mapped on g1 and g2
    //         var g1Neighbors = g1Adj[i];
    //         var g2Neighbors = g2Adj[perm[i]];

    //         //map the neighbours of g1 onto what they would be on g2
    //         var mappedG1Neighbors = System.Linq.Enumerable.ToHashSet(g1Neighbors.Select(neigh => perm[neigh]));

    //         if (!mappedG1Neighbors.SetEquals(g2Neighbors))
    //             return false;
    //     }
    //     return true;
    // }

    private void SetNodePos(int cardId)
    {
        currNodePos.Clear();
        for(int i = 0; i < graphData.nodes.Count(); i++)
        {
            int currNodePosition = graphData.nodePositions[cardId].arrangement[i];
            currNodePos.Add(getNodePos(currNodePosition, graphData.nodes.Count()));
        }
    }
    
    private Vector3 getNodePos(int counter, int n)
    {
        float angle = 2 * Mathf.PI * counter / n;
        float x = initRadius * Mathf.Sin(angle);
        x += levelParent.position.x;
        float y = initRadius * -Mathf.Cos(angle);
        y += levelParent.position.y;

        return new Vector3(x, y, 0);
    }
}
