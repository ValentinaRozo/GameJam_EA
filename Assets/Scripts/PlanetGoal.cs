using UnityEngine;

public class PlanetGoal : MonoBehaviour
{
    [Header("Config")]
    public string ownerTeam = "A";
    public float goalRadius = 3f;

    private BallPhysics ball;
    private bool ballWasInside = false;

    void Start()
    {
        ball = FindObjectOfType<BallPhysics>();
        if (ball == null)
            Debug.LogWarning("[PlanetGoal] BallPhysics not found in scene.");
    }

    void Update()
    {
        if (ball == null || GameManager.Instance == null || GameManager.Instance.gameEnded) return;

        float dist = Vector3.Distance(transform.position, ball.transform.position);
        bool isInside = dist <= goalRadius;

        if (isInside && !ballWasInside)
        {
            ballWasInside = true;
            CheckGoal();
        }
        else if (!isInside)
        {
            ballWasInside = false;
        }
    }

    void CheckGoal()
    {
        string scorer = ball.lastToucherTeam;
        if (string.IsNullOrEmpty(scorer) || scorer == ownerTeam) return;

        Debug.Log("[PlanetGoal] Goal! Team " + scorer + " scored on planet " + ownerTeam);

        if (scorer == "A") GameManager.Instance.TeamAScored();
        else if (scorer == "B") GameManager.Instance.TeamBScored();

        ball.lastToucherTeam = "";
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, goalRadius);
    }
}
