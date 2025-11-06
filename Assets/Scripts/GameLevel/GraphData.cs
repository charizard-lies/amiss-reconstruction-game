using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;

[CreateAssetMenu(fileName = "GraphData", menuName = "Graph/GraphData")]

//I only control the data of the graphs, preparing it for other scripts to use
public class GraphData : ScriptableObject
{
    [System.Serializable]
    public class Edge
    {
        public int fromNodeId;
        public int toNodeId;
    }

    public List<int> nodeIds = new List<int>();
    public List<Edge> edges = new List<Edge>();

    public List<int> NodesReduce(int nodeId)
    {
        return nodeIds.Where(n => n != nodeId).ToList();
    }

    public List<Edge> EdgesReduce(int nodeId)
    {
        return edges.Where(n => n.fromNodeId != nodeId && n.toNodeId != nodeId).ToList();
    }

    public GraphData GraphReduce(int nodeId)
    {
        GraphData newGraph = CreateInstance<GraphData>();
        newGraph.nodeIds = NodesReduce(nodeId);
        newGraph.edges = EdgesReduce(nodeId);
        return newGraph;
    }

    public void addNode(int id)
    {
        if (!nodeIds.Contains(id))
        {
            nodeIds.Add(id);
        }
    }

    public void AddEdge(int aID, int bID)
    {
        if (!edges.Any(e =>
    (e.fromNodeId == aID && e.toNodeId == bID) ||
    (e.fromNodeId == bID && e.toNodeId == aID)))
        {
        Edge edge = new Edge { fromNodeId = aID, toNodeId = bID };
        edges.Add(edge);
    }
    }
}
