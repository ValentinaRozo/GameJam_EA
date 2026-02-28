using UnityEngine;

public class PlanetGoal : MonoBehaviour
{
    [Header("Configuracion")]
    [Tooltip("Equipo dueno de este planeta: A o B")]
    public string ownerTeam = "A";

    void OnTriggerEnter(Collider other)
    {
        BallPhysics ball = other.GetComponent<BallPhysics>();
        if (ball == null) return;

        string scorer = ball.lastToucherTeam;

        // Only scores if the ball was last touched by the opposing team
        if (scorer == "" || scorer == ownerTeam) return;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager not found in scene.");
            return;
        }

        if (scorer == "A")
            GameManager.Instance.TeamAScored();
        else if (scorer == "B")
            GameManager.Instance.TeamBScored();

        // Reset to avoid double counting
        ball.lastToucherTeam = "";
    }
}