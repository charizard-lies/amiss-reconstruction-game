using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class NodeScript : MonoBehaviour
{
    private LayerMask anchorLayer;
    public float snapRadius;
    public int nodeId;
    public AnchorScript snappedAnchor;

    private bool isDragging = false;
    private bool isSnapping;
    private Vector2 currMouseWorldPos;
    private Camera cam;
    private LevelScript levelManager;
    // private NodeState nodeState;
    private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        cam = Camera.main;
    }

    public void Initialize(int id, LevelScript level)
    {
        nodeId = id;
        levelManager = level;
        // nodeState = card.cardState.nodes.First(nodeState => nodeState.nodeId == id);
        isSnapping = snappedAnchor == null ? false : true;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        Debug.Log(levelManager == null);
        if (Mouse.current.leftButton.wasPressedThisFrame && !levelManager.gamePaused) ManageNodeClick();
        // if (Mouse.current.leftButton.wasReleasedThisFrame) ManageNodeRelease();
        // if (isDragging) ManageNodeDrag();
        // if (isSnapping) ManageNodeSnap();

    }

    private void ManageNodeClick()
    {
        if (IsMouseOver())
        {
            GameObject edgeObj = Instantiate(levelManager.edgePrefab, levelManager.transform);
            EdgeScript edgeScript = edgeObj.GetComponent<EdgeScript>();
            edgeScript.Initialize(transform, null, levelManager);
        }
    }
    
    // private void ManageNodeDrag()
    // {
    //     currMouseWorldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    //     Approach(currMouseWorldPos);
    // }

    // private void ManageNodeRelease()
    // {
    //     isDragging = false;
    //     AnchorScript potentialSnappingAnchor = FindAnchorToSnap();
    //     if (potentialSnappingAnchor)
    //     {
    //         isSnapping = true;
    //         SnapToAnchor(potentialSnappingAnchor);
    //         levelManager.UIManager.UpdateSolved(levelManager.CheckGraphSolved());
    //     }
    //     UpdateNodeState();
    // }
    
    // private void ManageNodeSnap()
    // {
    //     if (!snappedAnchor) Debug.LogWarning("isSnapping is true but no anchor to snap to");

    //     Approach(snappedAnchor.transform.position);
    // }

    // private void Approach(Vector3 targetPos)
    // {
    //     transform.position = Vector3.SmoothDamp(
    //         transform.position,
    //         targetPos,
    //         ref velocity,
    //         levelManager.nodeAttractionTime
    //     );
    // }
    
    private bool IsMouseOver()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = Mathf.Abs(cam.transform.position.z);
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);

        Collider2D col = Physics2D.OverlapPoint(worldPos);
        return col != null && col.gameObject == gameObject;
    }

    // private AnchorScript FindAnchorToSnap()
    // {
    //     Collider2D[] anchorsFound = Physics2D.OverlapCircleAll(transform.position, snapRadius, anchorLayer);

    //     if (anchorsFound == null || anchorsFound.Length < 1) return null;

    //     Collider2D[] anchorCollidersByDist = anchorsFound.OrderBy(a => Vector2.Distance(a.transform.position, transform.position)).ToArray();

    //     foreach (Collider2D anchor in anchorCollidersByDist)
    //     {
    //         AnchorScript anchorDetected = anchor.GetComponent<AnchorScript>();
    //         if (anchorDetected == null) continue;
    //         if (!anchorDetected.CanAccept(this)) continue;

    //         return anchorDetected;
    //     }
    //     return null;
    // }

    // public void SnapToAnchor(AnchorScript anchor)
    // {
    //     isSnapping = true;
    //     snappedAnchor = anchor;
    //     anchor.Attach(this);
    //     UpdateNodeState();
    // }

    // public void UnsnapFromAnchor(AnchorScript anchor)
    // {
    //     isSnapping = false;
    //     snappedAnchor = null;
    //     anchor.Detach(this);
    //     UpdateNodeState();
    // }
    
    // private void UpdateNodeState()
    // {
    //     nodeState.pos = transform.position;
    //     nodeState.snapped = snappedAnchor != null;
    //     if (nodeState.snapped) nodeState.snappedAnchorId = snappedAnchor.id;
    // }

    // private IEnumerator DelayedSnap()
    // {
    //     yield return null;
    //     ManageNodeRelease();
    // }
}
