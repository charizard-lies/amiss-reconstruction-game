using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

public class EdgeScript : MonoBehaviour
{
    public Transform PointA;
    public Transform PointB;

    private float width;
    private Color color;
    private LineRenderer lr;
    
    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 0;
    }

    // Update is called once per frame
    public void Initialize(Transform A, Transform B, float linew, Color linecol)
    {
        PointA = A;
        PointB = B;
        width = linew;
        color = linecol;

        lr.startWidth = width;
        lr.endWidth = width;
        lr.startColor = linecol;
        lr.endColor = linecol;
        
    }

    void Update()
    {
        if (PointA != null && PointB != null)
        {
            Vector3 start = PointA.position;
            Vector3 end = PointB.position;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
    }
}
