using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
    [Header("Attributes")]
    public GraphData graphData;
    public float initRadius;
    public float nodeAttractionTime=0.1f;
    public float activeEdgeWidth;
    public float overlayEdgeWidth = 0.1f;
    public float overlayEdgeAlpha = 0.1f;
    public Color edgeColor = Color.white;
    public Transform levelParent;

    public string levelIndex;
    public bool daily;
    public LevelState levelState;


    [Header("Prefabs")]
    public GameObject deckPrefab;
    public GameObject anchorPrefab;
    public GameObject cardPrefab;
    public GameObject nodePrefab;
    public GameObject edgePrefab;

    [Header("Managers")]
    public LevelUI UIManager;
    public DeckScript deck;
    public bool gamePaused;
    private System.Random rng = new System.Random();
    public Dictionary<int, AnchorScript> anchorMap = new Dictionary<int, AnchorScript>();
    public List<AnchorScript> allAnchors = new List<AnchorScript>();

    void Start()
    {
        if (GameManager.Instance)
        {
            levelIndex = GameManager.Instance.selectedLevelId;
            daily = GameManager.Instance.selectedDailyLevel;
        }
        else
        {
            levelIndex = "2";
            daily = false;
        }

        if (daily) graphData = Resources.Load<GraphData>($"Levels/Daily/Level{levelIndex}");
        else graphData = Resources.Load<GraphData>($"Levels/Normal/Level{levelIndex}");

        BuildAnchors();
        SaveManager.CurrentState = LoadLevelState();
        levelState = SaveManager.CurrentState;

        deck = Instantiate(deckPrefab, levelParent).GetComponent<DeckScript>();
        deck.Initialize(this);
        deck.Build();
        UIManager.CreateCardButtons();

        gamePaused = false;
    }

    public void Restart()
    {
        deck.ResetDeck();
        gamePaused = false;
        UIManager.hasShownWin = false;
    }

    private LevelState LoadLevelState()
    {
        LevelState loaded = SaveManager.Load(levelIndex);
        if (loaded != null) return loaded;

        return CreateFreshLevelState();
    }

    private List<T> Shuffle<T>(List<T> list)
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

        return list;
    }

    private List<int> ShuffledAnchorIds(int removedId)
    {
        List<int> shuffledAnchors = allAnchors
        .Select(anchor => anchor.id)
        .ToList();

        Shuffle(shuffledAnchors);
        shuffledAnchors.RemoveAt(shuffledAnchors.Count - 1);

        return shuffledAnchors;
    }

    private LevelState CreateFreshLevelState()
    {
        LevelState state = new LevelState();
        state.levelIndex = levelIndex;
        state.activeCardId = graphData.nodeIds[0];
        state.solved = false;

        foreach (int nodeId in graphData.nodeIds)
        {
            state.idToCardStatesMap[nodeId] = CreateFreshCardState(nodeId);
        }
        state.EnsureList();

        return state;
    }
    
    private CardState CreateFreshCardState(int removedId)
    {
        CardState state = new CardState();
        GraphData reducedGraphData = graphData.GraphReduce(removedId);
        List<int> shuffledAnchorIds = ShuffledAnchorIds(removedId);

        state.isVisible = true;
        state.nodeAnchorIdMap = reducedGraphData.nodeIds
        .Select((nodeId, i) => new { nodeId, value = shuffledAnchorIds[i] })
        .ToDictionary(x => x.nodeId, x => x.value);
        
        
        foreach(int nodeId in graphData.nodeIds)
        {
            if (nodeId == removedId) continue;

            NodeState nodeState = new NodeState();
            nodeState.nodeId = nodeId;
            nodeState.snapped = true;
            nodeState.snappedAnchorId = state.nodeAnchorIdMap[nodeId];
            nodeState.pos = anchorMap[state.nodeAnchorIdMap[nodeId]].transform.position;

            state.nodes.Add(nodeState);
        }

        return state;
    }

    private void BuildAnchors()
    {
        for (int i = 0; i < graphData.nodeIds.Count; i++)
        {
            AnchorScript temp = Instantiate(anchorPrefab, getAnchorPos(i, graphData.nodeIds.Count), Quaternion.identity, levelParent).GetComponent<AnchorScript>();
            temp.id = i;
            allAnchors.Add(temp);
            anchorMap[i] = temp;
        }
    }

    public GraphData BuildOverlayGraph(List<CardScript> cards)
    {
        GraphData overlayGraph = ScriptableObject.CreateInstance<GraphData>();

        foreach (AnchorScript anchor in allAnchors)
        {
            overlayGraph.addNode(anchor.id);
        }

        for (int i = 0; i < cards.Count; i++)
        {
            CardScript card = cards[i];
            foreach (EdgeScript edge in card.allEdges)
            {
                if (edge.PointA.GetComponent<NodeScript>().snappedAnchor && edge.PointB.GetComponent<NodeScript>().snappedAnchor)
                {
                    overlayGraph.AddEdge(edge.PointA.GetComponent<NodeScript>().snappedAnchor.id, edge.PointB.GetComponent<NodeScript>().snappedAnchor.id);
                }
            }
        }

        return overlayGraph;
    }

    public bool CheckGraphSolved()
    {
        if (!CheckUniqueSnappedAnchors()) return false;
        if (!CheckAllCardsVisible()) return false;

        GraphData overlayGraph = BuildOverlayGraph(deck.allCards.Where(card => card.isVisible).ToList());
        bool overlayCorrect = CheckIsomorphism(graphData, overlayGraph);

        return overlayCorrect;
    }

    private bool CheckAllCardsVisible()
    {
        foreach(CardScript card in deck.allCards)
        {
            if (!card.isVisible) return false;
        }
        return true;
    }

    private bool CheckUniqueSnappedAnchors()
    {
        List<AnchorScript> anchors = allAnchors;
        List<AnchorScript> uniqueAnchors = new List<AnchorScript>();

        foreach (CardScript card in deck.allCards)
        {
            if (!card.isVisible) continue;
            
            List<AnchorScript> tempAnchors = new List<AnchorScript>();
            foreach (NodeScript node in card.nodeMap.Values)
            {
                if (!node.snappedAnchor) return false;
                tempAnchors.Add(node.snappedAnchor);
            }
            uniqueAnchors.Add(anchors.Except(tempAnchors).Single());
        }

        return uniqueAnchors.Count() == uniqueAnchors.Distinct().Count();
    }

    public bool CheckIsomorphism(GraphData g1, GraphData g2)
    {
        int n = g1.nodeIds.Count;
        if (n != g2.nodeIds.Count || g1.edges.Count != g2.edges.Count)
            return false;

        // Normalize both graphs into adjacency sets
        var g1Adj = BuildAdjacencyList(g1);
        var g2Adj = BuildAdjacencyList(g2);

        // Generate all permutations of node indices [0...n-1]
        var permutations = GetPermutations(Enumerable.Range(0, n).ToList());

        foreach (var perm in permutations)
        {
            if (IsPermutationIsomorphic(g1Adj, g2Adj, perm))
                return true;
        }

        return false;
    }

    private static Dictionary<int, HashSet<int>> BuildAdjacencyList(GraphData g)
    {
        var map = new Dictionary<int, int>();
        var sortedNodes = g.nodeIds.OrderBy(id => id).ToList();

        //map nodeid to index (ids may not be consecutive)
        for (int i = 0; i < sortedNodes.Count; i++)
            map[sortedNodes[i]] = i;

        //construct adjacency list
        var adj = new Dictionary<int, HashSet<int>>();
        for (int i = 0; i < sortedNodes.Count; i++)
            adj[i] = new HashSet<int>();

        foreach (var edge in g.edges)
        {
            int a = map[edge.fromNodeId];
            int b = map[edge.toNodeId];
            adj[a].Add(b);
            adj[b].Add(a); // undirected
        }

        return adj;
    }
    private static List<List<int>> GetPermutations(List<int> list)
    {
        var result = new List<List<int>>();
        Permute(list, 0, result);
        return result;
    }

    private static void Permute(List<int> list, int start, List<List<int>> result)
    {
        if (start >= list.Count)
        {
            result.Add(new List<int>(list));
            return;
        }

        for (int i = start; i < list.Count; i++)
        {
            //place every other number into the start position, including itself
            (list[start], list[i]) = (list[i], list[start]);
            //permute the rest of the list, keeping everything at and before start the same.
            Permute(list, start + 1, result);
            // backtrack
            (list[start], list[i]) = (list[i], list[start]); 
        }
    }

    private static bool IsPermutationIsomorphic(
        Dictionary<int, HashSet<int>> g1Adj,
        Dictionary<int, HashSet<int>> g2Adj,
        List<int> perm)
    {
        int n = perm.Count;
        for (int i = 0; i < n; i++)
        {
            //find the neighbours of the same node mapped on g1 and g2
            var g1Neighbors = g1Adj[i];
            var g2Neighbors = g2Adj[perm[i]];

            //map the neighbours of g1 onto what they would be on g2
            var mappedG1Neighbors = System.Linq.Enumerable.ToHashSet(g1Neighbors.Select(neigh => perm[neigh]));

            if (!mappedG1Neighbors.SetEquals(g2Neighbors))
                return false;
        }
        return true;
    }

    private Vector3 getAnchorPos(int counter, int n)
    {
        float angle = 2 * Mathf.PI * counter / n;
        float x = initRadius * Mathf.Cos(angle);
        x += levelParent.position.x;
        float y = initRadius * Mathf.Sin(angle);
        y += levelParent.position.y;

        return new Vector3(x, y, 0);
    }
}
