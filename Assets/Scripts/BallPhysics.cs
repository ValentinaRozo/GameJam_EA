using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallPhysics : MonoBehaviour
{
    [Header("Physics")]
    public float maxSpeed = 12f;

    [Header("Sphere Boundary")]
    public Transform sphereCenter;
    public float boundaryRadius = 23f;
    public float bounceDamping = 0.85f;

    // "A", "B", or "" if nobody has touched it yet
    [HideInInspector] public string lastToucherTeam = "";

    [HideInInspector] public Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;
    }

    void FixedUpdate()
    {
        EnforceBoundary();
    }

    void EnforceBoundary()
    {
        if (sphereCenter == null) return;

        Vector3 fromCenter = transform.position - sphereCenter.position;
        float dist = fromCenter.magnitude;

        if (dist >= boundaryRadius)
        {
            // Push back inside
            transform.position = sphereCenter.position + fromCenter.normalized * (boundaryRadius - 0.1f);

            // Reflect velocity off the inner wall
            Vector3 normal = -fromCenter.normalized;
            rb.velocity = Vector3.Reflect(rb.velocity, normal) * bounceDamping;
        }
    }

    public void RegisterTouch(string teamID)
    {
        lastToucherTeam = teamID;
    }

    public void Impulse(Vector3 dir, float force)
    {
        dir = dir.normalized;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(dir * force, ForceMode.Impulse);

        if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;
    }
}