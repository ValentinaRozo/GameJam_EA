using UnityEngine;

public class TeamAI : MonoBehaviour
{
    [Header("Team")]
    [Tooltip("Team identifier: A or B")]
    public string teamID = "B";

    [Header("References")]
    public BallPhysics ball;
    [Tooltip("The opponent planet to attack")]
    public Transform targetPlanet;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float grabBallDistance = 1.5f;
    public float shootDistance = 6f;

    [Header("Shooting")]
    public float shootForce = 10f;
    public float shootCooldown = 1.5f;

    [Header("Dribble")]
    public float dribbleForce = 3f;
    public float dribbleCooldown = 0.4f;

    [Header("Sphere Boundary")]
    public Transform sphereCenter;
    public float boundaryRadius = 23f;

    private float shootTimer = 0f;
    private float dribbleTimer = 0f;

    private enum State { GoBall, CarryToPlanet, Shoot }
    private State state = State.GoBall;

    void Start()
    {
        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;
    }

    void Update()
    {
        if (ball == null || targetPlanet == null) return;

        shootTimer -= Time.deltaTime;
        dribbleTimer -= Time.deltaTime;

        float dBall = Vector3.Distance(transform.position, ball.transform.position);
        float dPlanet = Vector3.Distance(transform.position, targetPlanet.position);

        switch (state)
        {
            // State 1: Go pick up the ball
            case State.GoBall:
                MoveTo(ball.transform.position, grabBallDistance);

                if (dBall <= grabBallDistance)
                {
                    ball.RegisterTouch(teamID);
                    state = State.CarryToPlanet;
                }
                break;

            // State 2: Carry the ball toward the enemy planet
            case State.CarryToPlanet:
                // Ball stolen or lost
                if (dBall > grabBallDistance * 3f)
                {
                    state = State.GoBall;
                    break;
                }

                MoveTo(targetPlanet.position, shootDistance);

                // Dribble: nudge ball toward planet on a cooldown to avoid velocity reset spam
                if (dBall <= grabBallDistance && dribbleTimer <= 0f)
                {
                    Vector3 toPlanet = (targetPlanet.position - ball.transform.position).normalized;
                    ball.RegisterTouch(teamID);
                    ball.Impulse(toPlanet, dribbleForce);
                    dribbleTimer = dribbleCooldown;
                }

                if (dPlanet <= shootDistance && shootTimer <= 0f)
                    state = State.Shoot;

                break;

            // State 3: Shoot at the planet
            case State.Shoot:
                if (dBall <= grabBallDistance * 2f)
                {
                    Vector3 shootDir = (targetPlanet.position - ball.transform.position);
                    ball.RegisterTouch(teamID);
                    ball.Impulse(shootDir, shootForce);
                    shootTimer = shootCooldown;
                }
                state = State.GoBall;
                break;
        }
    }

    void LateUpdate()
    {
        EnforceBoundary();
    }

    void MoveTo(Vector3 target, float stopDist)
    {
        Vector3 to = target - transform.position;
        float dist = to.magnitude;
        if (dist <= stopDist) return;

        transform.position += to.normalized * moveSpeed * Time.deltaTime;

        if (to != Vector3.zero)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(to),
                8f * Time.deltaTime);
    }

    void EnforceBoundary()
    {
        if (sphereCenter == null) return;
        Vector3 offset = transform.position - sphereCenter.position;
        if (offset.magnitude > boundaryRadius)
            transform.position = sphereCenter.position + offset.normalized * boundaryRadius;
    }
}