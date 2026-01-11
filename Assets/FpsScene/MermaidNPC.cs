using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class MermaidNPC : MonoBehaviour, IInteractable
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject relicsContainer;
    [SerializeField] private Image[] relicImages;
    [SerializeField] private UIManager uiManager;

    [Header("Dialogue Settings")]
    [SerializeField] private string[] dialogueLines;
    [SerializeField] private float textSpeed = 0.03f;
    [SerializeField] private float delayBetweenLines = 1f;

    [Header("Relic Hints")]
    [SerializeField] private Sprite[] relicSprites;

    [Header("Found Relic Effects")]
    [SerializeField] private Color foundRelicColor = new Color(0.5f, 1f, 0.5f, 1f);

    [Header("UI Positions")]
    [SerializeField] private float textMoveUpAmount = 100f;
    [SerializeField] private float transitionTime = 0.3f;

    [Header("Timing Settings")]
    [SerializeField] private float showRelicsDuration = 5f;

    [Header("Audio")]
    [SerializeField] private AudioClip talkSound;
    [SerializeField] private AudioClip revealSound;

    [Header("Visual Effects")]
    [SerializeField] private GameObject mermaidEffect;
    [SerializeField] private float effectFadeTime = 1.5f;

    private bool isTalking = false;
    private bool canInteract = true;
    private bool hasRevealedRelics = false;
    private Coroutine dialogueCoroutine;
    private Vector3 originalTextPosition;

    void Awake()
    {
        ForceDisableAllUI();
    }

    void Start()
    {
        if (mermaidEffect == null)
        {
            mermaidEffect = transform.Find("MermaidEffect")?.gameObject;
        }
    }

    void ForceDisableAllUI()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (relicsContainer != null)
            relicsContainer.SetActive(false);

        foreach (Image img in relicImages)
        {
            if (img != null)
            {
                img.color = new Color(1, 1, 1, 0);
                img.gameObject.SetActive(false);
            }
        }
    }

    public string GetInteractPrompt()
    {
        return "Mermaid";
    }

    public void OnInteract()
    {
        if (!canInteract || isTalking) return;

        StartDialogue();
    }

    public bool CanInteract()
    {
        return canInteract && !isTalking;
    }

    void StartDialogue()
    {
        if (dialoguePanel == null) return;

        isTalking = true;
        canInteract = false;
        hasRevealedRelics = false;

        if (dialogueCoroutine != null)
            StopCoroutine(dialogueCoroutine);

        dialoguePanel.SetActive(true);

        if (relicsContainer != null)
        {
            relicsContainer.SetActive(false);

            CanvasGroup containerCG = relicsContainer.GetComponent<CanvasGroup>();
            if (containerCG == null) containerCG = relicsContainer.AddComponent<CanvasGroup>();
            containerCG.alpha = 0f;
        }

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);
            originalTextPosition = dialogueText.rectTransform.localPosition;
        }

        SetPlayerControl(false);

        dialogueCoroutine = StartCoroutine(AutoDialogueSequence());
    }

    IEnumerator AutoDialogueSequence()
    {
        for (int i = 0; i < dialogueLines.Length; i++)
        {
            yield return StartCoroutine(ShowDialogueLine(dialogueLines[i]));

            if (i < dialogueLines.Length - 1)
                yield return new WaitForSeconds(delayBetweenLines);
        }

        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(MoveTextUp());

        yield return StartCoroutine(ShowRelics());

        yield return new WaitForSeconds(showRelicsDuration);

        EndDialogue();
    }

    IEnumerator ShowDialogueLine(string line)
    {
        dialogueText.text = "";

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;

            if (talkSound != null && dialogueText.text.Length % 3 == 0)
            {
                AudioSource.PlayClipAtPoint(talkSound, transform.position, 0.3f);
            }

            yield return new WaitForSeconds(textSpeed);
        }
    }

    IEnumerator MoveTextUp()
    {
        if (dialogueText == null) yield break;

        float elapsed = 0f;
        Vector3 startPos = dialogueText.rectTransform.localPosition;
        Vector3 targetPos = startPos + new Vector3(0, textMoveUpAmount, 0);

        while (elapsed < transitionTime)
        {
            dialogueText.rectTransform.localPosition = Vector3.Lerp(
                startPos, targetPos, elapsed / transitionTime
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        dialogueText.rectTransform.localPosition = targetPos;
    }

    IEnumerator ShowRelics()
    {
        hasRevealedRelics = true;

        if (uiManager != null)
        {
            uiManager.ShowRelicUI();
        }
        else
        {
            uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
                uiManager.ShowRelicUI();
        }

        if (relicsContainer != null)
        {
            relicsContainer.SetActive(true);

            CanvasGroup canvasGroup = relicsContainer.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = relicsContainer.AddComponent<CanvasGroup>();

            float elapsed = 0f;
            while (elapsed < transitionTime)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / transitionTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        LoadRelicImages();
        UpdateFoundRelics();

        if (revealSound != null)
        {
            AudioSource.PlayClipAtPoint(revealSound, transform.position, 0.5f);
        }
    }

    void LoadRelicImages()
    {
        for (int i = 0; i < relicImages.Length; i++)
        {
            if (relicImages[i] != null)
            {
                relicImages[i].gameObject.SetActive(true);

                if (i < relicSprites.Length && relicSprites[i] != null)
                {
                    relicImages[i].sprite = relicSprites[i];
                    relicImages[i].color = Color.white;
                }
            }
        }
    }

    public void OnRelicFound(int relicIndex)
    {
        if (isTalking && hasRevealedRelics)
        {
            UpdateFoundRelics();
        }
    }

    void UpdateFoundRelics()
    {
        RelicManager relicManager = FindObjectOfType<RelicManager>();
        if (relicManager != null)
        {
            for (int i = 0; i < relicImages.Length; i++)
            {
                if (relicImages[i] != null)
                {
                    relicImages[i].color = relicManager.IsRelicFound(i) ? foundRelicColor : Color.white;
                }
            }
        }
    }

    IEnumerator FadeOutEffect()
    {
        if (mermaidEffect == null) yield break;

        ParticleSystem[] particleSystems = mermaidEffect.GetComponentsInChildren<ParticleSystem>();

        // ParticleSystem'leri durdur
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps.isPlaying)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        // Parçacýklarýn ölmesini bekle
        float maxLifetime = 0f;
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps.main.startLifetime.constant > maxLifetime)
            {
                maxLifetime = ps.main.startLifetime.constant;
            }
        }

        yield return new WaitForSeconds(maxLifetime + 0.5f);

        // Eðer Light component'i varsa, onu da fade-out yap
        Light[] lights = mermaidEffect.GetComponentsInChildren<Light>();
        float elapsedTime = 0f;

        while (elapsedTime < effectFadeTime)
        {
            float progress = elapsedTime / effectFadeTime;

            foreach (Light light in lights)
            {
                if (light != null)
                {
                    light.intensity = Mathf.Lerp(light.intensity, 0f, progress);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Lights'ý kapat
        foreach (Light light in lights)
        {
            if (light != null)
            {
                light.enabled = false;
            }
        }

        // GameObject'i tamamen kapat
        mermaidEffect.SetActive(false);

        // Eðer tamamen yok etmek istiyorsan:
        // Destroy(mermaidEffect);
        // mermaidEffect = null;
    }

    void Update()
    {
        if (isTalking && Input.GetKeyDown(KeyCode.Escape))
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        if (!isTalking) return;

        isTalking = false;

        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (relicsContainer != null)
            relicsContainer.SetActive(false);

        foreach (Image img in relicImages)
        {
            if (img != null)
            {
                img.color = new Color(1, 1, 1, 0);
                img.gameObject.SetActive(false);
            }
        }

        if (dialogueText != null)
        {
            dialogueText.rectTransform.localPosition = originalTextPosition;
            dialogueText.text = "";
        }

        hasRevealedRelics = false;

        SetPlayerControl(true);

        // Efekti fade-out yaparak kaldýr
        if (mermaidEffect != null && mermaidEffect.activeSelf)
        {
            StartCoroutine(FadeOutEffect());
        }

        StartCoroutine(InteractionCooldown());
    }

    void SetPlayerControl(bool enable)
    {
        MonoBehaviour[] playerScripts = FindObjectsOfType<MonoBehaviour>();
        foreach (var script in playerScripts)
        {
            string scriptName = script.GetType().Name;
            if (scriptName.Contains("PlayerMovement") ||
                scriptName.Contains("FirstPerson") ||
                scriptName.Contains("PlayerController"))
            {
                script.enabled = enable;
            }
        }

        Cursor.lockState = enable ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enable;
    }

    IEnumerator InteractionCooldown()
    {
        yield return new WaitForSeconds(1f);
        canInteract = true;
    }
}