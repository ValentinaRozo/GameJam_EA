using UnityEngine;

public class ChargedAsteroid : MonoBehaviour
{
    [Header("Configuración")]
    public float pushForce = 15f;
    public float lifetime = 5f;

    void Start()
    {
        Destroy(this, lifetime);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Planet")) return;

        Vector3 dir = (col.transform.position - transform.position).normalized;
        if (dir == Vector3.zero) dir = Random.onUnitSphere;

        PlayerSpaceController player = col.gameObject.GetComponent<PlayerSpaceController>();
        if (player != null)
        {
            player.ApplyPush(dir * pushForce);
            Destroy(this);
            return;
        }

        TeamAI ai = col.gameObject.GetComponent<TeamAI>();
        if (ai != null)
        {
            ai.ApplyPush(dir * pushForce);
            Destroy(this);
        }
    }
}
