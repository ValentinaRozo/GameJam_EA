using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Asteroid : MonoBehaviour
{
    [Header("Movimiento inicial")]
    public float initialSpeedMin = 2f;
    public float initialSpeedMax = 6f;
    public float rotationSpeedMax = 90f;

    [Header("Límite")]
    public Transform sphereCenter;
    public float boundaryRadius = 24f;

    [Header("Física")]
    public float bounceDamping = 0.85f;

    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.drag = 0f;
        _rb.angularDrag = 0.1f;

        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;

        _rb.velocity = Random.onUnitSphere * Random.Range(initialSpeedMin, initialSpeedMax);
        _rb.angularVelocity = Random.insideUnitSphere * rotationSpeedMax * Mathf.Deg2Rad;
    }

    void FixedUpdate()
    {
        EnforceBoundary();
    }

    void EnforceBoundary()
    {
        Vector3 fromCenter = transform.position - sphereCenter.position;
        float dist = fromCenter.magnitude;

        if (dist >= boundaryRadius)
        {
            transform.position = sphereCenter.position + fromCenter.normalized * (boundaryRadius - 0.2f);

            Vector3 normal = -fromCenter.normalized;
            _rb.velocity = Vector3.Reflect(_rb.velocity, normal) * bounceDamping;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        float impactForce = col.relativeVelocity.magnitude;

        if (impactForce > 3f)
        {
            // Aquí puedes añadir: partículas, sonido, etc.
        }
    }
}