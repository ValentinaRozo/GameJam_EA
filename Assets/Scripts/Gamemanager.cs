
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Marcador")]
    public int scoreTeamA = 0;
    public int scoreTeamB = 0;

    [Header("Referencias para reset")]
    public BallPhysics ball;
    public Transform ballSpawnPoint;

    public Transform playerSpawn;
    public Transform teamAISpawn;

    public Transform playerObject;
    public Transform teamAIObject;

    [Header("Pausa tras gol (segundos)")]
    public float resetDelay = 2f;

    private bool resetting = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void TeamAScored()
    {
        if (resetting) return;
        scoreTeamA++;
        Debug.Log("GOOL Equipo A! | A:" + scoreTeamA + " - B:" + scoreTeamB);
        StartCoroutine(ResetAfterGoal());
    }

    public void TeamBScored()
    {
        if (resetting) return;
        scoreTeamB++;
        Debug.Log("GOOL Equipo B! | A:" + scoreTeamA + " - B:" + scoreTeamB);
        StartCoroutine(ResetAfterGoal());
    }

    IEnumerator ResetAfterGoal()
    {
        resetting = true;

        // Freeze ball
        if (ball != null)
        {
            ball.rb.velocity = Vector3.zero;
            ball.rb.angularVelocity = Vector3.zero;
        }

        yield return new WaitForSeconds(resetDelay);

        // Reposition ball
        if (ball != null && ballSpawnPoint != null)
        {
            ball.transform.position = ballSpawnPoint.position;
            ball.rb.velocity = Vector3.zero;
            ball.rb.angularVelocity = Vector3.zero;
            ball.lastToucherTeam = "";
        }

        // Reposition player
        if (playerObject != null && playerSpawn != null)
        {
            // CharacterController must be disabled to teleport
            CharacterController cc = playerObject.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            playerObject.position = playerSpawn.position;
            if (cc != null) cc.enabled = true;
        }

        // Reposition AI
        if (teamAIObject != null && teamAISpawn != null)
            teamAIObject.position = teamAISpawn.position;

        resetting = false;
    }
}