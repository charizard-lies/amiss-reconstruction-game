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
    private SpriteRenderer sr;
    private int activeNodeLayer;
    private int inactiveNodeLayer;
    private LevelScript levelManager;

    private void Awake()
    {
        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 2;
    }

    public void Initialize(int id, LevelScript level)
    {
        nodeId = id;
        activeNodeLayer = level.activeNodeLayer;
        inactiveNodeLayer = level.inactiveNodeLayer;
        transform.gameObject.layer = inactiveNodeLayer;
        levelManager = level;
    }   

    private void Update()
    {
        if (Mouse.current == null) return;

        // Get mouse position in world space
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = 0f;

        //start drag
        if (Mouse.current.leftButton.wasPressedThisFrame && !levelManager.gamePaused)
        {
            
            if (IsMouseOver())
            {
                //first detach current anchor
                if (snappedAnchor != null)
                {
                    snappedAnchor.Detach(this);
                    snappedAnchor = null;
                }

                isDragging = true;
                offset = transform.position - mouseWorldPos;
            }
            else
            {
                StartCoroutine(DelayedSnap());
            }
        }

        //end drag
        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;
            Snap();
        }

        //follow mouse
        if (isDragging)
        {
            transform.position = mouseWorldPos + offset;
        }
    }

    //check if mouse is over my current node
    private bool IsMouseOver()
    {
        Vector2 mouseWorld = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        LayerMask activeLayerMask = 1 << activeNodeLayer;

        Collider2D hit = Physics2D.OverlapPoint(mouseWorld, activeLayerMask);
        return hit != null && hit.gameObject == this.gameObject;
    }

    void Snap()
    {
        Collider2D[] anchors = Physics2D.OverlapCircleAll(transform.position, snapRadius, anchorLayer);

        //found anchors
        if (anchors != null && anchors.Length > 0)
        {
            Collider2D[] sorted_anchors = anchors.OrderBy(a => Vector2.SqrMagnitude(a.transform.position - transform.position)).ToArray();

            //Cycle through all anchors in range by distance
            foreach (Collider2D anchor in sorted_anchors)
            {
                AnchorScript anchorScript = anchor.GetComponent<AnchorScript>();
                if (anchorScript != null)
                {
                    //anchor is empty, attach to new anchor
                    if (anchorScript.CanAccept(this))
                    {
                        snappedAnchor = anchor.GetComponent<AnchorScript>();
                        anchorScript.Attach(this);
                        break;
                    }
                    //anchor is filled
                    else continue;

                }
            }

        }
        //no anchors
        else
        {
            if (snappedAnchor != null)
            {
                AnchorScript anchorScript = snappedAnchor.GetComponent<AnchorScript>();
                anchorScript.Detach(this);
                snappedAnchor = null;
            }
        }
    }
    
    private IEnumerator DelayedSnap()
    {
        yield return null; // Waits 1 frame
        Snap();
    }
}
