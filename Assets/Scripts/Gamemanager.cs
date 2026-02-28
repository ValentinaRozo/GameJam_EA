using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Score")]
    public int maxScore = 3; // <-- 3 goles
    public int scoreA = 0;
    public int scoreB = 0;

    [Header("UI")]
    public TextMeshProUGUI scoreTextA;
    public TextMeshProUGUI scoreTextB;
    public TextMeshProUGUI goalAnnouncerText;

    [Header("Scenes")]
    public string winScene = "Win";
    public string gameOverScene = "GameOver";

    [Header("References")]
    public RandomSpawnManager spawnManager;

    public static event System.Action OnGoalScored;
    public bool gameEnded = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        gameEnded = false;
        scoreA = 0;
        scoreB = 0;
        UpdateUI();

        if (goalAnnouncerText != null)
            goalAnnouncerText.gameObject.SetActive(false);
    }

    public void TeamAScored()
    {
        if (gameEnded) return;

        scoreA++;
        UpdateUI();
        OnGoalScored?.Invoke();

        if (scoreA >= maxScore)
        {
            EndGameAndLoad(winScene, "You Win!");
        }
        else
        {
            SafeShowAnnouncer("Team A scored!");
            Respawn();
        }
    }

    public void TeamBScored()
    {
        if (gameEnded) return;

        scoreB++;
        UpdateUI();
        OnGoalScored?.Invoke();

        if (scoreB >= maxScore)
        {
            EndGameAndLoad(gameOverScene, "Game Over!");
        }
        else
        {
            SafeShowAnnouncer("Team B scored!");
            Respawn();
        }
    }

    void EndGameAndLoad(string sceneName, string message)
    {
        gameEnded = true;
        SafeShowAnnouncer(message);

        // Si tienes pausa en alg?n lado, esto evita que se congele la espera
        StartCoroutine(LoadSceneDelayed(sceneName, 2f));
    }

    IEnumerator LoadSceneDelayed(string sceneName, float delay)
    {
        // Realtime para que funcione aunque Time.timeScale = 0
        yield return new WaitForSecondsRealtime(delay);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(sceneName);
    }

    void SafeShowAnnouncer(string msg)
    {
        if (goalAnnouncerText == null) return;
        goalAnnouncerText.text = msg;
        goalAnnouncerText.gameObject.SetActive(true);
    }

    void Respawn()
    {
        if (spawnManager != null) spawnManager.RespawnAfterGoal();
        else FindObjectOfType<RandomSpawnManager>()?.RespawnAfterGoal();
    }

    void UpdateUI()
    {
        if (scoreTextA != null) scoreTextA.text = "Team A: " + scoreA;
        if (scoreTextB != null) scoreTextB.text = "Team B: " + scoreB;
    }
}