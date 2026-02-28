using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerSpaceController : MonoBehaviour
{
    public float speed = 8f;
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

        Vector3 move = transform.right * x + transform.forward * z + transform.up * y;

        if (move.sqrMagnitude > 1f) move.Normalize();

        controller.Move(move * speed * Time.deltaTime);
    }
}