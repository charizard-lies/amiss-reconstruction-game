using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.PackageManager;

[CreateAssetMenu(fileName = "GraphData", menuName = "Graph/GraphData")]

//I only control the data of the graphs, preparing it for other scripts to use
public class GraphData : ScriptableObject
{
    [System.Serializable]
    public class Node
    {
        public int id;
        public List<int> adjacentNodeIds = new List<int>();
    }
    public class Edge
    {
        public int fromNodeId { get;}
        public int toNodeId { get;}

        public Edge(int a, int b)
        {
            if (a < b) { fromNodeId = a; toNodeId = b; }
            else { fromNodeId = b; toNodeId = a; }
        }
    }
    [System.Serializable]
    public class CardArrangement
    {
        public List<int> arrangement = new List<int>();
    }

    [SerializeField] public List<Node> nodes = new List<Node>();
    public List<Edge> edges
    {
        get
        {
            HashSet<Edge> edgeSet = new HashSet<Edge>();
            for(int i=0;i<nodes.Count();i++)
            {
                foreach(var adjNodeId in nodes[i].adjacentNodeIds)
                {
                    edgeSet.Add(new Edge(i, adjNodeId));
                }
            }
            return edgeSet.ToList();
        }
    }
    [SerializeField] public List<CardArrangement> nodePositions = new List<CardArrangement>();
}
