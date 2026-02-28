using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TypewriterTMP : MonoBehaviour
{
    [Header("Referencias")]
    public TextMeshProUGUI textTMP;
    public TextMeshProUGUI continueText;

    [Header("Contenido")]
    [TextArea(2, 8)]
    public string[] pages;

    [Header("Efecto")]
    public float charDelay = 0.03f;

    [Header("Audio")]
    public AudioSource typingAudio;
    public AudioSource continueAudio;

    [Header("Escena")]
    public string sceneToLoad;

    int pageIndex = 0;
    bool isTyping = false;
    bool pageFinished = false;
    bool isLastPage = false;
    Coroutine typingCoroutine;

    void Start()
    {
        if (continueText != null)
            continueText.gameObject.SetActive(false);

        // 🔥 IMPORTANTE PARA WEBGL (quita foco UI)
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        ShowPage(pageIndex);
    }

    void Update()
    {
        if (!AdvancePressed()) return;

        // 🔥 En WebGL ayuda a asegurar foco
#if UNITY_WEBGL
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
#endif

        if (isTyping)
        {
            SkipTyping();
            return;
        }

        if (pageFinished)
        {
            DestroyTypingAudio();
            PlayContinueAudio();

            if (isLastPage)
                StartGame();
            else
                NextPageImmediate();
        }
    }

    // 🔥 MÉTODO ROBUSTO MULTIPLATAFORMA
    bool AdvancePressed()
    {
        // Teclado
        if (Input.GetKeyDown(KeyCode.Return)) return true;
        if (Input.GetKeyDown(KeyCode.KeypadEnter)) return true;
        if (Input.GetKeyDown(KeyCode.Space)) return true;

        // WebGL fallback
        if (Input.GetKeyDown("enter")) return true;
        if (Input.GetKeyDown("return")) return true;

        // Mouse click
        if (Input.GetMouseButtonDown(0)) return true;

        // Gamepad botón A
        if (Input.GetButtonDown("Submit")) return true;

        return false;
    }

    void ShowPage(int index)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isLastPage = (index == pages.Length - 1);
        typingCoroutine = StartCoroutine(TypeText(pages[index]));
    }

    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        pageFinished = false;

        if (continueText != null)
            continueText.gameObject.SetActive(false);

        textTMP.text = "";

        if (typingAudio != null)
            typingAudio.Play();

        for (int i = 0; i < fullText.Length; i++)
        {
            textTMP.text += fullText[i];
            yield return new WaitForSecondsRealtime(charDelay); // 🔥 mejor para WebGL
        }

        FinishPage();
    }

    void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        textTMP.text = pages[pageIndex];
        FinishPage();
    }

    void FinishPage()
    {
        if (typingAudio != null && typingAudio.isPlaying)
            typingAudio.Stop();

        isTyping = false;
        pageFinished = true;

        if (continueText != null)
        {
            continueText.gameObject.SetActive(true);

            if (isLastPage)
                continueText.text = "Press Enter / Space to Start";
            else
                continueText.text = "Press Enter / Space to Continue";
        }
    }

    void NextPageImmediate()
    {
        pageIndex++;

        if (pageIndex < pages.Length)
            ShowPage(pageIndex);
    }

    void StartGame()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("No scene assigned in sceneToLoad");
        }
    }

    void DestroyTypingAudio()
    {
        if (typingAudio != null)
        {
            Destroy(typingAudio);
            typingAudio = null;
        }
    }

    void PlayContinueAudio()
    {
        if (continueAudio != null)
        {
            continueAudio.Stop();
            continueAudio.loop = false;
            continueAudio.Play();
        }
    }
}