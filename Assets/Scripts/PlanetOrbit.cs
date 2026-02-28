using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private Vector3 currentOrbitPos;

    void Start()
    {
        if (orbitCenter == null)
            orbitCenter = GameObject.Find("SceneSphere")?.transform;

        if (orbitCenter == null)
        {
            Debug.LogWarning("PlanetOrbit: orbitCenter not found. Assign it manually in the Inspector.");
            return;
        }

        currentOrbitPos = transform.position - orbitCenter.position;
        if (currentOrbitPos == Vector3.zero)
            currentOrbitPos = Vector3.right * orbitRadius;
    }

    void Update()
    {
        if (orbitCenter == null) return;

        currentOrbitPos = Quaternion.AngleAxis(orbitSpeed * Time.deltaTime, orbitAxis.normalized) * currentOrbitPos;

        Vector3 newPos = orbitCenter.position + currentOrbitPos.normalized * orbitRadius;

        if (Vector3.Distance(orbitCenter.position, newPos) < boundaryRadius)
            transform.position = newPos;

        transform.Rotate(Vector3.up, selfRotationSpeed * Time.deltaTime);
    }
}