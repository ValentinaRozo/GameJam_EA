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
    [Tooltip("FollowCamera script used in third person")]
    public FollowCamera followCamera;

    [Header("Spawn")]
    public Transform spawnPoint;      // PlayerSpawn
    public float freezeDuration = 3f;

    [Header("Push")]
    public float pushDamping = 4f;

    private CharacterController controller;
    private bool frozen = false;
    private Vector3 externalVelocity = Vector3.zero;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;

        // Asegurar cámara desacoplada
        if (mainCamera != null)
            mainCamera.transform.SetParent(null);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.OnGoalScored += OnGoalScored;
    }

    void OnDisable()
    {
        GameManager.OnGoalScored -= OnGoalScored;
    }

    void OnGoalScored()
    {
        if (spawnPoint != null)
        {
            controller.enabled = false;
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
            controller.enabled = true;
        }

        externalVelocity = Vector3.zero;
        frozen = true;
        Invoke(nameof(Unfreeze), freezeDuration);
    }

    void Unfreeze()
    {
        frozen = false;
    }

    public void ApplyPush(Vector3 force)
    {
        externalVelocity += force;
    }

    void Update()
    {
        HandleCursorToggle();
        if (!frozen)
            HandleMovement();
    }

    void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !locked;
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

        Vector3 move = mainCamera.transform.right * x
                     + mainCamera.transform.forward * z
                     + mainCamera.transform.up * y;

        if (move.sqrMagnitude > 1f) move.Normalize();

        if (move.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(move),
                10f * Time.deltaTime);
        }

        // Movimiento normal
        controller.Move(move * speed * Time.deltaTime);

        // Empujes externos de comodines
        if (externalVelocity.sqrMagnitude > 0.01f)
        {
            controller.Move(externalVelocity * Time.deltaTime);
            externalVelocity = Vector3.Lerp(
                externalVelocity,
                Vector3.zero,
                pushDamping * Time.deltaTime);
        }
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
