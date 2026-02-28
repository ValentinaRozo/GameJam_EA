using UnityEngine;

public class RaquetaTenisEffect : MonoBehaviour
{
    public float swingRadius = 4f;
    public float asteroidForce = 22f;   // velocidad aumentada
    public float duration = 5f;

    private float timer;
    private int planetLayer;

    void Start()
    {
        timer = duration;
        planetLayer = LayerMask.NameToLayer("Planet");
        Debug.Log("[RaquetaTenis] Activada — Click para golpear");
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
            if (!col.CompareTag("Asteroid")) continue;

            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb == null) continue;

            Vector3 dir = (col.transform.position - transform.position).normalized;
            rb.velocity = Vector3.zero;
            rb.AddForce(dir * asteroidForce, ForceMode.Impulse);

            // Marcar como cargado: si golpea a un jugador, lo empuja
            if (col.GetComponent<ChargedAsteroid>() == null)
                col.gameObject.AddComponent<ChargedAsteroid>();
        }
    }

    void OnDestroy() { Debug.Log("[RaquetaTenis] Expirada"); }
}
