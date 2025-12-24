using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MasterGraph : ScriptableObject
{
    GraphData graphData;

    public List<GraphData> deck = new List<GraphData>();

    public void GenerateDeck()
    {
        for(int i = 0; i < graphData.nodes.Count(); i++)
        {
            deck.Add(GraphReduce(i));
        }
    }
   
    private GraphData GraphReduce(int removeIndex)
    {
        GraphData reducedGraph = new GraphData();
        reducedGraph.SetNodes(graphData.nodes.Count-1);
        
        foreach(var edge in graphData.edges)
        {
            if(edge.fromNodeId == removeIndex || edge.toNodeId == removeIndex) continue;

            reducedGraph.nodes[edge.fromNodeId].adjacentNodeIds.Add(edge.toNodeId);
            reducedGraph.nodes[edge.toNodeId].adjacentNodeIds.Add(edge.fromNodeId);
        }

        return reducedGraph;
    }

}
