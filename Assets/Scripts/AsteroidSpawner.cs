using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject asteroidPrefab;

    [Header("Spawn")]
    public int asteroidCount = 20;
    public float spawnRadius = 20f;

    [Header("Size")]
    public float minScale = 0.4f;
    public float maxScale = 1.5f;

    [Header("Speed")]
    public float initialSpeedMin = 2f;
    public float initialSpeedMax = 6f;

    [Header("Boundary")]
    public Transform sphereCenter;
    public float boundaryRadius = 24f;

    void Start()
    {
        if (asteroidPrefab == null)
        {
            Debug.LogWarning("AsteroidSpawner: asteroidPrefab is not assigned.");
            return;
        }

        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;

        for (int i = 0; i < asteroidCount; i++)
            SpawnAsteroid();
    }

    void SpawnAsteroid()
    {
        Vector3 spawnPos = (sphereCenter != null ? sphereCenter.position : Vector3.zero)
                         + Random.insideUnitSphere * spawnRadius;

        GameObject obj = Instantiate(asteroidPrefab, spawnPos, Random.rotation);

        // Random scale
        float s = Random.Range(minScale, maxScale);
        obj.transform.localScale = Vector3.one * s;

        // Configure Rigidbody
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.drag = 0f;
            rb.angularDrag = 0.1f;
            rb.mass = s * s;
            rb.velocity = Random.onUnitSphere * Random.Range(initialSpeedMin, initialSpeedMax);
            rb.angularVelocity = Random.insideUnitSphere * 90f * Mathf.Deg2Rad;
        }

        // Pass boundary reference to Asteroid script if present
        Asteroid asteroidScript = obj.GetComponent<Asteroid>();
        if (asteroidScript != null)
        {
            asteroidScript.sphereCenter = sphereCenter;
            asteroidScript.boundaryRadius = boundaryRadius;
            asteroidScript.initialSpeedMin = initialSpeedMin;
            asteroidScript.initialSpeedMax = initialSpeedMax;
        }
    }
}