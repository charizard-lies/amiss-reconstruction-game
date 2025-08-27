using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;

//I will be the logic behind managing various different levels in the game
public class LevelScript : MonoBehaviour
{
    public GameObject deckPrefab;
    public GraphData graphData;
    public Transform levelParent;
    public GraphToggleUI UIManager;
    public GameObject anchorPrefab;
    public float initRadius;
    public List<AnchorScript> allAnchors = new List<AnchorScript>();
    public GraphData submitGraph;
    public int activeNodeLayer; //ActiveNode
    public int inactiveNodeLayer; //0

    private DeckScript deckScript;

    void Start()
    {

        for (int i = 0; i < graphData.nodeIds.Count; i++)
        {
            AnchorScript temp = Instantiate(anchorPrefab, getAnchorPos(i, graphData.nodeIds.Count), Quaternion.identity, levelParent).GetComponent<AnchorScript>();
            temp.id = i;
            allAnchors.Add(temp);
        }

        deckScript = Instantiate(deckPrefab, levelParent).GetComponent<DeckScript>();
        deckScript.Initialize(graphData, activeNodeLayer, inactiveNodeLayer, this);
        deckScript.BuildDeck();
        UIManager.deckManager = deckScript;
        UIManager.InitButtons(graphData);

    }

    //check whether the visible graphs overlay to form the correct graph
    public void checkGraph()
    {
        List<CardScript> cards = deckScript.visibleCards;
        submitGraph = ScriptableObject.CreateInstance<GraphData>();

        //add anchors as nodes
        foreach (AnchorScript anchor in allAnchors)
        {
            submitGraph.addNode(anchor.id);
        }

        Debug.Log(cards.Count);
        //go through every card in a deck
        for (int i = 0; i < cards.Count; i++)
        {
            //go through every node in a card
            CardScript card = cards[i];
            foreach (KeyValuePair<int, NodeScript> pair in card.nodeMap)
            {
                int nodeID = pair.Key;
                NodeScript node = pair.Value;

                //check all nodes are anchored
                if (!node.snappedAnchor)
                {
                    Debug.Log("false");
                    UIManager.UpdateSolved(false);
                }
            }

            //go through every edge in a card
            Debug.Log($"Card {card.removedId} has {(card.allEdges == null ? "null" : card.allEdges.Count.ToString())} edges");
            foreach (EdgeScript edge in card.allEdges)
            {
                Debug.Log($"added edge {edge.NodeA.snappedAnchor.id} to {edge.NodeB.snappedAnchor.id}");
                submitGraph.AddEdge(edge.NodeA.snappedAnchor.id, edge.NodeB.snappedAnchor.id);
            }
        }

        bool isSolved = checkIsomorphism(graphData, submitGraph);
        Debug.Log(isSolved);
        UIManager.UpdateSolved(isSolved);
    }

    public bool checkIsomorphism(GraphData g1, GraphData g2)
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
        float y = initRadius * Mathf.Sin(angle);
        Vector3 anchorPos = new Vector3(x, y, 0);
        return anchorPos;
    }
}
