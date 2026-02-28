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

    [Header("Escalado por gol")]
    [Tooltip("Cuánto aumenta la velocidad orbital por cada gol")]
    public float speedIncrement = 5f;
    [Tooltip("Velocidad orbital máxima")]
    public float maxOrbitSpeed = 80f;
    [Tooltip("Cuánto aumenta la autorotación por cada gol")]
    public float rotationIncrement = 15f;
    [Tooltip("Autorotación máxima")]
    public float maxRotationSpeed = 200f;

    private Rigidbody rb;
    private Vector3 currentOrbitPos;
    private float currentOrbitSpeed;
    private float currentRotationSpeed;

    void OnEnable() { GameManager.OnGoalScored += OnGoalScored; }
    void OnDisable() { GameManager.OnGoalScored -= OnGoalScored; }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        currentOrbitSpeed = orbitSpeed;
        currentRotationSpeed = selfRotationSpeed;

        if (orbitCenter == null)
            orbitCenter = GameObject.Find("SceneSphere")?.transform;

        if (orbitCenter == null)
        {
            Debug.LogWarning("PlanetOrbit: orbitCenter no encontrado.");
            return;
        }

        currentOrbitPos = transform.position - orbitCenter.position;
        if (currentOrbitPos == Vector3.zero)
            currentOrbitPos = Vector3.right * orbitRadius;
    }

    void OnGoalScored()
    {
        currentOrbitSpeed = Mathf.Min(currentOrbitSpeed + speedIncrement, maxOrbitSpeed);
        currentRotationSpeed = Mathf.Min(currentRotationSpeed + rotationIncrement, maxRotationSpeed);

        Debug.Log($"[PlanetOrbit] {gameObject.name} | Vel orbital: {currentOrbitSpeed:F1}");
    }

    void FixedUpdate()
    {
        if (orbitCenter == null) return;

        currentOrbitPos = Quaternion.AngleAxis(
            currentOrbitSpeed * Time.fixedDeltaTime,
            orbitAxis.normalized) * currentOrbitPos;

        Vector3 newPos = orbitCenter.position + currentOrbitPos.normalized * orbitRadius;

        if (Vector3.Distance(orbitCenter.position, newPos) < boundaryRadius)
            rb.MovePosition(newPos);

        Quaternion newRot = transform.rotation *
                            Quaternion.Euler(Vector3.up * currentRotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newRot);
    }
}
