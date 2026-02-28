using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button quitButton;

    private bool isPaused = false;
    private int selectedIndex = 0; // 0 = Resume, 1 = Quit

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (isPaused) Resume();
            else Pause();
            return;
        }

        if (!isPaused) return;

        // Navegar hacia abajo
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            selectedIndex = 1;
            SelectButton();
        }
        // Navegar hacia arriba
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            selectedIndex = 0;
            SelectButton();
        }
        // Confirmar con Enter
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (selectedIndex == 0) Resume();
            else QuitToMenu();
        }
    }

    void Pause()
    {
        isPaused = true;
        selectedIndex = 0;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        SelectButton();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Menu");
    }

    void SelectButton()
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (selectedIndex == 0) resumeButton.Select();
        else quitButton.Select();
    }
}
