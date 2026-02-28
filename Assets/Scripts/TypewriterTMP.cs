using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Control")]
    public KeyCode advanceKey = KeyCode.Return;

    [Header("Escena")]
    public string sceneToLoad; // 👈 Nombre de tu escena de juego

    int pageIndex = 0;
    bool isTyping = false;
    bool pageFinished = false;
    bool isLastPage = false;
    Coroutine typingCoroutine;

    void Start()
    {
        if (continueText != null)
            continueText.gameObject.SetActive(false);

        ShowPage(pageIndex);
    }

    void Update()
    {
        if (!Input.GetKeyDown(advanceKey)) return;

        if (isTyping)
        {
            SkipTyping();
            return;
        }

        if (pageFinished)
        {
            DestroyTypingAudio();
            PlayContinueAudio();

            // 🔥 Si es la última página → iniciar juego
            if (isLastPage)
            {
                StartGame();
            }
            else
            {
                NextPageImmediate();
            }
        }
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
            yield return new WaitForSeconds(charDelay);
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

            // 🔥 Cambiar texto según si es la última página
            if (isLastPage)
                continueText.text = "Press Enter to Start Game";
            else
                continueText.text = "Press Enter to Continue";
        }
    }

    void NextPageImmediate()
    {
        pageIndex++;

        if (pageIndex < pages.Length)
        {
            ShowPage(pageIndex);
        }
    }

    void StartGame()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
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