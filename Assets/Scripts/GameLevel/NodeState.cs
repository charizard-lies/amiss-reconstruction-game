using UnityEngine;

[System.Serializable]
public class NodeState
{
    public int nodeId;
    public Vector3 pos;

    public bool snapped;
    public int snappedAnchorId;

    public int? GetSnappedAnchorId()
    {
        if (!snapped) return null;
        else return snappedAnchorId;
    }
}