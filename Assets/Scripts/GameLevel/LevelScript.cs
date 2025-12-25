using System.Collections.Generic;
using System.Linq;
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

    [Header("Managers")]
    public List<Vector3> currNodePos;
    public List<NodeScript> nodeScripts;
    public LevelUI UIManager;
    public bool gamePaused;
    public int activeCardId;

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

        gamePaused = false;
        activeCardId =0;

        // UIManager.CreateCardButtons();
        BuildLevel();
    }

    public void BuildLevel()
    {
        SetNodePos();

        BuildNodes();
        BuildEdges();


    }

    private void BuildNodes()
    {
       for(int i = 0; i < graphData.nodes.Count(); i++)
        {
            GameObject nodeObj = Instantiate(nodePrefab, currNodePos[i], Quaternion.identity, transform);
            NodeScript nodeScript = nodeObj.GetComponent<NodeScript>();
            nodeScripts.Add(nodeScript);
            nodeScript.Initialize(i, this);
        } 
    }

    private void BuildEdges()
    {
        foreach(GraphData.Edge edge in graphData.edges)
        {
            if(edge.fromNodeId == activeCardId || edge.toNodeId == activeCardId) continue;
            GameObject edgeObj = Instantiate(edgePrefab, transform);
            EdgeScript edgeScript = edgeObj.GetComponent<EdgeScript>();
            edgeScript.Initialize(nodeScripts[edge.fromNodeId].transform, nodeScripts[edge.toNodeId].transform, this);
        }
    }

    public void Restart()
    {
        gamePaused = false;
        UIManager.hasShownWin = false;
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

    private void SetNodePos()
    {
        currNodePos.Clear();
        for(int i = 0; i < graphData.nodes.Count(); i++)
        {
            int currNodePosition = graphData.nodePositions[activeCardId].arrangement[i];
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
