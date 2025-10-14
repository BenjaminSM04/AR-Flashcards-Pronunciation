using UnityEngine;

public class BillboardFaceCamera : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;
        transform.forward = Camera.main.transform.forward;
    }
}
