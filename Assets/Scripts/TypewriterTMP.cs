using System.Collections;
using TMPro;
using UnityEngine;

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
    public AudioSource typingAudio;     // Audio 1 (se destruye)
    public AudioSource continueAudio;   // Audio 2

    public KeyCode advanceKey = KeyCode.Return;

    int pageIndex = 0;
    bool isTyping = false;
    bool pageFinished = false;
    Coroutine typingCoroutine;

    void Start()
    {
        if (continueText != null) continueText.gameObject.SetActive(false);
        ShowPage(pageIndex);
    }

    void Update()
    {
        if (!Input.GetKeyDown(advanceKey)) return;

        // Si está escribiendo → completar texto
        if (isTyping)
        {
            SkipTyping();
            return;
        }

        // Si ya terminó → destruir audio1, sonar audio2 y cambiar texto
        if (pageFinished)
        {
            DestroyTypingAudio();   // 🔥 destruye AudioSource 1
            PlayContinueAudio();    // ▶ reproduce AudioSource 2
            NextPageImmediate();    // ➜ cambia texto
        }
    }

    void ShowPage(int index)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(pages[index]));
    }

    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        pageFinished = false;

        if (continueText != null) continueText.gameObject.SetActive(false);
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
            continueText.gameObject.SetActive(true);
    }

    void NextPageImmediate()
    {
        pageIndex++;

        if (pageIndex < pages.Length)
        {
            ShowPage(pageIndex);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void DestroyTypingAudio()
    {
        if (typingAudio != null)
        {
            Destroy(typingAudio); // 💥 destruye el componente AudioSource
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