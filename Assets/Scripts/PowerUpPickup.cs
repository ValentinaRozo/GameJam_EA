using UnityEngine;

public enum PowerUpType { RaquetaTenis, Bate, BalonBasket }

public class PowerUpPickup : MonoBehaviour
{
    [Header("Tipo")]
    public PowerUpType type;

    [Header("BalonBasket")]
    public GameObject balonBasketBallPrefab;

    [Header("Visual")]
    public float rotateSpeed = 90f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Jugador humano
        PlayerSpaceController player = other.GetComponent<PlayerSpaceController>();
        if (player != null)
        {
            ApplyToPlayer(player);
            Destroy(gameObject);
            return;
        }

        // IA
        TeamAI ai = other.GetComponent<TeamAI>();
        if (ai != null)
        {
            ApplyToAI(ai);
            Destroy(gameObject);
        }
    }

    void ApplyToPlayer(PlayerSpaceController player)
    {
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
                SpawnBasketball();
                break;
        }
    }

    void ApplyToAI(TeamAI ai)
    {
        switch (type)
        {
            case PowerUpType.RaquetaTenis:
                if (ai.GetComponent<AIWeaponEffect>() == null)
                {
                    var effect = ai.gameObject.AddComponent<AIWeaponEffect>();
                    effect.type = AIWeaponEffect.WeaponType.Raqueta;
                }
                break;
            case PowerUpType.Bate:
                if (ai.GetComponent<AIWeaponEffect>() == null)
                {
                    var effect = ai.gameObject.AddComponent<AIWeaponEffect>();
                    effect.type = AIWeaponEffect.WeaponType.Bate;
                }
                break;
            case PowerUpType.BalonBasket:
                SpawnBasketball();
                break;
        }
    }

    void SpawnBasketball()
    {
        if (balonBasketBallPrefab != null)
            Instantiate(balonBasketBallPrefab, transform.position, Random.rotation);
    }
}
