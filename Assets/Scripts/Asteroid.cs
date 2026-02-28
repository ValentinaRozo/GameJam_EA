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
    private Vector3 originalScale;
    private float originalSpeedMin, originalSpeedMax;
    private bool caosActive = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 0f;
        rb.angularDrag = 0.1f;

        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;

        originalScale = transform.localScale;
        originalSpeedMin = initialSpeedMin;
        originalSpeedMax = initialSpeedMax;

        rb.velocity = Random.onUnitSphere * Random.Range(initialSpeedMin, initialSpeedMax);
        rb.angularVelocity = Random.insideUnitSphere * rotationSpeedMax * Mathf.Deg2Rad;
    }

    void FixedUpdate() { EnforceBoundary(); }

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

    // AsteroidCaos 

    public void ApplyCaos(float scaleMultiplier, float speedMultiplier)
    {
        if (caosActive) return;
        caosActive = true;

        transform.localScale = originalScale * scaleMultiplier;

        // Aplicar velocidad actual multiplicada
        rb.velocity = rb.velocity.normalized *
                      Mathf.Clamp(rb.velocity.magnitude * speedMultiplier,
                                  initialSpeedMin * speedMultiplier,
                                  initialSpeedMax * speedMultiplier);

        // Guardar nuevos límites para EnforceBoundary
        initialSpeedMin = originalSpeedMin * speedMultiplier;
        initialSpeedMax = originalSpeedMax * speedMultiplier;

        Debug.Log($"[Asteroid] Caos ON — scale x{scaleMultiplier}, speed x{speedMultiplier}");
    }

    public void RemoveCaos()
    {
        if (!caosActive) return;
        caosActive = false;

        transform.localScale = originalScale;
        initialSpeedMin = originalSpeedMin;
        initialSpeedMax = originalSpeedMax;

        // Reducir velocidad al rango original
        rb.velocity = rb.velocity.normalized * Random.Range(originalSpeedMin, originalSpeedMax);

        Debug.Log("[Asteroid] Caos OFF");
    }
}
