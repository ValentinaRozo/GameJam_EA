using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject asteroidPrefab;

    [Header("Spawn")]
    public int asteroidCount = 30;
    public float spawnRadius = 20f;
    public Transform sphereCenter;

    [Header("Tamaño variado")]
    public float minScale = 0.3f;
    public float maxScale = 1.5f;

    private AsteroidDifficultyManager diffManager;

    void Start()
    {
        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;

        diffManager = GetComponent<AsteroidDifficultyManager>();

        for (int i = 0; i < asteroidCount; i++)
            SpawnAsteroid();
    }

    void SpawnAsteroid()
    {
        Vector3 spawnPos = sphereCenter.position + Random.insideUnitSphere * spawnRadius;
        GameObject asteroid = Instantiate(asteroidPrefab, spawnPos, Random.rotation);
        asteroid.tag = "Asteroid";

        // Escala base aleatoria
        float baseScale = Random.Range(minScale, maxScale);
        asteroid.transform.localScale = Vector3.one * baseScale;

        Rigidbody rb = asteroid.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.mass = baseScale * baseScale;
        }

        // Asignar referencia al centro
        Asteroid asteroidScript = asteroid.GetComponent<Asteroid>();
        if (asteroidScript != null)
            asteroidScript.sphereCenter = sphereCenter;

        //  NUEVO: aplicar dificultad actual al nuevo asteroide
        if (diffManager != null)
            diffManager.InitAsteroid(asteroid, baseScale);
    }
}
