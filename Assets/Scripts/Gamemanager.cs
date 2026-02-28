using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Score")]
    public int maxScore = 2;
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

        // Log all scenes in Build Settings for verification
        Debug.Log("[GM] Scenes in Build Settings:");
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            Debug.Log("[GM]   [" + i + "] " + path);
        }
    }

    public void TeamAScored()
    {
        if (gameEnded) { Debug.Log("[GM] TeamAScored ignored — gameEnded is true"); return; }
        scoreA++;
        Debug.Log("[GM] Team A scored -> " + scoreA + "/" + maxScore);
        UpdateUI();
        OnGoalScored?.Invoke();

        if (scoreA >= maxScore)
        {
            gameEnded = true;
            Debug.Log("[GM] Team A wins! Starting coroutine to load: " + winScene);
            StartCoroutine(LoadSceneDelayed(winScene, 2f));
        }
        else
        {
            SafeShowAnnouncer("Team A scored!");
            Respawn();
        }
    }

    public void TeamBScored()
    {
        if (gameEnded) { Debug.Log("[GM] TeamBScored ignored — gameEnded is true"); return; }
        scoreB++;
        Debug.Log("[GM] Team B scored -> " + scoreB + "/" + maxScore);
        UpdateUI();
        OnGoalScored?.Invoke();

        if (scoreB >= maxScore)
        {
            gameEnded = true;
            Debug.Log("[GM] Team B wins! Starting coroutine to load: " + gameOverScene);
            StartCoroutine(LoadSceneDelayed(gameOverScene, 2f));
        }
        else
        {
            SafeShowAnnouncer("Team B scored!");
            Respawn();
        }
    }

    IEnumerator LoadSceneDelayed(string sceneName, float delay)
    {
        Debug.Log("[GM] Coroutine started — waiting " + delay + "s to load: " + sceneName);
        SafeShowAnnouncer(scoreA >= maxScore ? "You Win!" : "Game Over!");
        yield return new WaitForSeconds(delay);
        Debug.Log("[GM] Delay finished — calling SceneManager.LoadScene(" + sceneName + ")");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(sceneName);
        Debug.Log("[GM] LoadScene called — if you see this, the scene name is wrong or not in Build Settings");
    }

    void SafeShowAnnouncer(string msg)
    {
        if (goalAnnouncerText == null) return;
        try
        {
            goalAnnouncerText.text = msg;
            goalAnnouncerText.gameObject.SetActive(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[GM] goalAnnouncerText error: " + e.Message);
        }
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
