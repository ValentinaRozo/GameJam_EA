using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerSpaceController : MonoBehaviour
{
    public float speed = 8f;
    public Transform cameraTransform;
    public Transform ballTransform;
    public BallOwnership ballOwnership;

    private CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x = 1f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) z = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) z = -1f;

        float y = 0f;
        if (Input.GetKey(KeyCode.E)) y = 1f;
        if (Input.GetKey(KeyCode.Q)) y = -1f;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = right * x + forward * z + Vector3.up * y;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        controller.Move(move * speed * Time.deltaTime);

        if (ballOwnership != null && ballTransform != null)
        {
            if (Vector3.Distance(transform.position, ballTransform.position) < 1.3f)
                ballOwnership.SetHolder(transform);
            else
                ballOwnership.ClearHolder(transform);
        }

    }
}