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
        if (Pointer.current == null) return;

        if (Pointer.current.press.wasPressedThisFrame && !levelManager.gamePaused && PointerIsOver())
            ManageNodeClick();
    }

    private void ManageNodeClick()
    {
        if (id == levelManager.removedId) levelManager.PenDown();
        else levelManager.EraseEdge(id);
    }
    
    public bool PointerIsOver()
    {
        if (Pointer.current == null)
            return false;
            
        Vector2 screenPos = Pointer.current.position.ReadValue();

        float radius = Pointer.current is Touchscreen
            ? levelManager.touchRadius
            : levelManager.clickRadius;

        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(
            screenPos.x,
            screenPos.y,
            Mathf.Abs(cam.transform.position.z)
        ));

        Collider2D col = Physics2D.OverlapCircle(worldPos, radius);
        return col != null && col.gameObject == gameObject;
    }

}
