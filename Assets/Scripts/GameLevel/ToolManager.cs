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
    public LevelScript levelManager = LevelScript.Instance;
    private EdgeScript drawingEdge=null;

    public void OnClicked(NodeScript node)
    {
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
    public static SwapTool Instance = new SwapTool();
    LevelScript levelManager = LevelScript.Instance;

    public void OnClicked(NodeScript node)
    {
        if(node.id == levelManager.currRemovedId) return;
        node.SetFollowPointer();
    }

    public void OnReleased(NodeScript node)
    {
        if(node.id == levelManager.currRemovedId) return;
        NodeScript nodeToApproach = GetClosestNodeInRange(node.id);
        if(nodeToApproach == null || nodeToApproach.id == levelManager.currRemovedId) node.SetTargetLocalPos(levelManager.currNodeLocalPosMap[node.id]);
        else levelManager.SwapNodes(node.id, nodeToApproach.id);
    }

    private bool TryGetPointerWorldPos( out Vector3 worldPos)
    {
        worldPos = default;
        if (Pointer.current == null) return false;

        Vector2 screenPos = Pointer.current.position.ReadValue();
        worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        return true;
    }

    private NodeScript GetClosestNodeInRange(int? excludeId)
    {
        if(!TryGetPointerWorldPos(out Vector3 mousePos)) return null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(mousePos, levelManager.snapRadius);
        if (hits == null || hits.Length ==0) return null;

        NodeScript closestNode = null;
        float closestDist = float.PositiveInfinity;

        foreach(var col in hits)
        {
            if(!col.TryGetComponent(out NodeScript node)) continue;
            if(excludeId.HasValue && node.id == excludeId.Value) continue; 

            float sqrDist = (node.transform.position - mousePos).sqrMagnitude;
            if(sqrDist < closestDist)
            {
                closestNode = node;
                closestDist = sqrDist;
            }
        }
        return closestNode;
    }

}