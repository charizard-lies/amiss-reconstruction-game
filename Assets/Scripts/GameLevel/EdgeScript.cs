using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(LineRenderer))]
public class EdgeScript : MonoBehaviour
{
    public Transform PointA;
    public Transform PointB;

    private LevelScript levelManager;
    private float width;
    private LineRenderer lr;
    
    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 0;
        lr.enabled = false;
    }

    public void Initialize(Transform A, Transform B)
    {
        PointA = A;
        PointB = B;
        levelManager = LevelScript.Instance;
        width = levelManager.edgeWidth;

        lr.startWidth = width;
        lr.endWidth = width;
        lr.startColor = levelManager.edgeColor;
        lr.endColor = levelManager.edgeColor;

        SetEndpoints();
        lr.enabled = true;
    }

    public void SetEndpoints()
    {
        if (PointA != null)
        {
            Vector3 start = PointA.position;
            Vector3 end;
            if (PointB != null) end = PointB.position;
            else
            {
                Vector3 mousePos = Mouse.current.position.ReadValue();
                mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
                end = Camera.main.ScreenToWorldPoint(mousePos);
            }
            
            
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
    }

    void Update()
    {
        SetEndpoints();
    }
}
