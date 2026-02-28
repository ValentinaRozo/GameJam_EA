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

    [Header("Spawn")]
    public Transform spawnPoint;
    public float freezeDuration = 3f;

    [Header("Obstacle Avoidance")]
    public LayerMask planetLayer;
    public float avoidRadius = 3f;

    [Header("Push")]
    public float pushDamping = 4f;

    private float shootTimer = 0f;
    private float dribbleTimer = 0f;
    private bool frozen = false;
    private Vector3 externalVelocity = Vector3.zero;

    private enum State { GoBall, CarryToPlanet, Shoot }
    private State state = State.GoBall;

    void Start()
    {
        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.OnGoalScored += OnGoalScored;
    }

    void OnDisable()
    {
        GameManager.OnGoalScored -= OnGoalScored;
    }

    void OnGoalScored()
    {
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        externalVelocity = Vector3.zero;
        state = State.GoBall;
        frozen = true;
        Invoke(nameof(Unfreeze), freezeDuration);
    }

    void Unfreeze()
    {
        frozen = false;
    }

    public void ApplyPush(Vector3 force)
    {
        externalVelocity += force;
    }

    void Update()
    {
        if (frozen) return;
        if (ball == null || targetPlanet == null) return;

        // Aplicar empujes externos
        if (externalVelocity.sqrMagnitude > 0.01f)
        {
            transform.position += externalVelocity * Time.deltaTime;
            externalVelocity = Vector3.Lerp(
                externalVelocity,
                Vector3.zero,
                pushDamping * Time.deltaTime);
        }

        shootTimer -= Time.deltaTime;
        dribbleTimer -= Time.deltaTime;

        float dBall = Vector3.Distance(transform.position, ball.transform.position);
        float dPlanet = Vector3.Distance(transform.position, targetPlanet.position);

        switch (state)
        {
            case State.GoBall:
                MoveTo(ball.transform.position, grabBallDistance);
                if (dBall <= grabBallDistance)
                {
                    ball.RegisterTouch(teamID);
                    state = State.CarryToPlanet;
                }
                break;

            case State.CarryToPlanet:
                if (dBall > grabBallDistance * 3f)
                {
                    state = State.GoBall;
                    break;
                }

                MoveTo(targetPlanet.position, shootDistance);

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

            case State.Shoot:
                if (dBall <= grabBallDistance * 2f)
                {
                    Vector3 shootDir = targetPlanet.position - ball.transform.position;
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

        Vector3 dir = to.normalized;

        // Evitar planetas
        Collider[] obstacles = Physics.OverlapSphere(transform.position, avoidRadius, planetLayer);
        foreach (var obs in obstacles)
        {
            Vector3 away = transform.position - obs.transform.position;
            dir += away.normalized * 2f;
        }

        dir = dir.normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                8f * Time.deltaTime);
        }
    }

    void EnforceBoundary()
    {
        if (sphereCenter == null) return;

        Vector3 offset = transform.position - sphereCenter.position;
        if (offset.magnitude > boundaryRadius)
            transform.position = sphereCenter.position + offset.normalized * boundaryRadius;
    }
}
