using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class NodeScript : MonoBehaviour
{
    public int id;
    private Camera cam;
    private LevelScript levelManager;

    private void Awake()
    {
        cam = Camera.main;
    }

    public void Initialize(int nodeId, LevelScript level)
    {
        id = nodeId;
        levelManager = level;
    }

    private void Update()
    {
        if (Mouse.current == null) return;
        if (Mouse.current.leftButton.wasPressedThisFrame && !levelManager.gamePaused && MouseIsOver()) ManageNodeClick();
    }

    private void ManageNodeClick()
    {
        if (id == levelManager.removedId) levelManager.PenDown();
        else levelManager.EraseEdge(id);
    }
    
    public bool MouseIsOver()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = Mathf.Abs(cam.transform.position.z);
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);

        Collider2D col = Physics2D.OverlapPoint(worldPos);
        return col != null && col.gameObject == gameObject;
    }
}
