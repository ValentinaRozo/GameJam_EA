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
        // Unfreeze inmediato al inicio
        UnfreezeAll();
    }

    public void RespawnAfterGoal()
    {
        // Desactivar CharacterController del jugador para teletransportar
        PlayerSpaceController player = FindObjectOfType<PlayerSpaceController>();
        CharacterController cc = player?.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        RandomizeAllSpawns();

        if (cc != null) cc.enabled = true;

        // Descongelar tras freeze duration
        Invoke(nameof(UnfreezeAll), freezeDuration);
    }

    void RandomizeAllSpawns()
    {
        if (sphereCenter == null) return;

        // Pelota al centro
        if (ballSpawn != null)
        {
            ballSpawn.position = sphereCenter.position;
            // Mover la pelota real
            BallPhysics ball = FindObjectOfType<BallPhysics>();
            if (ball != null)
            {
                Rigidbody rb = ball.GetComponent<Rigidbody>();
                if (rb != null) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
                ball.transform.position = sphereCenter.position;
            }
        }

        // Recolectar todos los agentes: 1 jugador + todas las AIs
        List<Transform> agents = new List<Transform>();

        PlayerSpaceController playerCtrl = FindObjectOfType<PlayerSpaceController>();
        if (playerCtrl != null) agents.Add(playerCtrl.transform);

        TeamAI[] ais = FindObjectsOfType<TeamAI>();
        foreach (var ai in ais) agents.Add(ai.transform);

        // Generar posiciones
        List<Vector3> positions = GenerateValidPositions(agents.Count);

        // Teletransportar cada agente
        for (int i = 0; i < agents.Count && i < positions.Count; i++)
            agents[i].position = positions[i];
    }

    void UnfreezeAll()
    {
        PlayerSpaceController player = FindObjectOfType<PlayerSpaceController>();
        if (player != null) player.Unfreeze();

        TeamAI[] ais = FindObjectsOfType<TeamAI>();
        foreach (var ai in ais) ai.Unfreeze();
    }

    List<Vector3> GenerateValidPositions(int count)
    {
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> occupied = new List<Vector3>();
        int maxAttempts = count * 30;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 candidate = sphereCenter.position +
                                Random.onUnitSphere * boundaryRadius * 0.82f;

            bool tooClose = false;
            foreach (Vector3 occ in occupied)
            {
                if (Vector3.Distance(candidate, occ) < minSeparation)
                { tooClose = true; break; }
            }

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
