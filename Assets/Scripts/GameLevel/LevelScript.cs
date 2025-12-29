using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine;
using Unity.VisualScripting;
using System;
using System.Data;

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
    public LevelState levelState;
    public float touchRadius=0.2f;
    public float clickRadius=0.05f;


    [Header("Prefabs")]
    public GameObject nodePrefab;
    public GameObject edgePrefab;

    [Header("Other")]
    public LevelUI UIManager;
    public List<Vector3> currNodePosMap;
    public List<NodeScript> currNodeScripts;
    public List<char> currNodeColorMap;
    public Dictionary<int, EdgeScript> currDrawnEdges = new Dictionary<int, EdgeScript>();
    public bool[] currUsedReducedIds;
    public EdgeScript drawingEdge=null;


    public int removedId;
    public bool gameWon;
    public bool gameAdmiring;
    public bool gamePaused;

    void Start()
    {
        if (GameManager.Instance)
        {
            levelIndex = GameManager.Instance.selectedLevelId;
            if (GameManager.Instance.selectedDailyLevel) graphData = Resources.Load<GraphData>($"Levels/Daily/Level{levelIndex}");
            else graphData = Resources.Load<GraphData>($"Levels/Normal/Level{levelIndex}");   
        }
        graphData.CheckGraphValidity();

        SaveManager.CurrentState = LoadLevelState();
        levelState = SaveManager.CurrentState;

        removedId = levelState.activeCardId;
        gameWon = levelState.solved;
        gameAdmiring = false;
        gamePaused = levelState.solved;
        currUsedReducedIds = new bool [graphData.nodes.Count];

        UIManager.DrawCardButtons();
        BuildCard(removedId);

        if(gameWon) Win();
    }

    private void BuildCard(int cardId)
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        currNodePosMap.Clear();
        currNodeColorMap.Clear();
        currNodeScripts.Clear();
        currDrawnEdges.Clear();
        Array.Clear(currUsedReducedIds, 0, currUsedReducedIds.Length);

        currNodePosMap = ReturnNodePosMap(cardId);
        ComputeWLColouring(cardId);
        for(int i = 0; i < graphData.nodes.Count(); i++)
        {
            currNodeScripts.Add(BuildNode(i));
        } 

        BuildEdges();
        ManageMove();
    }
    
    private NodeScript BuildNode(int id)
    {
        GameObject nodeObj = Instantiate(nodePrefab, transform);
        nodeObj.transform.localPosition = currNodePosMap[id];
        NodeScript nodeScript = nodeObj.GetComponent<NodeScript>();
        nodeScript.Initialize(id, this);
        return nodeScript;
    }

    private EdgeScript BuildEdge(int a, int? b)
    {
        GameObject edgeObj = Instantiate(edgePrefab, transform);
        EdgeScript edgeScript = edgeObj.GetComponent<EdgeScript>();
        if (b==null) edgeScript.Initialize(currNodeScripts[a].transform, null, this);
        else edgeScript.Initialize(currNodeScripts[a].transform, currNodeScripts[b.Value].transform, this);

        return edgeScript;
    }

    private void BuildEdges()
    {
        foreach(GraphData.Edge edge in graphData.edges)
        {
            if(edge.fromNodeId == removedId || edge.toNodeId == removedId) continue;
            BuildEdge(edge.fromNodeId, edge.toNodeId);
        }

        foreach(int targetNode in levelState.cardStates[removedId].drawnEdges)
        {
            currDrawnEdges[targetNode] = BuildEdge(removedId, targetNode);
        }
    }

    public void PenDown()
    {
        drawingEdge = BuildEdge(removedId, null);
    }

    public void PenUp(int? nodeId)
    {
        if(!nodeId.HasValue || currDrawnEdges.TryGetValue(nodeId.Value, out _))
        {
            if(drawingEdge != null) Destroy(drawingEdge.gameObject);
            drawingEdge = null;
            return;
        }
        
        drawingEdge.PointB = currNodeScripts[nodeId.Value].transform;
        currDrawnEdges[nodeId.Value] = drawingEdge;
        levelState.cardStates[removedId].drawnEdges.Add(nodeId.Value);
        drawingEdge = null;

        ManageMove();
    }

    public void EraseEdge(int nodeId)
    {
        if(!currDrawnEdges.TryGetValue(nodeId, out _)) return;
        EdgeScript edgeToErase = currDrawnEdges[nodeId];
        if(!edgeToErase) Debug.LogWarning("currDrawnEdges has a node (key) but edgescript == null (value)");

        Destroy(edgeToErase.gameObject);
        currDrawnEdges.Remove(nodeId);
        levelState.cardStates[removedId].drawnEdges.Remove(nodeId);

        ManageMove();
        return;
    }

    private void ManageMove()
    {
        for(int i=0;i<graphData.nodes.Count; i++)
        {
            if(i == removedId) continue;
            UIManager.SetCardCorrect(i, CheckSubgraph(i));
        }
        if(CheckGraph()) Win();
    }

    public void SetActiveCard(int cardId)
    {
        removedId = cardId;
        levelState.activeCardId = cardId;
        BuildCard(cardId);
        UIManager.UpdateCards();
    }

    
    public bool CheckGraph()
    {
        List<int> neighboursIds = graphData.nodes[removedId].neighbourIds;
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
                List<char> neighbourColors = graphData.nodes[i].neighbourIds
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
            if(i == removedId) continue;
            if(currUsedReducedIds[i]) continue;

            GraphClass drawngraph = new GraphClass(graphData.nodes.Count-1);
            ComputeGraph(drawngraph, i, true);

            int[] subToDrawnMapping = Enumerable.Repeat(-1, subgraph.Adj.Length).ToArray();
            int[] drawnToSubMapping = Enumerable.Repeat(-1, drawngraph.Adj.Length).ToArray();

            int[] subDescendingDegreeNodes = Enumerable.Range(0, subgraph.Adj.Length)
                .OrderByDescending(n => subgraph.Adj[n].Count)
                .ToArray();

            bool result = DFSMapping(0, drawngraph, subgraph, drawnToSubMapping, subToDrawnMapping, subDescendingDegreeNodes);

            if (result)
            {
                currUsedReducedIds[i] = true;
                return true;  
            }
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
            if(drawn && (edge.fromNodeId == removedId || edge.toNodeId == removedId)) continue;
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
            
            int a = fullToSubNodeIds[removedId];
            int b = fullToSubNodeIds[connectedId];
            graph.Adj[a].Add(b);
            graph.Adj[b].Add(a);
        }
    }

   
    public List<Vector3> ReturnNodePosMap(int cardId)
    {
        List<Vector3> nodePos = new List<Vector3>();
        foreach(int positionIndex in levelState.cardStates[cardId].scramble)
        {
            nodePos.Add(getCirclePos(positionIndex, graphData.nodes.Count()));
        }

        return nodePos;
    }
    
    private Vector3 getCirclePos(int counter, int n)
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
            if(i == removedId) continue;
            levelState.cardStates[i].drawnEdges.Clear();
        }

        foreach(GraphData.Edge edge in graphData.edges)
        {
            if (!levelState.cardStates[edge.fromNodeId].drawnEdges.Contains(edge.toNodeId) && edge.fromNodeId != removedId)
            {
                levelState.cardStates[edge.fromNodeId].drawnEdges.Add(edge.toNodeId);
            }
            if (!levelState.cardStates[edge.toNodeId].drawnEdges.Contains(edge.fromNodeId) && edge.toNodeId != removedId)
            {
                levelState.cardStates[edge.toNodeId].drawnEdges.Add(edge.fromNodeId);
            }
        }
        SetActiveCard(removedId);

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

    private void Update()
    {
        if(gamePaused) return;
        if(Pointer.current == null) return;

        if (PointerReleasedThisFrame() && drawingEdge != null)
        {
            foreach(var nodeScript in currNodeScripts)
            {
                if(nodeScript.id == removedId) continue;
                if(!nodeScript.PointerIsOver()) continue;

                PenUp(nodeScript.id);
                break;
            }
            if (drawingEdge != null) PenUp(null);
        }
    }

    private bool PointerReleasedThisFrame()
    {
        if (Pointer.current != null && Pointer.current.press.wasReleasedThisFrame) return true;
        return false;
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
