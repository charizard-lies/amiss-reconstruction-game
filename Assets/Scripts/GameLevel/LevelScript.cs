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
    public List<Vector3> currNodePosMap;
    public List<NodeScript> currNodeScripts;
    public List<char> currNodeColorMap;
    public List<Dictionary<int, EdgeScript>> drawnEdges = new List<Dictionary<int, EdgeScript>>();
    public EdgeScript drawingEdge=null;


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
        graphData.GenerateRandomCardArrangements();

        for(int i = 0; i < graphData.nodes.Count(); i++) drawnEdges.Add(new Dictionary<int, EdgeScript>());

        gamePaused = false;

        UIManager.DrawCardButtons();
        BuildCard(removedId);
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

        currNodePosMap = ReturnNodePosMap(cardId);
        ComputeWLColouring(cardId);
        for(int i = 0; i < graphData.nodes.Count(); i++)
        {
            currNodeScripts.Add(BuildNode(i));
        } 

        BuildEdges();


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
    }

    public void PenDown()
    {
        drawingEdge = BuildEdge(removedId, null);
    }

    public void PenUp(int? nodeId)
    {
        if(nodeId == null || drawnEdges[removedId].TryGetValue(nodeId.Value, out _))
        {
            Destroy(drawingEdge.gameObject);
            drawingEdge = null;
            return;
        }
        
        drawingEdge.PointB = currNodeScripts[nodeId.Value].transform;
        drawnEdges[removedId][nodeId.Value] = drawingEdge;
        drawingEdge = null;

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

    public void SetActiveCard(int cardId)
    {
        removedId = cardId;
        BuildCard(cardId);
        UIManager.DrawCardButtons();
    }

    public void Restart()
    {
        gamePaused = false;
        UIManager.hasShownWin = false;
    }

    public bool CheckGraph()
    {
        List<int> neighboursIds = graphData.nodes[removedId].adjacentNodeIds;
        List<char> neighbourLabels = neighboursIds.Select(id => currNodeColorMap[id]).ToList();

        List<char> drawnNeighbourLabels = new List<char>();
        foreach(EdgeScript edgeScript in drawnEdges[removedId].Values)
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

        currNodeColorMap = currColors;
    }

    private char IntToLetter(int num)
    {
        return (char)('a' + num);
    }

    public List<Vector3> ReturnNodePosMap(int cardId)
    {
        List<Vector3> nodePos = new List<Vector3>();
        for(int i = 0; i < graphData.nodes.Count(); i++)
        {
            int positionIndex = graphData.cardArrangements[cardId].scramble[i];
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

    private void Update()
    {
        if(gamePaused) return;

        if (Mouse.current.leftButton.wasReleasedThisFrame && drawingEdge != null)
        {
            foreach(var nodeScript in currNodeScripts)
            {
                if(nodeScript.id == removedId) continue;
                if(!nodeScript.MouseIsOver()) continue;

                PenUp(nodeScript.id);
            }
            if (drawingEdge) PenUp(null);
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
}
