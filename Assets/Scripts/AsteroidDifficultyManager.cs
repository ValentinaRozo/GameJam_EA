using UnityEngine;

public class AsteroidDifficultyManager : MonoBehaviour
{
    [Header("Velocidad")]
    public float baseSpeed = 3f;
    [Tooltip("Aumento de velocidad por gol")]
    public float speedIncrement = 1.5f;
    [Tooltip("Velocidad máxima")]
    public float maxSpeed = 18f;

    [Header("Tamaño")]
    [Tooltip("Multiplicador base de escala")]
    public float baseSizeMultiplier = 1f;
    [Tooltip("Aumento del multiplicador por gol")]
    public float sizeIncrement = 0.15f;
    [Tooltip("Multiplicador máximo")]
    public float maxSizeMultiplier = 3f;

    [HideInInspector] public float currentSpeed;
    [HideInInspector] public float currentSizeMultiplier;

    void OnEnable() { GameManager.OnGoalScored += OnGoalScored; }
    void OnDisable() { GameManager.OnGoalScored -= OnGoalScored; }

    void Start()
    {
        currentSpeed = baseSpeed;
        currentSizeMultiplier = baseSizeMultiplier;
    }

    void OnGoalScored()
    {
        currentSpeed = Mathf.Min(currentSpeed + speedIncrement, maxSpeed);
        currentSizeMultiplier = Mathf.Min(currentSizeMultiplier + sizeIncrement, maxSizeMultiplier);

        Debug.Log($"[Dificultad] Vel: {currentSpeed:F1} | Tamaño x{currentSizeMultiplier:F2}");

        ApplyToAllAsteroids();
    }

    void ApplyToAllAsteroids()
    {
        GameObject[] asteroids = GameObject.FindGameObjectsWithTag("Asteroid");
        foreach (var ast in asteroids)
        {
            // Escalar respetando la escala base del asteroide
            AsteroidBaseScale baseScale = ast.GetComponent<AsteroidBaseScale>();
            float base_ = baseScale != null ? baseScale.originalScale : 1f;
            ast.transform.localScale = Vector3.one * base_ * currentSizeMultiplier;

            Rigidbody rb = ast.GetComponent<Rigidbody>();
            if (rb == null) continue;

            if (rb.velocity.sqrMagnitude > 0.01f)
                rb.velocity = rb.velocity.normalized * currentSpeed;
            else
                rb.velocity = Random.onUnitSphere * currentSpeed;
        }
    }

    // Llamado por AsteroidSpawner al instanciar
    public void InitAsteroid(GameObject asteroid, float baseScale)
    {
        // Guardar escala base en el componente
        AsteroidBaseScale bs = asteroid.GetComponent<AsteroidBaseScale>();
        if (bs == null) bs = asteroid.AddComponent<AsteroidBaseScale>();
        bs.originalScale = baseScale;

        // Aplicar dificultad actual
        asteroid.transform.localScale = Vector3.one * baseScale * currentSizeMultiplier;

        Rigidbody rb = asteroid.GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = Random.onUnitSphere * currentSpeed;
    }
}
