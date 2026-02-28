using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject raquetaPrefab;
    public GameObject batePrefab;
    public GameObject balonBasketPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 8f;
    public float spawnRadius = 18f;
    public int maxActive = 2;
    public Transform sphereCenter;

    private List<GameObject> active = new List<GameObject>();
    private Queue<GameObject> spawnQueue = new Queue<GameObject>();

    void Start()
    {
        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;

        RebuildQueue();
        StartCoroutine(SpawnLoop());
    }

    void RebuildQueue()
    {
        List<GameObject> prefabs = new List<GameObject>();
        if (raquetaPrefab != null) prefabs.Add(raquetaPrefab);
        if (batePrefab != null) prefabs.Add(batePrefab);
        if (balonBasketPrefab != null) prefabs.Add(balonBasketPrefab);

        // Shuffle
        for (int i = 0; i < prefabs.Count; i++)
        {
            int rand = Random.Range(i, prefabs.Count);
            (prefabs[i], prefabs[rand]) = (prefabs[rand], prefabs[i]);
        }

        spawnQueue.Clear();
        foreach (var p in prefabs) spawnQueue.Enqueue(p);

        Debug.Log($"[PowerUpSpawner] Cola reconstruida con {spawnQueue.Count} prefabs");
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(2f);

        while (true)
        {
            Cleanup();
            if (active.Count < maxActive)
                SpawnNext();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnNext()
    {
        // Reconstruir si está vacía
        if (spawnQueue.Count == 0) RebuildQueue();

        if (spawnQueue.Count == 0)
        {
            Debug.LogWarning("[PowerUpSpawner] No hay prefabs asignados.");
            return;
        }

        GameObject prefab = spawnQueue.Dequeue();

        if (prefab == null)
        {
            SpawnNext(); // intentar con el siguiente
            return;
        }

        Vector3 center = sphereCenter ? sphereCenter.position : Vector3.zero;
        Vector3 pos = center + Random.onUnitSphere * Random.Range(spawnRadius * 0.4f, spawnRadius);

        GameObject obj = Instantiate(prefab, pos, Random.rotation);
        active.Add(obj);

        Debug.Log($"[PowerUpSpawner] {prefab.name} | Activos: {active.Count}/{maxActive}");
    }

    void Cleanup()
    {
        active.RemoveAll(p => p == null);
    }
}
