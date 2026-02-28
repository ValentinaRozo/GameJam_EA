using UnityEngine;

public class TeamAI : MonoBehaviour
{
    public Transform player;
    public BallPhysics ball;

    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float grabBallDistance = 1.2f;     
    public float nearPlayerDistance = 2.0f;  

    [Header("Pase")]
    public float passForce = 6f;
    public float passCooldown = 1.0f;

    private float passTimer = 0f;

    private enum State { GoBall, BringBallToPlayer, PassToPlayer }
    private State state = State.GoBall;

    void Update()
    {
        if (player == null || ball == null) return;

        passTimer -= Time.deltaTime;

        float dBall = Vector3.Distance(transform.position, ball.transform.position);
        float dPlayer = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.GoBall:
                MoveTowards(ball.transform.position, grabBallDistance);

                if (dBall <= grabBallDistance)
                    state = State.BringBallToPlayer;
                break;

            case State.BringBallToPlayer:
                // Ir hacia el jugador
                MoveTowards(player.position, nearPlayerDistance);

                // Si la pelota se “escapó”, volver a buscarla
                if (dBall > 3.5f)
                    state = State.GoBall;

                // Si ya llegó cerca del jugador, pasa
                if (dPlayer <= nearPlayerDistance && passTimer <= 0f)
                    state = State.PassToPlayer;
                break;

            case State.PassToPlayer:
                Vector3 dir = (player.position - ball.transform.position);
                if (dir.sqrMagnitude > 0.01f)
                {
                    ball.Impulse(dir, passForce);
                }

                passTimer = passCooldown;
                state = State.GoBall;
                break;
        }
    }

    void MoveTowards(Vector3 target, float stopDist)
    {
        Vector3 to = target - transform.position;
        float dist = to.magnitude;
        if (dist <= stopDist) return;

        Vector3 step = to.normalized * moveSpeed * Time.deltaTime;
        transform.position += step;

        // Mirar hacia donde va
        if (to != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to), 8f * Time.deltaTime);
    }
}