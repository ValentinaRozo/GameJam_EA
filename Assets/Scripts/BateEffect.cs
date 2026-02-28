using UnityEngine;

public class BateEffect : MonoBehaviour
{
    public float swingRadius = 3f;
    public float asteroidForce = 6f;    // leve
    public float playerForce = 22f;   // fuerte
    public float duration = 4f;

    private float timer;
    private int planetLayer;

    void Start()
    {
        timer = duration;
        planetLayer = LayerMask.NameToLayer("Planet");
        Debug.Log("[Bate] Activado — Click para golpear");
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) { Destroy(this); return; }

        if (Input.GetMouseButtonDown(0)) Swing();
    }

    void Swing()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, swingRadius);
        foreach (var col in hits)
        {
            if (col.gameObject == gameObject) continue;
            if (col.gameObject.layer == planetLayer) continue;

            Vector3 dir = (col.transform.position - transform.position).normalized;

            // Asteroide: empujón leve
            if (col.CompareTag("Asteroid"))
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null) rb.AddForce(dir * asteroidForce, ForceMode.Impulse);
            }

            // Jugador humano: empujón fuerte
            PlayerSpaceController player = col.GetComponent<PlayerSpaceController>();
            if (player != null) player.ApplyPush(dir * playerForce);

            // IA: empujón fuerte
            TeamAI ai = col.GetComponent<TeamAI>();
            if (ai != null) ai.ApplyPush(dir * playerForce);
        }
    }

    void OnDestroy() { Debug.Log("[Bate] Expirado"); }
}
