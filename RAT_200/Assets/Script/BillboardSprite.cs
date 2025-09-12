using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    public Camera cam;
    public bool lockYAxis = true;

    void LateUpdate()
    {
        if (!cam) return;
        if (lockYAxis)
        {
            var dir = cam.transform.position - transform.position; dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(-dir, Vector3.up);
        }
        else
        {
            transform.forward = (transform.position - cam.transform.position).normalized;
        }
    }
}
