using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        ScaleToCamera();
    }

    void Update()
    {
        // In Editor or if camera resizes dynamically
        ScaleToCamera();
    }

    void ScaleToCamera()
    {
        if (sr == null || Camera.main == null) return;

        float screenRatio = (float)Screen.width / Screen.height;
        float targetHeight = Camera.main.orthographicSize * 2f;
        float targetWidth = targetHeight * screenRatio;

        // Get size of sprite in world units
        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;

        // Scale to match height
        float scale = targetHeight / spriteHeight;

        // Apply scale to match height first
        transform.localScale = new Vector3(scale, scale, 1f);

        // If width doesn’t fill screen, expand horizontally proportionally
        if (spriteWidth * scale < targetWidth)
        {
            float widthScale = targetWidth / (spriteWidth * scale);
            transform.localScale = new Vector3(scale * widthScale, scale, 1f);
        }
    }
}