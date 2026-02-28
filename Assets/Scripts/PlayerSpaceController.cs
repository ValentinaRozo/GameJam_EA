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

    [Header("Camera")]
    public Camera mainCamera;

    [Header("Audio")]
    public AudioSource shipAudio;                // <- arrastra aquí tu AudioSource
    public float minVelocityToPlay = 0.05f;      // umbral para considerarlo “movimiento”

    private CharacterController controller;
    private bool frozen = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;

        if (mainCamera == null)
            mainCamera = Camera.main;

        // Si no lo asignas, intenta agarrarlo del mismo GameObject
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
        controller.Move(force * Time.deltaTime);
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
            if (shipAudio.isPlaying) shipAudio.Pause(); // o Stop() si prefieres reiniciar siempre
        }
    }

    public void Freeze() { frozen = true; }
    public void Unfreeze() { frozen = false; }
}