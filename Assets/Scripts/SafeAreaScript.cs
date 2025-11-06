using UnityEngine;

[ExecuteAlways]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform panel;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);
    private Vector2Int lastScreenSize = new Vector2Int(0, 0);

    void Awake()
    {
        panel = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
        // Detect orientation or resolution changes
        if (Screen.safeArea != lastSafeArea || 
            Screen.width != lastScreenSize.x || 
            Screen.height != lastScreenSize.y)
        {
            ApplySafeArea();
        }
    }

    void ApplySafeArea()
    {
        if (panel == null)
            return;

        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        // Convert safe area rectangle (in pixels) to normalized anchor coordinates
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;
    }
}
