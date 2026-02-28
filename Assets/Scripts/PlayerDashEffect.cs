using UnityEngine;
using TMPro;

public class PlayerDashEffect : MonoBehaviour
{
    [Header("Dash")]
    public float dashForce = 22f;
    public float dashCooldown = 1.2f;
    public float pushMultiplier = 1.8f;

    [Header("UI Cooldown (opcional)")]
    public TextMeshProUGUI cooldownText; // esquina inferior izquierda, debajo del efecto de campo

    private PlayerSpaceController player;
    private float cooldownTimer = 0f;
    private bool onCooldown => cooldownTimer > 0f;

    void Start()
    {
        player = GetComponent<PlayerSpaceController>();
        if (player != null) player.pushMultiplier = pushMultiplier;
        Debug.Log("[Dash] Activado  Presiona Space para impulso");
    }

    void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            UpdateCooldownUI();
        }

        if (Input.GetKeyDown(KeyCode.Space) && !onCooldown)
            PerformDash();
    }

    void PerformDash()
    {
        Vector3 dashDir = GetDashDirection();
        player.ApplyPush(dashDir * dashForce);
        cooldownTimer = dashCooldown;
        UpdateCooldownUI();
        Debug.Log($"[Dash] Impulso  {dashDir}");
    }

    Vector3 GetDashDirection()
    {
        // Lee los mismos inputs que PlayerSpaceController
        float x = 0f, z = 0f, y = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x = 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) z = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) z = -1f;
        if (Input.GetKey(KeyCode.E)) y = 1f;
        if (Input.GetKey(KeyCode.Q)) y = -1f;

        Camera cam = player?.mainCamera;
        if (cam != null)
        {
            Vector3 dir = cam.transform.right * x
                        + cam.transform.forward * z
                        + cam.transform.up * y;

            // Si hay input direccional, dash hacia allá
            if (dir.sqrMagnitude > 0.01f) return dir.normalized;
        }

        // Sin input dash hacia donde mira el jugador
        return transform.forward;
    }

    void UpdateCooldownUI()
    {
        if (cooldownText == null) return;

        if (cooldownTimer > 0f)
        {
            cooldownText.gameObject.SetActive(true);
            cooldownText.text = $"Space — {cooldownTimer:F1}s";
        }
        else
        {
            cooldownText.text = "Space — Listo";
        }
    }

    void OnDestroy()
    {
        if (player != null) player.pushMultiplier = 1f;
        if (cooldownText != null) cooldownText.gameObject.SetActive(false);
        Debug.Log("[Dash] Desactivado");
    }
}
