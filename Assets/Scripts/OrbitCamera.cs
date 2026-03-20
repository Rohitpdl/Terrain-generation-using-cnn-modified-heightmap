using UnityEngine;

/// <summary>
/// Attach to the Camera in terrainviewer scene.
/// Left-mouse drag to orbit, scroll wheel to zoom.
/// </summary>
public class OrbitCamera : MonoBehaviour
{
    public Transform target;
    public float distance    = 80f;
    public float xSpeed      = 120f;
    public float ySpeed      = 60f;
    public float yMinLimit   = 5f;
    public float yMaxLimit   = 80f;
    public float zoomSpeed   = 10f;
    public float minDistance = 10f;
    public float maxDistance = 200f;

    float x, y;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // ── Mouse drag to orbit ───────────────────────────────────────────────
        if (Input.GetMouseButton(0))
        {
            x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
            y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
            y  = Mathf.Clamp(y, yMinLimit, yMaxLimit);
        }

        // ── Scroll wheel to zoom ──────────────────────────────────────────────
        distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        distance  = Mathf.Clamp(distance, minDistance, maxDistance);

        Quaternion rot = Quaternion.Euler(y, x, 0);
        transform.rotation = rot;
        transform.position = rot * new Vector3(0, 0, -distance) + target.position;
    }
}
