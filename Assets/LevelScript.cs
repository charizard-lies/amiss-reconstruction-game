using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;

//I will be the logic behind managing various different levels in the game
public class LevelScript : MonoBehaviour
{
    [Header("Attributes")]
    public int levelIndex;
    public float initRadius;
    public GraphData graphData;
    public int activeNodeLayer = 6; //ActiveNode
    public int inactiveNodeLayer = 0; //0
    public float activeEdgeWidth;
    public float overlayEdgeWidth = 0.1f;
    public float overlayEdgeAlpha = 0.1f;
    public Transform levelParent;
    public Dictionary<int, AnchorScript> anchorMap = new Dictionary<int, AnchorScript>();
    public List<AnchorScript> allAnchors = new List<AnchorScript>();

    [Header("Prefabs")]
    public GameObject deckPrefab;
    public GameObject anchorPrefab;
    public GameObject cardPrefab;
    public GameObject nodePrefab;
    public GameObject edgePrefab;

    [Header("Managers")]
    public GraphToggleUI UIManager;
    private DeckScript deckScript;

    void Start()
    {
        //levelIndex = GameManager.Instance.selectedLevelId;
        levelIndex = 1;
        graphData = Resources.Load<GraphData>($"Levels/Level{levelIndex}");

        //anchors
        for (int i = 0; i < graphData.nodeIds.Count; i++)
        {
            AnchorScript temp = Instantiate(anchorPrefab, getAnchorPos(i, graphData.nodeIds.Count), Quaternion.identity, levelParent).GetComponent<AnchorScript>();
            temp.id = i;
            allAnchors.Add(temp);
            anchorMap[i] = temp;
        }

        //game deck
        deckScript = Instantiate(deckPrefab, levelParent).GetComponent<DeckScript>();
        deckScript.Initialize(this);
        deckScript.BuildDeck();

        //create ui
        UIManager.deckManager = deckScript;
        UIManager.InitButtons(graphData);
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

    public bool GraphisSolved()
    {
        bool isSolved = false;

        bool allNodesSnapped = false;
        foreach(CardScript card in deckScript.visibleCards)
        {
            
        }
        
        if (!allNodesSnapped)
        {
            return false;
        }

        bool uniqueAnchorsOverLayers = false;
        if (!uniqueAnchorsOverLayers)
        {
            return false;
        }

        GraphData overlayGraph = BuildOverlayGraph(deckScript.visibleCards);
        bool overlayCorrect = CheckIsomorphism(graphData, overlayGraph);
        Debug.Log("Overlay: " + overlayCorrect);

        return isSolved;
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

    //turns any graph into an isomorphic adjacency list with nodes 0...n
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

    //get list of all permutations (every way to order n numbers in list)
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
            var mappedG1Neighbors = g1Neighbors.Select(neigh => perm[neigh]).ToHashSet();

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
