using UnityEngine;
using System.Collections.Generic;
using System.Linq;

//I only control the data of the graphs, preparing it for other scripts to use
public class GraphData : ScriptableObject
{
    [System.Serializable]
    public class Node
    {
        public int pos=-1;
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
    
    
    public void SetNodes(int n)
    {
        nodes.Clear();
        for(int i = 0; i < n; i++)
        {
            nodes.Add(new Node{pos = i});
        }
    }
    
    public void AddEdge(int a, int b)
    {
        if(nodes[a].adjacentNodeIds.Contains(b) || nodes[b].adjacentNodeIds.Contains(a)) Debug.LogError("edge already exists");

        nodes[a].adjacentNodeIds.Add(b);
        nodes[b].adjacentNodeIds.Add(a);
    }

    public List<Node> GetDegreeSortedNodes()
    {
        return nodes.OrderByDescending(n => n.adjacentNodeIds.Count).ToList();
    }

}
