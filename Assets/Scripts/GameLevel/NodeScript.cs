using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEditor.Rendering;



[RequireComponent(typeof(Collider2D))]
public class NodeScript : MonoBehaviour
{
    public static NodeScript HeldNode;
    public int id;
    private Camera cam;
    private LevelScript levelManager;
    public bool isDragged = false;
    private Vector3 velocity = Vector3.zero;

    public UnityEvent<NodeScript> onClicked;
    public UnityEvent<NodeScript> onReleased;
    
    private void Awake()
    {
        cam = Camera.main;
    }

    public void Initialize(int nodeId)
    {
        levelManager = LevelScript.Instance;
        id = nodeId;
    }

    private void Update()
    {
        if (Pointer.current.press.wasPressedThisFrame && PointerIsOver())
        {    
            onClicked?.Invoke(this);
            HeldNode = this;
        }

        if (Pointer.current.press.wasReleasedThisFrame && HeldNode == this)
        {
            onReleased?.Invoke(this);
            HeldNode = null;
        }

        if (isDragged)
        {
            if(TryGetPointerWorldPos(out Vector3 targetPos)) return;
            Approach(targetPos);
        }
    }

    public void Approach(Vector3 targetPos)
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            levelManager.nodeAttractionTime
        );
    }

    public bool PointerIsOver()
    {
        if (!TryGetPointerWorldPos(out Vector3 pos))
        return false;

        float radius = Pointer.current is Touchscreen
            ? levelManager.touchRadius
            : levelManager.clickRadius;
        
        Collider2D col = Physics2D.OverlapCircle(pos, radius);
        return col != null && col.gameObject == gameObject;
    }

    private bool TryGetPointerWorldPos( out Vector3 worldPos)
    {
        worldPos = default;

        if (Pointer.current == null) return false;

        Vector2 screenPos = Pointer.current.position.ReadValue();

        worldPos = cam.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        return true;
    }
    
}
