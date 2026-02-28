using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 6f;
    public float mouseSensitivity = 3f;
    public float minY = -40f;
    public float maxY = 80f;

    private float currentX = 0f;
    private float currentY = 20f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
        currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        currentY = Mathf.Clamp(currentY, minY, maxY);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = new Vector3(0, 0, -distance);

        transform.position = target.position + rotation * direction;
        transform.LookAt(target.position);
    }
}