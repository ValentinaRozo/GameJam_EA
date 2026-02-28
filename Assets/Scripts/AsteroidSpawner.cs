using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject asteroidPrefab;

    [Header("Spawn")]
    public int asteroidCount = 30;
    public float spawnRadius = 20f;       // Radio dentro del cual spawnean
    public Transform sphereCenter;

    [Header("Tamaño variado")]
    public float minScale = 0.3f;
    public float maxScale = 1.5f;

    void Start()
    {
        if (sphereCenter == null)
            sphereCenter = GameObject.Find("Sphere")?.transform;

        for (int i = 0; i < asteroidCount; i++)
            SpawnAsteroid();
    }

    void SpawnAsteroid()
    {
        // Posición aleatoria dentro de la esfera, alejada de los planetas
        Vector3 spawnPos = sphereCenter.position + Random.insideUnitSphere * spawnRadius;
        GameObject asteroid = Instantiate(asteroidPrefab, spawnPos, Random.rotation);

        // Tamaño aleatorio
        float s = Random.Range(minScale, maxScale);
        asteroid.transform.localScale = Vector3.one * s;

        // Asignar masa proporcional al tamaño
        Rigidbody rb = asteroid.GetComponent<Rigidbody>();
        if (rb != null) rb.mass = s * s; // masa ~ volumen aproximado

        // Asignar referencia al centro
        Asteroid asteroidScript = asteroid.GetComponent<Asteroid>();
        if (asteroidScript != null) asteroidScript.sphereCenter = sphereCenter;
    }
}