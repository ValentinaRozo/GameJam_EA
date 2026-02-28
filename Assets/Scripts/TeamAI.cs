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
    public float moveSpeed = 6f;
    public float grabBallDistance = 1.8f;
    public float shootDistance = 7f;

    [Header("Shooting")]
    public float shootForce = 12f;
    public float shootCooldown = 1.2f;

    [Header("Dribble")]
    public float dribbleForce = 4f;
    public float dribbleCooldown = 0.3f;

    [Header("Defense")]
    public float defendTriggerRadius = 8f;
    [Range(0f, 1f)] public float defendApproachThreshold = 0.2f;
    public float defendPushDistance = 2.5f;

    [Header("Pickup Seeking")]
    public float pickupDetectRadius = 15f;

    [Header("Separation entre agentes")]
    public float separationRadius = 2.5f;
    public float separationForce = 3f;

    [Header("Empuje por contacto")]
    public float contactPushRadius = 1.5f;
    public float contactPushForce = 10f;
    public float contactCooldown = 0.3f;

    [Header("Obstacle Avoidance")]
    public LayerMask planetLayer;
    public float avoidRadius = 3f;

    [Header("Push")]
    public float pushDamping = 5f;

    [Header("Sphere Boundary")]
    public Transform sphereCenter;
    public float boundaryRadius = 23f;

    private static readonly List<TeamAI> allAIs = new List<TeamAI>();

    private float shootTimer = 0f;
    private float dribbleTimer = 0f;
    private float stateEvalTimer = 0f;
    private float contactTimer = 0f;
    private bool frozen = false;

    private Vector3 externalVelocity = Vector3.zero;
    private Transform currentPickup = null;

    private float baseMoveSpeed;
    private float baseShootForce;
    private float baseDribbleForce;
    private float baseContactPushForce;

    private enum State { GoBall, CarryToPlanet, Shoot, Defend, SeekPickup }
    private State state = State.GoBall;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    void Awake()
    {
        // Guardar antes de que cualquier otro script los modifique
        baseMoveSpeed = moveSpeed;
        baseShootForce = shootForce;
        baseDribbleForce = dribbleForce;
        baseContactPushForce = contactPushForce;
    }

    void OnEnable() { if (!allAIs.Contains(this)) allAIs.Add(this); }
    void OnDisable() { allAIs.Remove(this); }

    void Start()
    {
        if (sphereCenter == null)
            sphereCenter = GameObject.Find("SceneSphere")?.transform;

        if (ball == null)
            ball = FindObjectOfType<BallPhysics>();

        if (targetPlanet == null)
        {
            string name = teamID == "A" ? "PlanetB" : "PlanetA";
            GameObject go = GameObject.Find(name);
            if (go != null) targetPlanet = go.transform;
        }

        if (ownPlanet == null)
        {
            string name = teamID == "A" ? "PlanetA" : "PlanetB";
            GameObject go = GameObject.Find(name);
            if (go != null) ownPlanet = go.transform;
            else
            {
                PlanetGoal[] planets = FindObjectsOfType<PlanetGoal>();
                foreach (var p in planets)
                    if (p.transform != targetPlanet) { ownPlanet = p.transform; break; }
            }
        }

        frozen = false;
    }

    // ─── Freeze ───────────────────────────────────────────────────────────────

    public void Freeze()
    {
        frozen = true;
        externalVelocity = Vector3.zero;
        currentPickup = null;
        state = State.GoBall;
    }

    public void Unfreeze() { frozen = false; }
    public void ApplyPush(Vector3 force) { externalVelocity += force; }

    // ─── Field Effects ────────────────────────────────────────────────────────

    public void EnableDash()
    {
        moveSpeed = baseMoveSpeed * 1.5f;
        contactPushForce = baseContactPushForce * 1.5f;
        Debug.Log($"[TeamAI] Dash ON — speed: {moveSpeed}");
    }

    public void DisableDash()
    {
        moveSpeed = baseMoveSpeed;
        contactPushForce = baseContactPushForce;
        Debug.Log("[TeamAI] Dash OFF");
    }

    public void EnableHeavyBall()
    {
        shootForce = baseShootForce * 2f;
        dribbleForce = baseDribbleForce * 2f;
        Debug.Log($"[TeamAI] HeavyBall ON — shoot: {shootForce}");
    }

    public void DisableHeavyBall()
    {
        shootForce = baseShootForce;
        dribbleForce = baseDribbleForce;
        Debug.Log("[TeamAI] HeavyBall OFF");
    }

    // ─── Update ───────────────────────────────────────────────────────────────

    void Update()
    {
        if (frozen) return;
        if (ball == null || targetPlanet == null) return;

        float dt = Time.deltaTime;

        if (externalVelocity.sqrMagnitude > 0.01f)
        {
            transform.position += externalVelocity * dt;
            externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, pushDamping * dt);
        }

        shootTimer -= dt;
        dribbleTimer -= dt;
        stateEvalTimer -= dt;
        contactTimer -= dt;

        if (contactTimer <= 0f) CheckContactPush();
        if (stateEvalTimer <= 0f) { EvaluateState(); stateEvalTimer = 0.5f; }

        ExecuteState();
    }

    // ─── Máquina de estados ───────────────────────────────────────────────────

    void EvaluateState()
    {
        if (BallThreateningOwnPlanet()) { state = State.Defend; return; }
        if (state == State.Defend) { state = State.GoBall; return; }
        if (state == State.SeekPickup && currentPickup == null) { state = State.GoBall; return; }

        if (state == State.GoBall)
        {
            Transform pickup = FindNearestPickup();
            if (pickup != null) { currentPickup = pickup; state = State.SeekPickup; }
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
                { ball.RegisterTouch(teamID); state = State.CarryToPlanet; }
                break;

            case State.CarryToPlanet:
                if (dBall > grabBallDistance * 3f) { state = State.GoBall; break; }
                MoveTo(targetPlanet.position, shootDistance);
                if (dBall <= grabBallDistance && dribbleTimer <= 0f)
                {
                    ball.RegisterTouch(teamID);
                    ball.Impulse((targetPlanet.position - ball.transform.position).normalized, dribbleForce);
                    dribbleTimer = dribbleCooldown;
                }
                if (dPlanet <= shootDistance && shootTimer <= 0f) state = State.Shoot;
                break;

            case State.Shoot:
                if (dBall > grabBallDistance * 3f) { state = State.GoBall; break; }
                ball.RegisterTouch(teamID);
                ball.Impulse(targetPlanet.position - ball.transform.position, shootForce);
                shootTimer = shootCooldown;
                state = State.GoBall;
                break;

            case State.Defend:
                Defend(dBall);
                break;

            case State.SeekPickup:
                if (currentPickup == null) { state = State.GoBall; break; }
                if (dBall < grabBallDistance * 2f) { state = State.GoBall; break; }
                if (Vector3.Distance(transform.position, currentPickup.position) > pickupDetectRadius * 1.5f)
                { currentPickup = null; state = State.GoBall; break; }
                MoveTo(currentPickup.position, grabBallDistance * 0.8f);
                break;
        }
    }

    // ─── Defensa ──────────────────────────────────────────────────────────────

    bool BallThreateningOwnPlanet()
    {
        if (ownPlanet == null || ball == null || ball.rb == null) return false;
        float dist = Vector3.Distance(ball.transform.position, ownPlanet.position);
        if (dist >= defendTriggerRadius) return false;
        Vector3 toPlanet = (ownPlanet.position - ball.transform.position).normalized;
        float approachDot = Vector3.Dot(ball.rb.velocity.normalized, toPlanet);
        return approachDot > defendApproachThreshold;
    }

    void Defend(float dBall)
    {
        if (ownPlanet == null) return;
        Vector3 interceptPos = ball.transform.position +
            (ownPlanet.position - ball.transform.position).normalized * defendPushDistance * 0.5f;
        MoveTo(interceptPos, defendPushDistance);
        if (dBall <= defendPushDistance && shootTimer <= 0f)
        {
            ball.RegisterTouch(teamID);
            ball.Impulse((ball.transform.position - ownPlanet.position).normalized, shootForce * 1.2f);
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

    // ─── Empuje por contacto ──────────────────────────────────────────────────

    void CheckContactPush()
    {
        contactTimer = contactCooldown;
        Collider[] hits = Physics.OverlapSphere(transform.position, contactPushRadius);
        foreach (var col in hits)
        {
            if (col.gameObject == gameObject) continue;
            Vector3 dir = (col.transform.position - transform.position).normalized;
            if (dir == Vector3.zero) dir = Random.onUnitSphere;

            PlayerSpaceController player = col.GetComponent<PlayerSpaceController>();
            if (player != null) { player.ApplyPush(dir * contactPushForce); return; }

            TeamAI other = col.GetComponent<TeamAI>();
            if (other != null && other != this) other.ApplyPush(dir * contactPushForce);
        }
    }

    // ─── Movimiento ───────────────────────────────────────────────────────────

    void LateUpdate() { EnforceBoundary(); }

    void MoveTo(Vector3 target, float stopDist)
    {
        Vector3 to = target - transform.position;
        if (to.magnitude <= stopDist) return;

        Vector3 baseDir = to.normalized;
        Vector3 dir = baseDir;

        if (planetLayer.value != 0)
        {
            Collider[] obstacles = Physics.OverlapSphere(transform.position, avoidRadius, planetLayer);
            foreach (var obs in obstacles)
            {
                if (obs.gameObject == gameObject) continue;
                dir += (transform.position - obs.transform.position).normalized * 2f;
            }
        }

        foreach (var other in allAIs)
        {
            if (other == this) continue;
            float d = Vector3.Distance(transform.position, other.transform.position);
            if (d < separationRadius && d > 0.01f)
                dir += (transform.position - other.transform.position).normalized * (separationForce / d);
        }

        dir = dir.sqrMagnitude > 0.001f ? dir.normalized : baseDir;
        transform.position += dir * moveSpeed * Time.deltaTime;

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                   Quaternion.LookRotation(dir),
                                                   10f * Time.deltaTime);
    }

    void EnforceBoundary()
    {
        if (sphereCenter == null) return;
        Vector3 offset = transform.position - sphereCenter.position;
        if (offset.magnitude > boundaryRadius)
            transform.position = sphereCenter.position + offset.normalized * boundaryRadius;
    }
}
