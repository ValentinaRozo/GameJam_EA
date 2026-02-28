using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerSpaceController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;

    [Header("Rotation")]
    public float mouseSensitivity = 3f;
    public float verticalRotationLimit = 80f;

    [Header("Team")]
    [Tooltip("Team identifier: A or B")]
    public string teamID = "A";

    [Header("Sphere Boundary")]
    public Transform sphereCenter;
    public float boundaryRadius = 23.5f;

    private CharacterController controller;
    private float rotationX = 0f;
    private float rotationY = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;

        // Initialize rotation from current transform so there is no snap on start
        Vector3 startAngles = transform.eulerAngles;
        rotationX = startAngles.y;
        rotationY = startAngles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    void HandleRotation()
    {
        rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationY = Mathf.Clamp(rotationY, -verticalRotationLimit, verticalRotationLimit);

        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
    }

    void HandleMovement()
    {
        float x = 0f, z = 0f, y = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x = 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) z = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) z = -1f;
        if (Input.GetKey(KeyCode.E)) y = 1f;
        if (Input.GetKey(KeyCode.Q)) y = -1f;

        Vector3 move = transform.right * x + transform.forward * z + transform.up * y;
        if (move.sqrMagnitude > 1f) move.Normalize();

        controller.Move(move * speed * Time.deltaTime);
    }

    void LateUpdate()
    {
        EnforceBoundary();
    }

    void EnforceBoundary()
    {
        if (sphereCenter == null) return;

        Vector3 offset = transform.position - sphereCenter.position;
        if (offset.magnitude > boundaryRadius)
            transform.position = sphereCenter.position + offset.normalized * boundaryRadius;
    }
}