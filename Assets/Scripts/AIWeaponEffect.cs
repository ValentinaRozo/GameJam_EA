using UnityEngine;

public class AIWeaponEffect : MonoBehaviour
{
    public enum WeaponType { Raqueta, Bate }

    [Header("Tipo")]
    public WeaponType type;

    [Header("Configuración")]
    public float swingRadius = 4f;
    public float asteroidForce = 20f;
    public float playerForce = 18f;
    public float swingCooldown = 1.5f;
    public float duration = 5f;

    private float timer;
    private float swingTimer;
    private int planetLayer;

    void Start()
    {
        timer = duration;
        swingTimer = 0f;
        planetLayer = LayerMask.NameToLayer("Planet");
    }

    void Update()
    {
        timer -= Time.deltaTime;
        swingTimer -= Time.deltaTime;

        if (timer <= 0f) { Destroy(this); return; }

        if (swingTimer <= 0f)
        {
            AutoSwing();
            swingTimer = swingCooldown;
        }
    }

    void AutoSwing()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, swingRadius);

        foreach (var col in hits)
        {
            if (col.gameObject == gameObject) continue;
            if (col.gameObject.layer == planetLayer) continue;

            Vector3 dir = (col.transform.position - transform.position).normalized;
            if (dir == Vector3.zero) dir = Random.onUnitSphere;

            if (type == WeaponType.Raqueta && col.CompareTag("Asteroid"))
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.AddForce(dir * asteroidForce, ForceMode.Impulse);
                    if (col.GetComponent<ChargedAsteroid>() == null)
                        col.gameObject.AddComponent<ChargedAsteroid>();
                }
            }

            if (type == WeaponType.Bate)
            {
                if (col.CompareTag("Asteroid"))
                {
                    Rigidbody rb = col.GetComponent<Rigidbody>();
                    if (rb != null) rb.AddForce(dir * (asteroidForce * 0.3f), ForceMode.Impulse);
                }

                PlayerSpaceController player = col.GetComponent<PlayerSpaceController>();
                if (player != null) player.ApplyPush(dir * playerForce);
            }
        }
    }
}
