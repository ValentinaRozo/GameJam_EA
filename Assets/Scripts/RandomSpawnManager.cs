using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawnManager : MonoBehaviour
{
    [Header("Referencias")]
    public Transform ballSpawn;

    [Header("Settings")]
    public Transform sphereCenter;
    public float boundaryRadius = 23f;
    public float minSeparation = 8f;
    public float freezeDuration = 3f;

    void Start()
    {
        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;

        RandomizeAllSpawns();
        UnfreezeAll();
    }

    public void RespawnAfterGoal()
    {
        // Cancelar cualquier Invoke pendiente para no apilar unfreezes
        CancelInvoke(nameof(UnfreezeAll));
        StopAllCoroutines();

        // 1. Congelar a todos inmediatamente
        FreezeAll();

        // 2. Limpiar PowerUps de la escena
        CleanupPowerUps();

        // 3. Teletransportar
        PlayerSpaceController player = FindObjectOfType<PlayerSpaceController>();
        CharacterController cc = player?.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        RandomizeAllSpawns();

        if (cc != null) cc.enabled = true;

        // 4. Descongelar tras delay
        Invoke(nameof(UnfreezeAll), freezeDuration);
    }

    void FreezeAll()
    {
        PlayerSpaceController player = FindObjectOfType<PlayerSpaceController>();
        if (player != null) player.Freeze();

        foreach (TeamAI ai in FindObjectsOfType<TeamAI>())
            ai.Freeze();
    }

    void UnfreezeAll()
    {
        PlayerSpaceController player = FindObjectOfType<PlayerSpaceController>();
        if (player != null) player.Unfreeze();

        foreach (TeamAI ai in FindObjectsOfType<TeamAI>())
            ai.Unfreeze();
    }

    void CleanupPowerUps()
    {
        // Destruir todos los PowerUps activos en escena
        foreach (var pickup in FindObjectsOfType<PowerUpPickup>())
            Destroy(pickup.gameObject);

        // Resetear el spawner para que empiece limpio
        PowerUpSpawner spawner = FindObjectOfType<PowerUpSpawner>();
        if (spawner != null) spawner.ResetSpawner();
    }

    void RandomizeAllSpawns()
    {
        if (sphereCenter == null) return;

        // Pelota al centro con velocidad 0
        BallPhysics ball = FindObjectOfType<BallPhysics>();
        if (ball != null)
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
            ball.transform.position = sphereCenter.position;
            ball.lastToucherTeam = ""; // reset para evitar gol doble
        }

        // Agentes
        List<Transform> agents = new List<Transform>();
        PlayerSpaceController playerCtrl = FindObjectOfType<PlayerSpaceController>();
        if (playerCtrl != null) agents.Add(playerCtrl.transform);
        foreach (TeamAI ai in FindObjectsOfType<TeamAI>()) agents.Add(ai.transform);

        List<Vector3> positions = GenerateValidPositions(agents.Count);
        for (int i = 0; i < agents.Count && i < positions.Count; i++)
            agents[i].position = positions[i];
    }

    List<Vector3> GenerateValidPositions(int count)
    {
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> occupied = new List<Vector3>();

        for (int attempt = 0; attempt < count * 30; attempt++)
        {
            Vector3 candidate = sphereCenter.position + Random.onUnitSphere * boundaryRadius * 0.82f;
            bool tooClose = false;
            foreach (Vector3 occ in occupied)
                if (Vector3.Distance(candidate, occ) < minSeparation) { tooClose = true; break; }

            if (!tooClose)
            {
                positions.Add(candidate);
                occupied.Add(candidate);
                if (positions.Count >= count) break;
            }
        }
        return positions;
    }
}
