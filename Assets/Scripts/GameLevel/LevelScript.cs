using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine;
using Unity.VisualScripting;

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


    [Header("Prefabs")]
    public GameObject nodePrefab;
    public GameObject edgePrefab;

    [Header("Other")]
    public LevelUI UIManager;
    public List<Vector3> currNodePosMap;
    public List<NodeScript> currNodeScripts;
    public List<char> currNodeColorMap;
    public Dictionary<int, EdgeScript> currDrawnEdges = new Dictionary<int, EdgeScript>();
    public EdgeScript drawingEdge=null;


    public int removedId;
    public bool gameWon;
    public bool gameAdmiring;
    public bool gamePaused;

    void Start()
    {
        // if (GameManager.Instance)
        // {
        //     levelIndex = GameManager.Instance.selectedLevelId;
        //     if (GameManager.Instance.selectedDailyLevel) graphData = Resources.Load<GraphData>($"Levels/Daily/Level{levelIndex}");
        //     else graphData = Resources.Load<GraphData>($"Levels/Normal/Level{levelIndex}");   
        // }

        levelIndex = "1";
        graphData = Resources.Load<GraphData>($"Levels/Normal/Level1");

        SaveManager.CurrentState = LoadLevelState();
        levelState = SaveManager.CurrentState;

        removedId = levelState.activeCardId;
        gameWon = levelState.solved;
        gameAdmiring = false;
        gamePaused = levelState.solved;

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
        if(nodeId == null || currDrawnEdges.TryGetValue(nodeId.Value, out _))
        {
            Destroy(drawingEdge.gameObject);
            drawingEdge = null;
            return;
        }
        
        drawingEdge.PointB = currNodeScripts[nodeId.Value].transform;
        currDrawnEdges[nodeId.Value] = drawingEdge;
        levelState.cardStates[removedId].drawnEdges.Add(nodeId.Value);
        drawingEdge = null;

        if(CheckGraph()) Win();
    }

    public void EraseEdge(int nodeId)
    {
        if(!currDrawnEdges.TryGetValue(nodeId, out _)) return;
        EdgeScript edgeToErase = currDrawnEdges[nodeId];
        if(!edgeToErase) Debug.LogWarning("currDrawnEdges has a node (key) but edgescript == null (value)");

        Destroy(edgeToErase.gameObject);
        currDrawnEdges.Remove(nodeId);
        levelState.cardStates[removedId].drawnEdges.Remove(nodeId);

        if(CheckGraph()) Win();
        return;
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
        List<int> neighboursIds = graphData.nodes[removedId].adjacentNodeIds;
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
            int positionIndex = levelState.cardStates[cardId].scramble[i];
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
        gameWon = true;
        gameAdmiring = false;
        gamePaused = true;

        UIManager.ShowWinMenu();

        //show all answers!
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

        SetActiveCard(0);

        //reset save!!!
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


    // public void Quit()
    // {
    //     if (levelManager.daily) GameManager.Instance.LoadMainMenu();
    //     else GameManager.Instance.LoadLevelMenu();
    // }


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
