using UnityEngine;

// Se añade temporalmente a asteroides golpeados por la raqueta
public class ChargedAsteroid : MonoBehaviour
{
    public float pushForce = 15f;
    public float lifetime = 5f;

    void Start() { Destroy(this, lifetime); }

    void OnCollisionEnter(Collision col)
    {
        PlayerSpaceController player = col.gameObject.GetComponent<PlayerSpaceController>();
        if (player != null)
        {
            Vector3 dir = (col.transform.position - transform.position).normalized;
            player.ApplyPush(dir * pushForce);
            Destroy(this);
            return;
        }

        TeamAI ai = col.gameObject.GetComponent<TeamAI>();
        if (ai != null)
        {
            Vector3 dir = (col.transform.position - transform.position).normalized;
            ai.ApplyPush(dir * pushForce);
            Destroy(this);
        }
    }
}
