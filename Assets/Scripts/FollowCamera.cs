using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public float distance = 6f;

    [Header("Mouse Look")]
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
        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;

        // Iniciar con los ángulos actuales para evitar snap
        currentX = transform.eulerAngles.y;
        currentY = transform.eulerAngles.x;

        // Garantizar que la cámara no tenga padre
        transform.SetParent(null);
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
            currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            currentY = Mathf.Clamp(currentY, minY, maxY);
        }

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPos = target.position + rotation * new Vector3(0, 0, -distance);

        // Clamp dentro de la esfera
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
