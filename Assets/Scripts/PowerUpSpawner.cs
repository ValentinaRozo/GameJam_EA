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

    void Start()
    {
        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            active.RemoveAll(p => p == null);
            if (active.Count < maxActive) SpawnRandom();
        }
    }

    void SpawnRandom()
    {
        GameObject[] pool = { raquetaPrefab, batePrefab, balonBasketPrefab };
        // Filtrar nulos
        List<GameObject> valid = new List<GameObject>();
        foreach (var p in pool) if (p != null) valid.Add(p);
        if (valid.Count == 0) return;

        Vector3 center = sphereCenter ? sphereCenter.position : Vector3.zero;
        Vector3 pos = center + Random.insideUnitSphere.normalized
                         * Random.Range(spawnRadius * 0.3f, spawnRadius);

        GameObject obj = Instantiate(valid[Random.Range(0, valid.Count)], pos, Random.rotation);
        active.Add(obj);
    }
}
