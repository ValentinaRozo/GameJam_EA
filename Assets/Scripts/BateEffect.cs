using UnityEngine;

public class BateEffect : MonoBehaviour
{
    [Header("Configuracion")]
    public float swingRadius = 3f;
    public float asteroidForce = 6f;
    public float playerForce = 22f;
    public float duration = 4f;

    private float timer;
    private int planetLayer;

    void Start()
    {
        timer = duration;
        planetLayer = LayerMask.NameToLayer("Planet");
        Debug.Log("[Bate] Activado — Click izquierdo para golpear");
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Debug.Log("[Bate] Expirado");
            Destroy(this);
            return;
        }

        if (Input.GetMouseButtonDown(0)) Swing();
    }

    void Swing()
    {
        Debug.Log("[Bate] Swing!");
        Collider[] hits = Physics.OverlapSphere(transform.position, swingRadius);

        foreach (var col in hits)
        {
            if (col.gameObject == gameObject) continue;
            if (col.gameObject.layer == planetLayer) continue;

            Vector3 dir = (col.transform.position - transform.position).normalized;
            if (dir == Vector3.zero) dir = Random.onUnitSphere;

            // Asteroide  empujón leve
            if (col.CompareTag("Asteroid"))
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null) rb.AddForce(dir * asteroidForce, ForceMode.Impulse);
                continue;
            }

            // Jugador humano  empujón fuerte
            PlayerSpaceController player = col.GetComponent<PlayerSpaceController>();
            if (player != null)
            {
                player.ApplyPush(dir * playerForce);
                continue;
            }

            // IA  empujón fuerte
            TeamAI ai = col.GetComponent<TeamAI>();
            if (ai != null) ai.ApplyPush(dir * playerForce);
        }
    }

    // Gizmo para ver el radio de swing en Scene View
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, swingRadius);
    }
}
