using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Score")]
    public int goalsToEnd = 3;

    [Header("Scenes")]
    public string winScene = "Win";
    public string gameOverScene = "GameOver";

    [Header("Reset")]
    public float freezeDuration = 3f;
    public Transform ballSpawn;
    public BallPhysics ball;

    // Todos los scripts se suscriben a este evento
    public static event Action OnGoalScored;

    private int scoreA = 0;
    private int scoreB = 0;
    private int totalGoals = 0;
    private bool gameEnded = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TeamAScored()
    {
        scoreA++;
        RegisterGoal();
    }

    public void TeamBScored()
    {
        scoreB++;
        RegisterGoal();
    }

    void RegisterGoal()
    {
        if (gameEnded) return;
        totalGoals++;

        ResetBall();
        OnGoalScored?.Invoke();  // Notifica jugador, AI y freeze

        // Respawn aleatorio en borde de esfera
        RandomSpawnManager spawner = FindObjectOfType<RandomSpawnManager>();
        if (spawner != null)
            spawner.RespawnAfterGoal();

        Debug.Log($"Gol #{totalGoals} | Equipo A: {scoreA} - Equipo B: {scoreB}");

        if (totalGoals >= goalsToEnd)
        {
            gameEnded = true;
            StartCoroutine(LoadEndScene());
        }
    }

    void ResetBall()
    {
        if (ball == null || ballSpawn == null) return;

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        ball.transform.position = ballSpawn.position;
        ball.lastToucherTeam = "";
    }

    IEnumerator LoadEndScene()
    {
        yield return new WaitForSeconds(freezeDuration);
        string endScene = scoreA >= scoreB ? winScene : gameOverScene;
        Debug.Log($"Juego terminado. Cargando {endScene}");
        SceneManager.LoadScene(endScene);
    }

    // Para debug — llamar desde botón en escena
    [ContextMenu("Test Goal A")]
    void TestGoalA() { TeamAScored(); }

    [ContextMenu("Test Goal B")]
    void TestGoalB() { TeamBScored(); }
}
