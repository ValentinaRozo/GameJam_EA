using UnityEngine;

public class PlanetGoal : MonoBehaviour
{
    [Header("Configuracion")]
    [Tooltip("Equipo dueño de este planeta: A o B")]
    public string ownerTeam = "A";

    // Detección por colisión física (la pelota golpea el planeta)
    void OnCollisionEnter(Collision collision)
    {
        CheckGoal(collision.gameObject);
    }

    // Detección por trigger (por si se usa GoalZone hijo)
    void OnTriggerEnter(Collider other)
    {
        CheckGoal(other.gameObject);
    }

    void CheckGoal(GameObject obj)
    {
        BallPhysics ball = obj.GetComponent<BallPhysics>();
        if (ball == null) return;

        string scorer = ball.lastToucherTeam;
        if (scorer == "" || scorer == ownerTeam) return;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager not found in scene.");
            return;
        }

        if (scorer == "A") GameManager.Instance.TeamAScored();
        else if (scorer == "B") GameManager.Instance.TeamBScored();

        ball.lastToucherTeam = "";
    }
}
