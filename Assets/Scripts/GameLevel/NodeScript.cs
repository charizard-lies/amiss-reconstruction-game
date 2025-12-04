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
    private bool isSnapping;
    private Vector2 currMouseWorldPos;
    private Camera cam;
    private LevelScript levelManager;
    private NodeState nodeState;
    private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        cam = Camera.main;
    }

    public void Initialize(int id, LevelScript level, CardScript card)
    {
        nodeId = id;
        levelManager = level;
        nodeState = card.cardState.nodes.First(nodeState => nodeState.nodeId == id);
        isSnapping = snappedAnchor == null ? false : true;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame && !levelManager.gamePaused) ManageNodePickup();
        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging) ManageNodeRelease();
        if (isDragging) ManageNodeDrag();
        if (isSnapping) ManageNodeSnap();

    }

    private void ManageNodePickup()
    {
        if (IsMouseOver())
        {
            if (snappedAnchor != null) UnsnapFromAnchor(snappedAnchor);
            isDragging = true;
        }
        else StartCoroutine(DelayedSnap());
    }
    
    private void ManageNodeDrag()
    {
        currMouseWorldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Approach(currMouseWorldPos);
    }

    private void ManageNodeRelease()
    {
        isDragging = false;
        AnchorScript potentialSnappingAnchor = FindAnchorToSnap();
        if (potentialSnappingAnchor)
        {
            isSnapping = true;
            SnapToAnchor(potentialSnappingAnchor);
            levelManager.UIManager.UpdateSolved(levelManager.CheckGraphSolved());
        }
        UpdateNodeState();
    }
    
    private void ManageNodeSnap()
    {
        if (!snappedAnchor) Debug.LogWarning("isSnapping is true but no anchor to snap to");

        Approach(snappedAnchor.transform.position);
    }

    private void Approach(Vector3 targetPos)
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            levelManager.nodeAttractionTime
        );
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
        isSnapping = true;
        snappedAnchor = anchor;
        anchor.Attach(this);
        UpdateNodeState();
    }

    public void UnsnapFromAnchor(AnchorScript anchor)
    {
        isSnapping = false;
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
