using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "GraphData", menuName = "Graph/GraphData")]

public class GraphData : ScriptableObject
{
    private  System.Random rng = new System.Random();

    [System.Serializable]
    public class Node
    {
        public int id;
        public List<int> neighbourIds = new List<int>();
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
        public List<int> scramble = new List<int>();
    }

    
    [SerializeField] public List<Node> nodeData = new();
    public Dictionary<int, Node> nodes;
    public List<Edge> edges
    {
        get
        {
            HashSet<Edge> edgeSet = new HashSet<Edge>();
            for(int i=0;i<nodes.Count();i++)
            {
                foreach(var adjNodeId in nodes[i].neighbourIds)
                {
                    edgeSet.Add(new Edge(i, adjNodeId));
                }
            }
            return edgeSet.ToList();
        }
    }

    public List<CardArrangement> GenerateRandomCardArrangements()
    {
        List<CardArrangement> arrangements = new List<CardArrangement>();
        
        for(int i = 0; i < nodes.Count(); i++)
        {
            arrangements.Add(new CardArrangement());

            List<int> positionIndices = new List<int>();
            for(int j = 1; j < nodes.Count(); j++)
            {
                positionIndices.Add(j);
            }

            for (int j = positionIndices.Count()-1; j > 0; j--)
            {
                int k = rng.Next(j + 1);
                (positionIndices[j], positionIndices[k]) = (positionIndices[k], positionIndices[j]);
            }

            int l = 0;
            for (int j = 0; j < nodes.Count(); j++)
            {
                if (j == i) arrangements[i].scramble.Add(0);
                else arrangements[i].scramble.Add(positionIndices[l++]);
            }
        }

        return arrangements;
    }

    private void OnEnable()
    {
        nodes = nodeData.ToDictionary(n => n.id);
    }
}
