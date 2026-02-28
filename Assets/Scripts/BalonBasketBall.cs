using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BalonBasketBall : MonoBehaviour
{
    [Header("Physics")]
    public float speed = 18f;
    public float pushForce = 18f;
    public float lifetime = 6f;

    [Header("Tamaño aleatorio")]
    public float minSize = 1f;
    public float maxSize = 4f;

    [Header("Boundary")]
    public float boundaryRadius = 21f;

    private Rigidbody rb;
    private Transform sphereCenter;
    private int planetLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 0f;
        rb.angularDrag = 0f;
        rb.velocity = Random.onUnitSphere * speed;

        // Tamaño aleatorio entre minSize y maxSize
        float size = Random.Range(minSize, maxSize);
        transform.localScale = Vector3.one * size;

        sphereCenter = GameObject.Find("SceneSphere")?.transform;
        planetLayer = LayerMask.NameToLayer("Planet");

        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (rb.velocity.sqrMagnitude > 0.1f)
            rb.velocity = rb.velocity.normalized * speed;

        if (sphereCenter == null) return;
        Vector3 fromCenter = transform.position - sphereCenter.position;
        if (fromCenter.magnitude >= boundaryRadius)
        {
            transform.position = sphereCenter.position + fromCenter.normalized * (boundaryRadius - 1f);
            rb.velocity = Vector3.Reflect(rb.velocity, -fromCenter.normalized);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.layer == planetLayer) return;

        Vector3 dir = (col.transform.position - transform.position).normalized;
        if (dir == Vector3.zero) dir = Random.onUnitSphere;

        if (col.gameObject.CompareTag("Asteroid"))
        {
            Rigidbody hitRb = col.rigidbody;
            if (hitRb != null) hitRb.AddForce(dir * pushForce, ForceMode.Impulse);
        }

        PlayerSpaceController player = col.gameObject.GetComponent<PlayerSpaceController>();
        if (player != null) player.ApplyPush(dir * pushForce);

        TeamAI ai = col.gameObject.GetComponent<TeamAI>();
        if (ai != null) ai.ApplyPush(dir * pushForce);
    }
}
