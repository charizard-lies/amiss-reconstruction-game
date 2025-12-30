using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class ToolManager
{
    public static ToolManager Instance = new ToolManager();

    public ITool currentTool {get; private set; }

    public void SetTool(ITool tool)
    {
        currentTool = tool;
    }

    public void Register(NodeScript node)
    {
        node.onClicked.AddListener(n =>
        {
            currentTool.OnClicked(n);
        });

        node.onReleased.AddListener(n =>
        {
            currentTool.OnReleased(n);
        });
    }
}

public interface ITool
{
    void OnClicked(NodeScript node);
    void OnReleased(NodeScript node);
}

public class DrawTool: ITool
{
    public static DrawTool Instance = new DrawTool();
    private EdgeScript drawingEdge=null;

    public void OnClicked(NodeScript node)
    {
        LevelScript levelManager = LevelScript.Instance;
        if(node.id == levelManager.currRemovedId)
        {
            drawingEdge = levelManager.PenDown();
        }
        else
        {
            levelManager.EraseEdge(node.id);
        }
    }

    public void OnReleased(NodeScript node)
    {
        LevelScript levelManager = LevelScript.Instance;

        if(node.id == levelManager.currRemovedId)
        {
            NodeScript releasedOnNode = GetNodeScriptUnderMouse();
            int? releasedNodeId = releasedOnNode == null ? null : releasedOnNode.id;
            LevelScript.Instance.PenUp(releasedNodeId, drawingEdge);

            drawingEdge = null;
        }
    }

    private Vector3 GetPointerWorldPos()
    {
        Vector2 screenPos = Pointer.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;
        return worldPos;
    }

    private NodeScript GetNodeScriptUnderMouse()
    {
        var hit = Physics2D.Raycast(GetPointerWorldPos(), Vector2.zero);
        return hit ? hit.collider.GetComponent<NodeScript>() : null;
    }
}

public class SwapTool: ITool
{
    public void OnClicked(NodeScript node)
    {
        
    }

    public void OnReleased(NodeScript node)
    {
        
    }
}