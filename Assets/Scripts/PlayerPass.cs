using UnityEngine;

public class PlayerPass : MonoBehaviour
{
    public BallPhysics ball;
    public Transform teammate;

    public float passRange = 2f;
    public float passForce = 6f;

    private PlayerSpaceController playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerSpaceController>();
    }

    void Update()
    {
        // Dash toma prioridad sobre el pase cuando está activo
        if (GetComponent<PlayerDashEffect>() != null) return;

        if (Input.GetKeyDown(KeyCode.Space))
            TryPassToTeammate();
    }

    void TryPassToTeammate()
    {
        if (ball == null || teammate == null) return;

        if (Vector3.Distance(transform.position, ball.transform.position) > passRange)
            return;

        Vector3 dir = teammate.position - ball.transform.position;
        if (dir.sqrMagnitude < 0.01f) return;

        if (playerController != null)
            ball.RegisterTouch(playerController.teamID);

        ball.Impulse(dir, passForce);
    }
}