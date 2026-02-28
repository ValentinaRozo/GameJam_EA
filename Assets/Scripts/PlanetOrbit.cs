using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlanetOrbit : MonoBehaviour
{
    [Header("Orbit")]
    public Transform orbitCenter;
    public float orbitRadius = 15f;
    public float orbitSpeed = 20f;
    public Vector3 orbitAxis = Vector3.up;

    [Header("Self Rotation")]
    public float selfRotationSpeed = 50f;

    [Header("Boundary")]
    public float boundaryRadius = 24f;

    private Rigidbody rb;
    private Vector3 currentOrbitPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        if (orbitCenter == null)
            orbitCenter = GameObject.Find("SceneSphere")?.transform;

        if (orbitCenter == null) { Debug.LogWarning("PlanetOrbit: orbitCenter no asignado."); return; }

        currentOrbitPos = transform.position - orbitCenter.position;
        if (currentOrbitPos == Vector3.zero)
            currentOrbitPos = Vector3.right * orbitRadius;
    }

    void FixedUpdate()
    {
        if (orbitCenter == null) return;

        // Rotar la posición orbital
        currentOrbitPos = Quaternion.AngleAxis(orbitSpeed * Time.fixedDeltaTime, orbitAxis.normalized) * currentOrbitPos;
        Vector3 newPos = orbitCenter.position + currentOrbitPos.normalized * orbitRadius;

        if (Vector3.Distance(orbitCenter.position, newPos) < boundaryRadius)
            rb.MovePosition(newPos);

        // Auto-rotación
        Quaternion newRot = transform.rotation * Quaternion.Euler(Vector3.up * selfRotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newRot);
    }
}
