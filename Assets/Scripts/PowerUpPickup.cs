using UnityEngine;

public enum PowerUpType { RaquetaTenis, Bate, BalonBasket }

public class PowerUpPickup : MonoBehaviour
{
    [Header("Tipo")]
    public PowerUpType type;

    [Header("BalonBasket (solo si aplica)")]
    public GameObject balonBasketBallPrefab;

    [Header("Visual")]
    public float rotateSpeed = 90f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerSpaceController player = other.GetComponent<PlayerSpaceController>();
        if (player == null) return;

        switch (type)
        {
            case PowerUpType.RaquetaTenis:
                if (player.GetComponent<RaquetaTenisEffect>() == null)
                    player.gameObject.AddComponent<RaquetaTenisEffect>();
                break;

            case PowerUpType.Bate:
                if (player.GetComponent<BateEffect>() == null)
                    player.gameObject.AddComponent<BateEffect>();
                break;

            case PowerUpType.BalonBasket:
                if (balonBasketBallPrefab != null)
                    Instantiate(balonBasketBallPrefab, transform.position, Quaternion.identity);
                break;
        }

        Destroy(gameObject);
    }
}
