using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Asteroid : MonoBehaviour
{
    [Header("Initial Movement")]
    public float initialSpeedMin = 2f;
    public float initialSpeedMax = 6f;
    public float rotationSpeedMax = 90f;

    [Header("Boundary")]
    public Transform sphereCenter;
    public float boundaryRadius = 24f;

    [Header("Physics")]
    public float bounceDamping = 0.85f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 0f;
        rb.angularDrag = 0.1f;

        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;

        rb.velocity = Random.onUnitSphere * Random.Range(initialSpeedMin, initialSpeedMax);
        rb.angularVelocity = Random.insideUnitSphere * rotationSpeedMax * Mathf.Deg2Rad;
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
            transform.position = sphereCenter.position + fromCenter.normalized * (boundaryRadius - 0.2f);
            Vector3 normal = -fromCenter.normalized;
            rb.velocity = Vector3.Reflect(rb.velocity, normal) * bounceDamping;
        }
    }
}