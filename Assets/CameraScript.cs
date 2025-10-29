using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour
{
    public float targetAspect = 9f/16f;
    public float baseOrthoSize = 11f;
    private Camera cam;

    void Start() => cam = GetComponent<Camera>();

    void Update()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        cam.orthographicSize = baseOrthoSize * (targetAspect / currentAspect );
    }
}