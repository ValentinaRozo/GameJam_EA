using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TeamAI : MonoBehaviour
{
    public Transform player;
    public Transform ball;
    public BallOwnership ballOwnership;

    [Header("Movimiento")]
    public float speed = 6f;
    public float stopDistanceToBall = 1.4f;
    public float supportDistanceToPlayer = 3.5f;

    [Header("Pase (empuje)")]
    public float passForce = 8f;
    public float passCooldown = 1.2f;

    private CharacterController controller;
    private float nextPassTime;

    private enum State { ChaseBall, SupportPlayer }
    private State state;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (player == null || ball == null || ballOwnership == null) return;

        // --- Decidir estado ---
        bool playerHasBall = ballOwnership.IsHeldBy(player);
        bool iHaveBall = ballOwnership.IsHeldBy(transform);

        if (playerHasBall)
            state = State.SupportPlayer;
        else
            state = State.ChaseBall;

        // --- Ejecutar estado ---
        if (state == State.ChaseBall)
            ChaseBall(iHaveBall);
        else
            SupportPlayer();
    }

    void ChaseBall(bool iHaveBall)
    {
        float d = Vector3.Distance(transform.position, ball.position);

        // Si llegué a la pelota, la “tomo” y decido pasar
        if (d <= stopDistanceToBall)
        {
            ballOwnership.SetHolder(transform);

            // Intentar pase al jugador (solo si cooldown y el jugador no está pegado)
            if (Time.time >= nextPassTime)
            {
                float dp = Vector3.Distance(player.position, transform.position);
                if (dp > 2f)
                {
                    PassToPlayer();
                    nextPassTime = Time.time + passCooldown;

                    // Después del pase, dejo de “tenerla”
                    ballOwnership.ClearHolder(transform);
                }
            }

            // No seguir empujando infinito: me quedo cerca
            return;
        }

        // Si no he llegado, me muevo hacia la pelota
        MoveTowards(ball.position);
    }

    void SupportPlayer()
    {
        // Me ubico cerca del jugador, no encima.
        Vector3 toMe = (transform.position - player.position);
        Vector3 offsetDir = toMe.sqrMagnitude < 0.01f ? player.right : toMe.normalized;
        Vector3 target = player.position + offsetDir * supportDistanceToPlayer;

        MoveTowards(target);
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 dir = (target - transform.position);
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.05f) return;

        dir.Normalize();
        controller.Move(dir * speed * Time.deltaTime);

        // opcional: mirar hacia donde va
        if (dir.sqrMagnitude > 0.001f)
            transform.forward = Vector3.Lerp(transform.forward, dir, 10f * Time.deltaTime);
    }

    void PassToPlayer()
    {
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 dir = (player.position - ball.position).normalized;

        // limpia velocidad para que el pase sea consistente
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(dir * passForce, ForceMode.VelocityChange);
    }
}