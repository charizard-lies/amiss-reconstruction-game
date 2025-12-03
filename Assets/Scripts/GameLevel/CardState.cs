using System.Collections.Generic;
using System.Data;
using System.Linq;

[System.Serializable]
public class CardState
{
    public List<NodeState> nodes = new List<NodeState>();
    public List<nodeAnchorIdPair> nodeAnchorIdPairs = new List<nodeAnchorIdPair>();
    public bool isVisible;

    [System.NonSerialized]
    public Dictionary<int, int> nodeAnchorIdMap = new Dictionary<int, int>();

    public void EnsureList()
    {
        nodeAnchorIdPairs = nodeAnchorIdMap
            .Select(kv => new nodeAnchorIdPair { nodeId = kv.Key, anchorId = kv.Value })
            .ToList();
    }
    
    public void EnsureDict()
    {
        nodeAnchorIdMap = nodeAnchorIdPairs.ToDictionary(p => p.nodeId, p => p.anchorId);
    }
}



[System.Serializable]
public class nodeAnchorIdPair
{
    public int nodeId;
    public int anchorId;
}