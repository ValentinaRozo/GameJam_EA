using UnityEngine;
using TMPro;

public enum FieldEffectType { Dash, LowGravity, AsteroidCaos }

public class FieldEffectManager : MonoBehaviour
{
    public static FieldEffectManager Instance;

    [Header("Configuración General")]
    public float effectDuration = 20f;

    [Header("Probabilidades (deben sumar <= 1)")]
    [Range(0f, 1f)] public float chanceNone = 0.25f; // probabilidad de que NO haya efecto
    [Range(0f, 1f)] public float chanceDash = 0.35f;
    [Range(0f, 1f)] public float chanceLowGravity = 0.30f;
    [Range(0f, 1f)] public float chanceAsteroidCaos = 0.10f;
    // Para pruebas: pon chanceAsteroidCaos = 1 y el resto = 0

    [Header("LowGravity — Balón Pesado")]
    public float heavyMass = 4f;
    public float heavyDrag = 2f;

    [Header("AsteroidCaos")]
    public float caosScaleMultiplier = 2.5f;   // qué tan grandes se hacen
    public float caosSpeedMultiplier = 3f;     // qué tan rápidos se hacen
    public float caosDuration = 8f;     // duración independiente (más corta)

    [Header("UI")]
    public TextMeshProUGUI effectNameText;
    public TextMeshProUGUI dashCooldownText;

    [Header("Referencias")]
    public BallPhysics ball;

    private FieldEffectType? currentEffect = null;
    private float timer = 0f;
    private Rigidbody ballRb;
    private float originalMass, originalDrag;

    void Awake() { Instance = this; }

    void Start()
    {
        if (ball == null) ball = FindObjectOfType<BallPhysics>();
        ballRb = ball?.GetComponent<Rigidbody>();

        if (ballRb != null)
        {
            originalMass = ballRb.mass;
            originalDrag = ballRb.drag;
        }

        GameManager.OnGoalScored += OnGoalScored;
        HideUI();

        Invoke(nameof(TryActivateFirst), 0.5f);
    }

    void OnDestroy() { GameManager.OnGoalScored -= OnGoalScored; }

    void Update()
    {
        if (currentEffect == null) return;
        timer -= Time.deltaTime;
        if (timer <= 0f) Deactivate();
    }

    void TryActivateFirst()
    {
        FieldEffectType? chosen = PickEffect();
        if (chosen != null) Activate(chosen.Value);
    }

    void OnGoalScored()
    {
        Deactivate();
        FieldEffectType? chosen = PickEffect();
        if (chosen != null) Activate(chosen.Value);
    }

    // Selección por rangos acumulados — más claro que Random.Range múltiple
    FieldEffectType? PickEffect()
    {
        float roll = Random.value; // 0.0 a 1.0

        float cursor = 0f;

        cursor += chanceNone;
        if (roll < cursor) return null;

        cursor += chanceDash;
        if (roll < cursor) return FieldEffectType.Dash;

        cursor += chanceLowGravity;
        if (roll < cursor) return FieldEffectType.LowGravity;

        cursor += chanceAsteroidCaos;
        if (roll < cursor) return FieldEffectType.AsteroidCaos;

        return null;
    }

    void Activate(FieldEffectType effect)
    {
        currentEffect = effect;
        timer = effect == FieldEffectType.AsteroidCaos ? caosDuration : effectDuration;

        switch (effect)
        {
            case FieldEffectType.Dash:
                var player = FindObjectOfType<PlayerSpaceController>();
                if (player != null && player.GetComponent<PlayerDashEffect>() == null)
                {
                    var d = player.gameObject.AddComponent<PlayerDashEffect>();
                    if (dashCooldownText != null) d.cooldownText = dashCooldownText;
                }
                foreach (var ai in FindObjectsOfType<TeamAI>()) ai.EnableDash();
                break;

            case FieldEffectType.LowGravity:
                if (ballRb != null) { ballRb.mass = heavyMass; ballRb.drag = heavyDrag; }
                foreach (var ai in FindObjectsOfType<TeamAI>()) ai.EnableHeavyBall();
                break;

            case FieldEffectType.AsteroidCaos:
                foreach (var ast in FindObjectsOfType<Asteroid>())
                    ast.ApplyCaos(caosScaleMultiplier, caosSpeedMultiplier);
                break;
        }

        ShowUI(GetDisplayName(effect));
        Debug.Log($"[FieldEffect] Activado: {effect} por {timer}s");
    }

    void Deactivate()
    {
        if (currentEffect == null) return;

        switch (currentEffect.Value)
        {
            case FieldEffectType.Dash:
                var player = FindObjectOfType<PlayerSpaceController>();
                var dash = player?.GetComponent<PlayerDashEffect>();
                if (dash != null) Destroy(dash);
                foreach (var ai in FindObjectsOfType<TeamAI>()) ai.DisableDash();
                break;

            case FieldEffectType.LowGravity:
                if (ballRb != null) { ballRb.mass = originalMass; ballRb.drag = originalDrag; }
                foreach (var ai in FindObjectsOfType<TeamAI>()) ai.DisableHeavyBall();
                break;

            case FieldEffectType.AsteroidCaos:
                foreach (var ast in FindObjectsOfType<Asteroid>())
                    ast.RemoveCaos();
                break;
        }

        Debug.Log($"[FieldEffect] Desactivado: {currentEffect.Value}");
        currentEffect = null;
        timer = 0f;
        HideUI();
    }

    void ShowUI(string name)
    {
        if (effectNameText == null) return;
        effectNameText.text = name;
        effectNameText.gameObject.SetActive(true);
    }

    void HideUI()
    {
        if (effectNameText != null)
            effectNameText.gameObject.SetActive(false);
    }

    string GetDisplayName(FieldEffectType e) => e switch
    {
        FieldEffectType.Dash => "Dash",
        FieldEffectType.LowGravity => "Balón Pesado",
        FieldEffectType.AsteroidCaos => "Asteroid Caos",
        _ => e.ToString()
    };
}

