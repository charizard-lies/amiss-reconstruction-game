using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class NodeScript : MonoBehaviour
{
    public LayerMask anchorLayer;
    public float snapRadius;
    public int nodeId;
    public AnchorScript snappedAnchor;
    
    private bool isDragging = false;
    private Vector3 offset;
    private Camera cam;
    private LevelScript levelManager;
    private NodeState nodeState;

    private void Awake()
    {
        cam = Camera.main;
    }

    public void Initialize(int id, LevelScript level, CardScript card)
    {
        nodeId = id;
        levelManager = level;
        nodeState = card.cardState.nodes.First(nodeState => nodeState.nodeId == id);
    }   

    private void Update()
    {
        if (Mouse.current == null) return;
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = 0f;

        if (Mouse.current.leftButton.wasPressedThisFrame && !levelManager.gamePaused)
        {
            if (IsMouseOver())
            {
                if (snappedAnchor != null) UnsnapFromAnchor(snappedAnchor);
                isDragging = true;
                offset = transform.position - mouseWorldPos;
            }
            else StartCoroutine(DelayedSnap());
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;
            Snap();
        }

        if (isDragging) transform.position = mouseWorldPos + offset;
    }
    private bool IsMouseOver()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D hit = Physics2D.Raycast(
            mousePos2D,
            Vector2.zero,
            0f,
            ~0
        );

        return hit.collider != null && hit.collider.gameObject == this.gameObject;
    }

    void Snap()
    {
        Collider2D[] anchorsFound = Physics2D.OverlapCircleAll(transform.position, snapRadius, anchorLayer);

        if (anchorsFound == null || anchorsFound.Length < 1) return;
        
        Collider2D[] anchorCollidersByDist = anchorsFound.OrderBy(a => Vector2.Distance(a.transform.position, transform.position)).ToArray();

        foreach (Collider2D anchor in anchorCollidersByDist)
        {
            AnchorScript anchorScript = anchor.GetComponent<AnchorScript>();
            if (anchorScript == null) continue;
            if (!anchorScript.CanAccept(this)) continue;
            
            snappedAnchor = anchor.GetComponent<AnchorScript>();
            anchorScript.Attach(this);
            levelManager.UIManager.UpdateSolved(levelManager.CheckGraphSolved());
        }
        
    }

    public void SnapToAnchor(AnchorScript anchor)
    {
        snappedAnchor = anchor;
        anchor.Attach(this);
        UpdateNodeState();
    }

    public void UnsnapFromAnchor(AnchorScript anchor)
    {
        snappedAnchor = null;
        anchor.Detach(this);
        UpdateNodeState();
    }
    
    private void UpdateNodeState()
    {
        nodeState.pos = transform.position;
        nodeState.snappedAnchorId = snappedAnchor == null ? null : snappedAnchor.id;
    }

    private IEnumerator DelayedSnap()
    {
        yield return null; // Waits 1 frame
        Snap();
    }
}
