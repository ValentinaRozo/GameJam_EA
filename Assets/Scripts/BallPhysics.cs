using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallPhysics : MonoBehaviour
{
    public Rigidbody rb;
    public float maxSpeed = 12f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    public void Impulse(Vector3 dir, float force)
    {
        dir = dir.normalized;

        // “Pase limpio”
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(dir * force, ForceMode.Impulse);

        // Limitar velocidad
        if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;
    }
}