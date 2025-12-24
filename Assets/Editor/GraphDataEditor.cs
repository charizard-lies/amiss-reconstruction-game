using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

[CustomEditor(typeof(GraphData))]
public class GraphDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GraphData graph = (GraphData)target;

        if (GUILayout.Button("Update From Scene"))
        {
            graph.nodes.Clear();

            foreach (var node in FindObjectsByType<NodeScript>(FindObjectsSortMode.None))
            {
                graph.nodes.Add(new GraphData.Node
                {
                    id = node.nodeId,
                    position = node.transform.position
                });
            }

            EditorUtility.SetDirty(graph);
        }
    }
}
