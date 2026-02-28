using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerSpaceController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;

    [Header("Team")]
    [Tooltip("Team identifier: A or B")]
    public string teamID = "A";

    [Header("Sphere Boundary")]
    public Transform sphereCenter;
    public float boundaryRadius = 23.5f;

    [Header("Camera Setup")]
    [Tooltip("The main camera of the scene")]
    public Camera mainCamera;
    [Tooltip("FollowCamera script para tercera persona")]
    public FollowCamera followCamera;

    private CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;

        // Desanclar la cámara del jugador si quedó parenteada
        if (mainCamera != null)
            mainCamera.transform.SetParent(null);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleCursorToggle();
        HandleMovement();
    }

    void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void HandleMovement()
    {
        if (mainCamera == null) return;

        float x = 0f, z = 0f, y = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x = 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) z = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) z = -1f;
        if (Input.GetKey(KeyCode.E)) y = 1f;
        if (Input.GetKey(KeyCode.Q)) y = -1f;

        // Movimiento relativo a la cámara
        Vector3 move = mainCamera.transform.right * x
                     + mainCamera.transform.forward * z
                     + mainCamera.transform.up * y;

        if (move.sqrMagnitude > 1f) move.Normalize();

        // Rotar el jugador suavemente hacia la dirección de movimiento
        if (move.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(move),
                10f * Time.deltaTime);

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
