using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CardState
{
    public List<NodeState> nodes = new List<NodeState>();
    public Dictionary<int, int> nodeAnchorIdMap = new Dictionary<int, int>();
    public bool isVisible;
}