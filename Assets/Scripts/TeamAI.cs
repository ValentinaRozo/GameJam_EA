using System.Collections.Generic;
using UnityEngine;

public class TeamAI : MonoBehaviour
{
    [Header("Team")]
    [Tooltip("Team identifier: A or B")]
    public string teamID = "B";

    [Header("References")]
    public BallPhysics ball;
    public Transform targetPlanet;
    public Transform ownPlanet;

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

    [Header("Defense")]
    public float defendTriggerRadius = 12f;
    public float defendPushDistance = 2.5f;

    [Header("Pickup Seeking")]
    public float pickupDetectRadius = 15f;

    [Header("Separation")]
    public float separationRadius = 2.5f;
    public float separationForce = 3f;

    [Header("Empuje a jugadores cercanos")]
    public float contactPushRadius = 1.5f;
    public float contactPushForce = 10f;
    public float contactCooldown = 0.3f;

    [Header("Sphere Boundary")]
    public Transform sphereCenter;
    public float boundaryRadius = 23f;

    [Header("Obstacle Avoidance")]
    public LayerMask planetLayer;
    public float avoidRadius = 3f;

    [Header("Push")]
    public float pushDamping = 4f;

    private static List<TeamAI> allAIs = new List<TeamAI>();

    private float shootTimer = 0f;
    private float dribbleTimer = 0f;
    private float stateEvalTimer = 0f;
    private float contactTimer = 0f;
    public bool frozen = false;
    private Vector3 externalVelocity = Vector3.zero;
    private Transform currentPickup = null;

    private enum State { GoBall, CarryToPlanet, Shoot, Defend, SeekPickup }
    private State state = State.GoBall;

    void OnEnable()
    {
        if (!allAIs.Contains(this)) allAIs.Add(this);
        GameManager.OnGoalScored += OnGoalScored;
    }

    void OnDisable()
    {
        allAIs.Remove(this);
        GameManager.OnGoalScored -= OnGoalScored;
    }

    void Start()
    {
        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;
    }

    void OnGoalScored()
    {
        externalVelocity = Vector3.zero;
        currentPickup = null;
        state = State.GoBall;
        frozen = true;
        CancelInvoke(nameof(Unfreeze));
        Invoke(nameof(Unfreeze), 4f);
    }

    public void Unfreeze()
    {
        CancelInvoke(nameof(Unfreeze));
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

        // Empujes externos
        if (externalVelocity.sqrMagnitude > 0.01f)
        {
            transform.position += externalVelocity * Time.deltaTime;
            externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero,
                                            pushDamping * Time.deltaTime);
        }

        shootTimer -= Time.deltaTime;
        dribbleTimer -= Time.deltaTime;
        stateEvalTimer -= Time.deltaTime;
        contactTimer -= Time.deltaTime;

        if (contactTimer <= 0f) CheckContactPush();

        if (stateEvalTimer <= 0f)
        {
            EvaluateState();
            stateEvalTimer = 0.5f;
        }

        ExecuteState();
    }

    // ─── Empuje por contacto ──────────────────────────────────────────────────

    void CheckContactPush()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, contactPushRadius);
        foreach (var col in hits)
        {
            if (col.gameObject == gameObject) continue;

            // Empujar jugador humano
            PlayerSpaceController player = col.GetComponent<PlayerSpaceController>();
            if (player != null)
            {
                Vector3 dir = (col.transform.position - transform.position).normalized;
                if (dir == Vector3.zero) dir = Random.onUnitSphere;
                player.ApplyPush(dir * contactPushForce);
                contactTimer = contactCooldown;
                return;
            }

            // Empujar otra IA
            TeamAI otherAI = col.GetComponent<TeamAI>();
            if (otherAI != null && otherAI != this)
            {
                Vector3 dir = (col.transform.position - transform.position).normalized;
                if (dir == Vector3.zero) dir = Random.onUnitSphere;
                otherAI.ApplyPush(dir * contactPushForce);
                contactTimer = contactCooldown;
            }
        }
    }

    // ─── Máquina de estados ───────────────────────────────────────────────────

    void EvaluateState()
    {
        if (BallThreateningOwnPlanet())
        {
            state = State.Defend;
            return;
        }

        if (state == State.Defend && !BallThreateningOwnPlanet())
        {
            state = State.GoBall;
            return;
        }

        if (state == State.SeekPickup && currentPickup == null)
        {
            state = State.GoBall;
            return;
        }

        if (state == State.GoBall)
        {
            Transform pickup = FindNearestPickup();
            if (pickup != null)
            {
                currentPickup = pickup;
                state = State.SeekPickup;
            }
        }
    }

    void ExecuteState()
    {
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
                if (dBall > grabBallDistance * 3f) { state = State.GoBall; break; }

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
                    Vector3 dir = targetPlanet.position - ball.transform.position;
                    ball.RegisterTouch(teamID);
                    ball.Impulse(dir, shootForce);
                    shootTimer = shootCooldown;
                }
                state = State.GoBall;
                break;

            case State.Defend:
                Defend(dBall);
                break;

            case State.SeekPickup:
                if (currentPickup == null) { state = State.GoBall; break; }
                if (dBall < grabBallDistance * 2f) { state = State.GoBall; break; }
                MoveTo(currentPickup.position, grabBallDistance * 0.8f);
                break;
        }
    }

    // ─── Defensa ──────────────────────────────────────────────────────────────

    bool BallThreateningOwnPlanet()
    {
        if (ownPlanet == null || ball == null) return false;
        return Vector3.Distance(ball.transform.position, ownPlanet.position) < defendTriggerRadius;
    }

    void Defend(float dBall)
    {
        if (ownPlanet == null) return;

        Vector3 interceptDir = (ownPlanet.position - ball.transform.position).normalized;
        Vector3 interceptPos = ball.transform.position + interceptDir * defendPushDistance * 0.5f;
        MoveTo(interceptPos, defendPushDistance);

        if (dBall <= defendPushDistance && shootTimer <= 0f)
        {
            Vector3 pushDir = (ball.transform.position - ownPlanet.position).normalized;
            ball.RegisterTouch(teamID);
            ball.Impulse(pushDir, shootForce * 1.2f);
            shootTimer = shootCooldown;
            state = State.GoBall;
        }
    }

    // ─── Comodines ────────────────────────────────────────────────────────────

    Transform FindNearestPickup()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, pickupDetectRadius);
        Transform nearest = null;
        float nearestDist = pickupDetectRadius;

        foreach (var col in cols)
        {
            if (col.GetComponent<PowerUpPickup>() == null) continue;
            float d = Vector3.Distance(transform.position, col.transform.position);
            if (d < nearestDist) { nearestDist = d; nearest = col.transform; }
        }
        return nearest;
    }

    // ─── Movimiento ───────────────────────────────────────────────────────────

    void LateUpdate() { EnforceBoundary(); }

    void MoveTo(Vector3 target, float stopDist)
    {
        Vector3 to = target - transform.position;
        if (to.magnitude <= stopDist) return;

        Vector3 dir = to.normalized;

        // Evasión de planetas
        Collider[] obstacles = Physics.OverlapSphere(transform.position, avoidRadius, planetLayer);
        foreach (var obs in obstacles)
        {
            Vector3 away = transform.position - obs.transform.position;
            dir += away.normalized * 2f;
        }

        // Separación entre agentes
        foreach (var other in allAIs)
        {
            if (other == this) continue;
            float d = Vector3.Distance(transform.position, other.transform.position);
            if (d < separationRadius && d > 0.01f)
            {
                Vector3 away = (transform.position - other.transform.position).normalized;
                dir += away * (separationForce / d);
            }
        }

        dir = dir.normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                   Quaternion.LookRotation(dir),
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
