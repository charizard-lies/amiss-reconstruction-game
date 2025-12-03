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
            ManageNodePickup(mouseWorldPos);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;
            ManageNodeRelease();
        }

        if (isDragging) transform.position = mouseWorldPos + offset;
    }
    
    private void ManageNodePickup(Vector3 mouseWorldPos)
    {
        if (IsMouseOver())
            {
                if (snappedAnchor != null) UnsnapFromAnchor(snappedAnchor);
                isDragging = true;
                offset = transform.position - mouseWorldPos;
            }
        else StartCoroutine(DelayedSnap());
    }

    private void ManageNodeRelease()
    {
        AnchorScript potentialSnappingAnchor = FindAnchorToSnap();
        if (potentialSnappingAnchor)
        {
            SnapToAnchor(potentialSnappingAnchor);
            levelManager.UIManager.UpdateSolved(levelManager.CheckGraphSolved());
        }
        UpdateNodeState();
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

        return hit.collider != null && hit.collider.gameObject == gameObject;
    }

    private AnchorScript FindAnchorToSnap()
    {
        Collider2D[] anchorsFound = Physics2D.OverlapCircleAll(transform.position, snapRadius, anchorLayer);

        if (anchorsFound == null || anchorsFound.Length < 1) return null;

        Collider2D[] anchorCollidersByDist = anchorsFound.OrderBy(a => Vector2.Distance(a.transform.position, transform.position)).ToArray();

        foreach (Collider2D anchor in anchorCollidersByDist)
        {
            AnchorScript anchorDetected = anchor.GetComponent<AnchorScript>();
            if (anchorDetected == null) continue;
            if (!anchorDetected.CanAccept(this)) continue;

            return anchorDetected;
        }
        return null;
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
        nodeState.snapped = snappedAnchor != null;
        if (nodeState.snapped) nodeState.snappedAnchorId = snappedAnchor.id;
    }

    private IEnumerator DelayedSnap()
    {
        yield return null;
        ManageNodeRelease();
    }
}
