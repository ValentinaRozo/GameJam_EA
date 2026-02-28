using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetOrbit : MonoBehaviour
{
    [Header("Órbita")]
    public Transform orbitCenter;        // Centro de la esfera grande
    public float orbitRadius = 15f;      // Distancia al centro
    public float orbitSpeed = 20f;       // Grados por segundo
    public Vector3 orbitAxis = Vector3.up; // Eje de órbita (inclínalo para variedad)

    [Header("Rotación propia")]
    public float selfRotationSpeed = 50f;

    [Header("Límite")]
    public float boundaryRadius = 24f;   // Slightly menos que el radio de la esfera (25)

    private Vector3 _currentOrbitPos;

    void Start()
    {
        if (orbitCenter == null)
            orbitCenter = GameObject.Find("Sphere")?.transform;

        // Posición inicial en la órbita
        _currentOrbitPos = transform.position - orbitCenter.position;
        if (_currentOrbitPos == Vector3.zero)
            _currentOrbitPos = Vector3.right * orbitRadius;
    }

    void Update()
    {
        // Rotar posición orbital
        _currentOrbitPos = Quaternion.AngleAxis(orbitSpeed * Time.deltaTime, orbitAxis.normalized) * _currentOrbitPos;

        // Calcular nueva posición y verificar límite
        Vector3 newPos = orbitCenter.position + _currentOrbitPos.normalized * orbitRadius;

        if (Vector3.Distance(orbitCenter.position, newPos) < boundaryRadius)
            transform.position = newPos;

        // Rotación propia
        transform.Rotate(Vector3.up, selfRotationSpeed * Time.deltaTime);
    }
}