using System.Collections.Generic;
using UnityEngine;

public class AnchorScript : MonoBehaviour
{
    public int id;

    public List<NodeScript> attachedNodes = new List<NodeScript>();
    
    public bool CanAccept(NodeScript node)
    {
        foreach (var attachedNode in attachedNodes)
        {
            if (attachedNode.transform.parent == node.transform.parent)
            {
                return false;
            }
        }
        return true;
    }

    public void Attach(NodeScript node)
    {
        if (!attachedNodes.Contains(node)) attachedNodes.Add(node);
        else Debug.LogWarning($"Node {node.nodeId} was already attached to anchor {id}");
    }

    public void Detach(NodeScript node)
    {
        attachedNodes.Remove(node);
    }
}
