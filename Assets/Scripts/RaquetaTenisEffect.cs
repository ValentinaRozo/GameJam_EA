using UnityEngine;

public class RaquetaTenisEffect : MonoBehaviour
{
    [Header("Configuración")]
    public float swingRadius = 4f;
    public float asteroidForce = 22f;
    public float duration = 5f;

    private float timer;
    private int planetLayer;

    void Start()
    {
        timer = duration;
        planetLayer = LayerMask.NameToLayer("Planet");
        Debug.Log("[RaquetaTenis] Activada — Click izquierdo para golpear");
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Debug.Log("[RaquetaTenis] Expirada");
            Destroy(this);
            return;
        }

        if (Input.GetMouseButtonDown(0)) Swing();
    }

    void Swing()
    {
        Debug.Log("[RaquetaTenis] Swing!");
        Collider[] hits = Physics.OverlapSphere(transform.position, swingRadius);

        foreach (var col in hits)
        {
            if (col.gameObject == gameObject) continue;
            if (col.gameObject.layer == planetLayer) continue;
            if (!col.CompareTag("Asteroid")) continue;

            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb == null) continue;

            Vector3 dir = (col.transform.position - transform.position).normalized;
            if (dir == Vector3.zero) dir = Random.onUnitSphere;

            rb.velocity = Vector3.zero;
            rb.AddForce(dir * asteroidForce, ForceMode.Impulse);

            // Marcar el asteroide para que empuje jugadores si los golpea
            ChargedAsteroid charged = col.GetComponent<ChargedAsteroid>();
            if (charged == null)
                col.gameObject.AddComponent<ChargedAsteroid>();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, swingRadius);
    }
}
