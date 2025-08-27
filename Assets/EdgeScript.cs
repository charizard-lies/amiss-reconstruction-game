using UnityEngine;
using UnityEngine.UIElements;

public class EdgeScript : MonoBehaviour
{
    public NodeScript NodeA;
    public NodeScript NodeB;
    private LineRenderer lr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 1;
    }

    // Update is called once per frame
    public void Initialize(NodeScript nodeA, NodeScript nodeB)
    {
        NodeA = nodeA;
        NodeB = nodeB;
    }

    void Update()
    {
        if (NodeA != null && NodeB != null)
        {
            Vector3 start = NodeA.transform.position;
            start.z = 0.5f;
            Vector3 end = NodeB.transform.position;
            end.z = 0.5f;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
    }
}
