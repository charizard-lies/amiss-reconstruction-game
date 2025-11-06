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
        node.transform.position = new Vector3(transform.position.x, transform.position.y, node.transform.position.z);
        if (!attachedNodes.Contains(node))
        {
            attachedNodes.Add(node);
        }
        else
        {
            Debug.LogWarning($"Node {node.nodeId} was already attached to anchor {id}");
        }
    }

    public void Detach(NodeScript node)
    {
        if (attachedNodes.Remove(node))
        {
            //Debug.Log($"Node {node.nodeId} detached from anchor {id}");
        }
        else
        {
            Debug.LogWarning($"Node {node.nodeId} was not attached to anchor {id}");
        }
    }
}
