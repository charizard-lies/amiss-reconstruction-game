using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine;
using System;
using System.Data;
using UnityEditor;

public class LevelScript : MonoBehaviour
{
    public static LevelScript Instance;

    [Header("Attributes")]
    public GraphData graphData;
    public float initRadius;
    public float nodeAttractionTime=0.1f;
    public Color edgeColor = Color.white;
    public float edgeWidth = 0.2f;
    public string levelIndex;
    public LevelState levelState;
    public float touchRadius=0.2f;
    public float clickRadius=0.05f;
    public bool tutorial = false;
    public float closeSnapSqrDist = 0.0001f;
    public float snapRadius;


    [Header("Prefabs")]
    public GameObject nodePrefab;
    public GameObject edgePrefab;

    [Header("Other")]
    public LevelUI UIManager;
    public List<Vector3> currNodeLocalPosMap;
    public Dictionary<int, NodeScript> currNodeScripts = new Dictionary<int, NodeScript>();
    public bool[] usedReduceIds;
    public List<char> currNodeColorMap;
    public Dictionary<int, EdgeScript> currDrawnEdges = new Dictionary<int, EdgeScript>();


    public int currRemovedId;
    public bool gameWon;
    public bool gameAdmiring;
    public bool gamePaused;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        if (GameManager.Instance)
        {
            levelIndex = GameManager.Instance.selectedLevelId;
            if (GameManager.Instance.selectedDailyLevel) graphData = Resources.Load<GraphData>($"Levels/Daily/Level{levelIndex}");
            else if (GameManager.Instance.selectedTutorialLevel)
            {
                graphData = Resources.Load<GraphData>($"Levels/Tutorial/Level{levelIndex}");
                tutorial = true;
            }
            else graphData = Resources.Load<GraphData>($"Levels/Normal/Level{levelIndex}");   
        }

        graphData.CheckGraphValidity();

        if (tutorial) SaveManager.Delete(levelIndex);

        SaveManager.CurrentState = LoadLevelState();
        levelState = SaveManager.CurrentState;

        gameWon = levelState.solved;
        gameAdmiring = false;
        gamePaused = levelState.solved;

        UIManager.DrawCardButtons();
        SetActiveCard(levelState.activeCardId);

        ToolManager.Instance.SetTool(SwapTool.Instance);

        if(gameWon) Win();
    }

    private void BuildCard(int cardId)
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        currNodeLocalPosMap.Clear();
        currNodeColorMap.Clear();
        currNodeScripts.Clear();
        currDrawnEdges.Clear();

        currNodeLocalPosMap = ReturnNodeLocalPosMap(cardId);
        ComputeWLColouring(cardId);

        for(int i = 0; i < graphData.nodes.Count(); i++)
        {
            NodeScript nodeAdded = BuildNode(i);
            currNodeScripts[i] = nodeAdded;
            ToolManager.Instance.Register(nodeAdded);
        } 

        BuildEdges();
    }
    
    private NodeScript BuildNode(int id)
    {
        GameObject nodeObj = Instantiate(nodePrefab, transform);
        nodeObj.transform.localPosition = currNodeLocalPosMap[id];
        NodeScript nodeScript = nodeObj.GetComponent<NodeScript>();
        nodeScript.Initialize(id);
        return nodeScript;
    }

    private EdgeScript BuildEdge(int a, int? b)
    {
        GameObject edgeObj = Instantiate(edgePrefab, transform);
        EdgeScript edgeScript = edgeObj.GetComponent<EdgeScript>();
        if (b==null) edgeScript.Initialize(currNodeScripts[a].transform, null);
        else edgeScript.Initialize(currNodeScripts[a].transform, currNodeScripts[b.Value].transform);

        return edgeScript;
    }

    private void BuildEdges()
    {
        foreach(GraphData.Edge edge in graphData.edges)
        {
            if(edge.fromNodeId == currRemovedId || edge.toNodeId == currRemovedId) continue;
            BuildEdge(edge.fromNodeId, edge.toNodeId);
        }

        foreach(int targetNode in levelState.cardStates[currRemovedId].drawnEdges)
        {
            currDrawnEdges[targetNode] = BuildEdge(currRemovedId, targetNode);
        }
    }


    private void ManageMove()
    {
        CheckAllSubgraphs();
        if(CheckGraph()) Win();
    }

    public EdgeScript PenDown()
    {
        return BuildEdge(currRemovedId, null);
    }

    public void PenUp(int? nodeId, EdgeScript drawingEdge)
    {
        if(!nodeId.HasValue || currDrawnEdges.TryGetValue(nodeId.Value, out _) || nodeId.Value == currRemovedId)
        {
            if(drawingEdge != null) Destroy(drawingEdge.gameObject);
            return;
        }
        
        drawingEdge.PointB = currNodeScripts[nodeId.Value].transform;
        currDrawnEdges[nodeId.Value] = drawingEdge;
        levelState.cardStates[currRemovedId].drawnEdges.Add(nodeId.Value);

        ManageMove();
    }

    public void EraseEdge(int nodeId)
    {
        if(!currDrawnEdges.TryGetValue(nodeId, out _)) return;
        EdgeScript edgeToErase = currDrawnEdges[nodeId];
        if(!edgeToErase) Debug.LogWarning("currDrawnEdges has a node (key) but edgescript == null (value)");

        Destroy(edgeToErase.gameObject);
        currDrawnEdges.Remove(nodeId);
        levelState.cardStates[currRemovedId].drawnEdges.Remove(nodeId);

        ManageMove();
        return;
    }

    public void CheckAllSubgraphs()
    {
        usedReduceIds = Enumerable.Repeat(false, graphData.nodes.Count).ToArray();
        for(int i=0;i<graphData.nodes.Count; i++)
        {
            if(i == currRemovedId) continue;
            UIManager.SetCardCorrect(i, CheckSubgraph(i));
        }
    }

    public void SetActiveCard(int cardId)
    {
        currRemovedId = cardId;
        levelState.activeCardId = cardId;

        BuildCard(cardId);
        UIManager.UpdateCards();
        CheckAllSubgraphs();
    }

    public void SwapNodes(int a, int b)
    {
        NodeScript nodeA = currNodeScripts[a];
        NodeScript nodeB = currNodeScripts[b];

        int positionIndexA = levelState.cardStates[currRemovedId].scramble[a];
        int positionIndexB = levelState.cardStates[currRemovedId].scramble[b];
        
        nodeA.SetTargetLocalPos(currNodeLocalPosMap[b]);
        nodeB.SetTargetLocalPos(currNodeLocalPosMap[a]);
        
        levelState.cardStates[currRemovedId].scramble[a] = positionIndexB;
        levelState.cardStates[currRemovedId].scramble[b] = positionIndexA;

        currNodeLocalPosMap = ReturnNodeLocalPosMap(currRemovedId);
        UIManager.UpdateCards();
    }
    
    public bool CheckGraph()
    {
        List<int> neighboursIds = graphData.nodes[currRemovedId].neighbourIds;
        List<char> neighbourLabels = neighboursIds.Select(id => currNodeColorMap[id]).ToList();

        List<char> drawnNeighbourLabels = new List<char>();
        foreach(EdgeScript edgeScript in currDrawnEdges.Values)
        {
            NodeScript connectedNode = edgeScript.PointB.gameObject.GetComponent<NodeScript>();
            drawnNeighbourLabels.Add(currNodeColorMap[connectedNode.id]);
        }

        return neighbourLabels.OrderBy(x=>x).SequenceEqual(drawnNeighbourLabels.OrderBy(x=>x));
    }

    private void ComputeWLColouring(int cardId)
    {
        currNodeColorMap.Clear();

        List<char> currColors = new List<char>();
        foreach(var node in graphData.nodes.Values)
        {
            if(node.id == cardId) currColors.Add('a');
            else currColors.Add(IntToLetter(node.neighbourIds.Count(neighbourId => neighbourId!=cardId)));
        }
        
        int oldSignatureCount=0;
        for(int i = 0; i < graphData.nodes.Count(); i++)
        {
            Dictionary<int,string> signatures = new Dictionary<int, string>();

            for(int j = 0; j < graphData.nodes.Count(); j++)
            {
                List<char> neighbourColors = graphData.nodes[j].neighbourIds
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
                if(j == currRemovedId) continue;
                currColors[j] = IntToLetter(uniqueSignatures.IndexOf(signatures[j]));
            }
        }

        currNodeColorMap = currColors;
    }

    private char IntToLetter(int num)
    {
        return (char)('a' + num);
    }

    public class GraphClass
    {
        public HashSet<int>[] Adj;

        public GraphClass(int n)
        {
            Adj = new HashSet<int>[n];
            for(int i = 0; i < n; i++)
            {
                Adj[i] = new HashSet<int>();
            }
        }

        public void PrintGraph()
        {
            int counter = 0;
            foreach(var list in Adj)
            {
                Debug.Log($"{counter++}: {string.Join(", ", list)}");
            }
        }
    }

    public bool CheckSubgraph(int cardId)
    {
        GraphClass subgraph = new GraphClass(graphData.nodes.Count-1);
        ComputeGraph(subgraph, cardId, false);

        for(int i = 0; i < graphData.nodes.Count; i++)
        {
            if(usedReduceIds[i]) continue;
            if(i == currRemovedId) continue;

            GraphClass drawngraph = new GraphClass(graphData.nodes.Count-1);
            ComputeGraph(drawngraph, i, true);

            int[] subToDrawnMapping = Enumerable.Repeat(-1, subgraph.Adj.Length).ToArray();
            int[] drawnToSubMapping = Enumerable.Repeat(-1, drawngraph.Adj.Length).ToArray();

            int[] subDescendingDegreeNodes = Enumerable.Range(0, subgraph.Adj.Length)
                .OrderByDescending(n => subgraph.Adj[n].Count)
                .ToArray();

            bool result = DFSMapping(0, drawngraph, subgraph, drawnToSubMapping, subToDrawnMapping, subDescendingDegreeNodes);

            if (!result) continue;
            
            usedReduceIds[i] = true;
            return true;  
        }
        return false;
    }
    
    private bool DFSMapping(int index, GraphClass grapha, GraphClass graphb, int[]aTobMapping, int[] bToaMapping, int[] sortedbIndices)
    {
        if (index == graphb.Adj.Length) return true;

        int bId = sortedbIndices[index];

        for(int j = 0; j < grapha.Adj.Length; j++)
        {
            if (aTobMapping[j] != -1) continue;
            if(!MappingIsCompatible(j, bId, grapha, graphb, aTobMapping, bToaMapping)) continue;

            bToaMapping[bId] = j;
            aTobMapping[j] = bId;
            if(DFSMapping(index+1, grapha, graphb, aTobMapping, bToaMapping, sortedbIndices)) return true;
            bToaMapping[bId] = -1;
            aTobMapping[j] = -1;
        }

        return false;
    }

    private bool MappingIsCompatible(int aId, int bId, GraphClass agraph, GraphClass bgraph, int[]aTobMapping, int[] bToaMapping)
    {
        if(agraph.Adj[aId].Count < bgraph.Adj[bId].Count) return false;

        foreach(int neighbour in bgraph.Adj[bId])
        {
            if(bToaMapping[neighbour] == -1) continue;
            if(!agraph.Adj[aId].Contains(bToaMapping[neighbour])) return false; 
        }

        foreach(int neighbour in agraph.Adj[aId])
        {
            if(aTobMapping[neighbour] == -1) continue;
            if(!bgraph.Adj[bId].Contains(aTobMapping[neighbour])) return false;
        }

        return true;
    }

    private void ComputeGraph(GraphClass graph, int reducedId, bool drawn)
    {
        Dictionary<int, int> fullToSubNodeIds = new();
        int index = 0;
        foreach(GraphData.Node node in graphData.nodes.Values)
        {
            if(node.id == reducedId) continue;
            fullToSubNodeIds[node.id] = index++;
        }

        foreach(GraphData.Edge edge in graphData.edges)
        {
            if(drawn && (edge.fromNodeId == currRemovedId || edge.toNodeId == currRemovedId)) continue;
            if(edge.fromNodeId == reducedId || edge.toNodeId == reducedId) continue;
                
            int a = fullToSubNodeIds[edge.fromNodeId];
            int b = fullToSubNodeIds[edge.toNodeId];
            graph.Adj[a].Add(b);
            graph.Adj[b].Add(a);
        }

        if (!drawn) return;
        
        foreach(int connectedId in currDrawnEdges.Keys)
        {
            if(connectedId == reducedId) continue;
            
            int a = fullToSubNodeIds[currRemovedId];
            int b = fullToSubNodeIds[connectedId];
            graph.Adj[a].Add(b);
            graph.Adj[b].Add(a);
        }
    }

    public List<Vector3> ReturnNodeLocalPosMap(int cardId)
    {
        List<Vector3> nodePos = new List<Vector3>();
        foreach(int positionIndex in levelState.cardStates[cardId].scramble)
        {
            nodePos.Add(getLocalCirclePos(positionIndex, graphData.nodes.Count()));
        }

        return nodePos;
    }
    
    private Vector3 getLocalCirclePos(int counter, int n)
    {
        float angle = 2 * Mathf.PI * counter / n;
        float x = initRadius * Mathf.Sin(angle);
        float y = initRadius * -Mathf.Cos(angle);

        return new Vector3(x, y, 0);
    }


    public void Win()
    {
        UIManager.ShowWinMenu();

        gameWon = true;
        gameAdmiring = false;
        gamePaused = true;

        for(int i = 0; i < graphData.nodes.Count(); i++)
        {
            if(i == currRemovedId) continue;
            levelState.cardStates[i].drawnEdges.Clear();
        }

        foreach(GraphData.Edge edge in graphData.edges)
        {
            if (!levelState.cardStates[edge.fromNodeId].drawnEdges.Contains(edge.toNodeId) && edge.fromNodeId != currRemovedId)
            {
                levelState.cardStates[edge.fromNodeId].drawnEdges.Add(edge.toNodeId);
            }
            if (!levelState.cardStates[edge.toNodeId].drawnEdges.Contains(edge.fromNodeId) && edge.toNodeId != currRemovedId)
            {
                levelState.cardStates[edge.toNodeId].drawnEdges.Add(edge.fromNodeId);
            }
        }
        SetActiveCard(currRemovedId);

        for(int i=0;i<graphData.nodes.Count; i++)
        {
            UIManager.SetCardCorrect(i, true);
        }

        levelState.solved = true;
        SaveManager.Save(levelIndex);
    }

    public void Admire()
    {
        UIManager.AdmirePuzzle();

        gameWon = true;
        gameAdmiring = true;
        gamePaused = true;
    }

    public void Pause()
    {
        UIManager.Pause();

        gameAdmiring = false;
        gamePaused = true;

        SaveManager.Save(levelIndex);
    }

    public void Resume()
    {
        UIManager.Resume();

        gameWon = false;
        gameAdmiring = false;
        gamePaused = false;
    }

    public void TryRestart()
    {
        UIManager.TryRestart();

        gamePaused = true;
    }

    public void CancelRestart()
    {
        UIManager.CancelRestart();

        gamePaused = gameWon;
    }

    public void Restart()
    {
        UIManager.Restart();

        gamePaused = false;
        gameWon = false;
        gameAdmiring = false;

        foreach(CardState cardState in levelState.cardStates) cardState.drawnEdges.Clear();
        levelState.solved = false;
        SaveManager.Save(levelIndex);

        SetActiveCard(0);
    }

    public void Quit()
    {
        if (GameManager.Instance.selectedDailyLevel) GameManager.Instance.LoadMainMenu();
        else GameManager.Instance.LoadLevelMenu();
    }


    private LevelState LoadLevelState()
    {
        LevelState loaded = SaveManager.Load(levelIndex);
        if (loaded != null) return loaded;

        return CreateFreshLevelState();
    }

    private LevelState CreateFreshLevelState()
    {
        LevelState state = new LevelState();
        state.levelIndex = levelIndex;
        state.activeCardId = 0;
        state.solved = false;

        List<GraphData.CardArrangement> graphArrangement = graphData.GenerateRandomCardArrangements();

        for(int i = 0; i < graphData.nodes.Count(); i++)
        {
            state.cardStates.Add(new CardState());
            state.cardStates[i].id = i;
            state.cardStates[i].scramble = graphArrangement[i].scramble;
        }

        return state;
    }
}
