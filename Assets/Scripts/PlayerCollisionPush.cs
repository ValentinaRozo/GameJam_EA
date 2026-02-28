using UnityEngine;

public class PlayerCollisionPush : MonoBehaviour
{
    [Header("Push entre jugadores")]
    public float pushForce = 10f;
    public float pushRadius = 1.2f;
    public float pushCooldown = 0.3f;

    private CharacterController controller;
    private float cooldownTimer = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;
    }

    // CharacterController detecta cuando golpea a un RigidBody o collider
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (cooldownTimer > 0f) return;

        // Empujar AI (TeamAI usa transform, no RigidBody)
        TeamAI ai = hit.gameObject.GetComponent<TeamAI>();
        if (ai != null)
        {
            Vector3 dir = (hit.transform.position - transform.position).normalized;
            ai.ApplyPush(dir * pushForce);
            cooldownTimer = pushCooldown;
            return;
        }

        // Empujar otro jugador humano si hubiera más
        PlayerSpaceController otherPlayer = hit.gameObject.GetComponent<PlayerSpaceController>();
        if (otherPlayer != null && otherPlayer.gameObject != gameObject)
        {
            Vector3 dir = (hit.transform.position - transform.position).normalized;
            otherPlayer.ApplyPush(dir * pushForce);
            cooldownTimer = pushCooldown;
        }
    }
}
