using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerSpaceController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float pushMultiplier = 1f;

    [Header("Team")]
    [Tooltip("Team identifier: A or B")]
    public string teamID = "A";

    [Header("Sphere Boundary")]
    public Transform sphereCenter;
    public float boundaryRadius = 23.5f;

    [Header("Camera")]
    public Camera mainCamera;

    [Header("Audio")]
    public AudioSource shipAudio;
    public float minVelocityToPlay = 0.05f;

    [Header("Push Settings")]
    public float pushDamping = 8f; // Qué tan rápido se desvanece el empujón

    private CharacterController controller;
    private bool frozen = false;
    private Vector3 externalForce = Vector3.zero; // Acumula empujones externos

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (shipAudio == null)
            shipAudio = GetComponent<AudioSource>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (frozen)
        {
            UpdateMovementAudio(false);
            return;
        }

        HandleCursorToggle();
        HandleMovement();

        // Aplicar empujones externos acumulados
        if (externalForce.sqrMagnitude > 0.01f)
        {
            controller.Move(externalForce * Time.deltaTime);
            // Reducir gradualmente la fuerza
            externalForce = Vector3.Lerp(externalForce, Vector3.zero, pushDamping * Time.deltaTime);
        }

        // CharacterController.velocity es la velocidad real resultante
        bool isMoving = controller.velocity.sqrMagnitude > (minVelocityToPlay * minVelocityToPlay);
        UpdateMovementAudio(isMoving);
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
        float x = 0f, z = 0f, y = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x = 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) z = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) z = -1f;
        if (Input.GetKey(KeyCode.E)) y = 1f;
        if (Input.GetKey(KeyCode.Q)) y = -1f;

        Vector3 camForward = mainCamera != null ? mainCamera.transform.forward : transform.forward;
        Vector3 camRight = mainCamera != null ? mainCamera.transform.right : transform.right;
        Vector3 camUp = mainCamera != null ? mainCamera.transform.up : transform.up;

        Vector3 move = camRight * x + camForward * z + camUp * y;
        if (move.sqrMagnitude > 1f) move.Normalize();

        if (move.sqrMagnitude > 0.01f)
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

    public void ApplyPush(Vector3 force)
    {
        // Acumula la fuerza en lugar de aplicarla inmediatamente
        externalForce += force;
    }

    void UpdateMovementAudio(bool shouldPlay)
    {
        if (shipAudio == null) return;

        if (shouldPlay)
        {
            if (!shipAudio.isPlaying) shipAudio.Play();
        }
        else
        {
            if (shipAudio.isPlaying) shipAudio.Pause();
        }
    }

    public void Freeze() { frozen = true; }
    public void Unfreeze() { frozen = false; }
}
