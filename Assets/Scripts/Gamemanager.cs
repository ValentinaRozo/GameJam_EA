using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Score")]
    public int maxScore = 3;
    public int scoreA = 0;
    public int scoreB = 0;

    [Header("UI")]
    public TextMeshProUGUI scoreTextA;
    public TextMeshProUGUI scoreTextB;
    public TextMeshProUGUI goalAnnouncerText;
    public TextMeshProUGUI matchTimerText;    // cronometro superior central
    public TextMeshProUGUI countdownText;     // "3 2 1" centro pantalla

    [Header("Scenes")]
    public string winScene = "Win";
    public string gameOverScene = "GameOver";

    [Header("References")]
    public RandomSpawnManager spawnManager;

    public static event System.Action OnGoalScored;
    public bool gameEnded = false;

    private float matchTime = 0f;

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
        matchTime = 0f;
        UpdateUI();

        if (goalAnnouncerText != null) goalAnnouncerText.gameObject.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (gameEnded) return;
        matchTime += Time.deltaTime;
        UpdateMatchTimer();
    }

    void UpdateMatchTimer()
    {
        if (matchTimerText == null) return;
        int minutes = (int)(matchTime / 60f);
        int seconds = (int)(matchTime % 60f);
        matchTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void TeamAScored()
    {
        if (gameEnded) return;
        scoreA++;
        UpdateUI();
        OnGoalScored?.Invoke();

        if (scoreA >= maxScore) EndGameAndLoad(winScene, "You Win!");
        else StartCoroutine(RespawnWithCountdown("Team A scored!"));
    }

    public void TeamBScored()
    {
        if (gameEnded) return;
        scoreB++;
        UpdateUI();
        OnGoalScored?.Invoke();

        if (scoreB >= maxScore) EndGameAndLoad(gameOverScene, "Game Over!");
        else StartCoroutine(RespawnWithCountdown("Team B scored!"));
    }

    IEnumerator RespawnWithCountdown(string goalMsg)
    {
        // Muestra "Team X scored!" brevemente
        SafeShowAnnouncer(goalMsg);

        // Llama al respawn del spawnManager (congela jugadores)
        if (spawnManager != null) spawnManager.RespawnAfterGoal();
        else FindObjectOfType<RandomSpawnManager>()?.RespawnAfterGoal();

        // Countdown 3 - 2 - 1
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        for (int i = 3; i >= 1; i--)
        {
            if (countdownText != null) countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (goalAnnouncerText != null) goalAnnouncerText.gameObject.SetActive(false);
    }

    void EndGameAndLoad(string sceneName, string message)
    {
        gameEnded = true;
        SafeShowAnnouncer(message);
        StartCoroutine(LoadSceneDelayed(sceneName, 2f));
    }

    IEnumerator LoadSceneDelayed(string sceneName, float delay)
    {
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

    void UpdateUI()
    {
        if (scoreTextA != null) scoreTextA.text = "Team A: " + scoreA;
        if (scoreTextB != null) scoreTextB.text = "Team B: " + scoreB;
    }
}
