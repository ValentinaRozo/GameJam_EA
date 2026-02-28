using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 6f;
    public float mouseSensitivity = 3f;
    public float minY = -40f;
    public float maxY = 80f;

    [Header("Sphere Boundary")]
    public Transform sphereCenter;
    public float boundaryRadius = 23f;

    private float currentX = 0f;
    private float currentY = 20f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;
    }

    void LateUpdate()
    {
        if (target == null) return;

        currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
        currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        currentY = Mathf.Clamp(currentY, minY, maxY);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = new Vector3(0, 0, -distance);
        Vector3 desiredPos = target.position + rotation * direction;

        // If desired position is outside the sphere, pull it back inside
        if (sphereCenter != null)
        {
            Vector3 offset = desiredPos - sphereCenter.position;
            if (offset.magnitude > boundaryRadius)
                desiredPos = sphereCenter.position + offset.normalized * (boundaryRadius - 0.1f);
        }

        transform.position = desiredPos;
        transform.LookAt(target.position);
    }
}