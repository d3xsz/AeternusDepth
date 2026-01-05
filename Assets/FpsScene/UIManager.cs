using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Relic Counter UI")]
    [SerializeField] private GameObject relicCounterPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image[] relicIcons;

    [Header("Relic Sprites")]
    [SerializeField] private Sprite[] relicIconSprites;

    [Header("UI Settings")]
    [SerializeField] private string titleString = "RELIC LIST";
    [SerializeField] private Color titleColor = new Color(1f, 0.84f, 0f, 1f);
    [SerializeField] private Color collectedColor = Color.white;
    [SerializeField] private Color notCollectedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private Color stolenColor = Color.red;
    [SerializeField] private float fadeInDuration = 1f;

    [Header("Start State")]
    [SerializeField] private bool startHidden = true;

    [Header("Crab Thief UI")]
    [SerializeField] private AudioClip warningSound;

    private RelicManager relicManager;
    private bool uiVisible = false;
    private CanvasGroup panelCanvasGroup;

    void Start()
    {
        relicManager = FindObjectOfType<RelicManager>();

        if (relicCounterPanel != null)
        {
            panelCanvasGroup = relicCounterPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
                panelCanvasGroup = relicCounterPanel.AddComponent<CanvasGroup>();
        }

        if (titleText != null)
        {
            titleText.text = titleString;
            titleText.color = titleColor;
        }

        SetupIcons();

        if (startHidden)
        {
            HideUI();
        }
        else
        {
            ShowRelicUI();
        }

        UpdateRelicUI();
    }

    void SetupIcons()
    {
        for (int i = 0; i < relicIcons.Length; i++)
        {
            if (relicIcons[i] != null && i < relicIconSprites.Length)
            {
                relicIcons[i].sprite = relicIconSprites[i];
                relicIcons[i].color = notCollectedColor;

                CanvasGroup iconCG = relicIcons[i].GetComponent<CanvasGroup>();
                if (iconCG == null) iconCG = relicIcons[i].gameObject.AddComponent<CanvasGroup>();
                iconCG.alpha = 0f;
            }
        }
    }

    void HideUI()
    {
        uiVisible = false;

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }

        if (titleText != null)
        {
            CanvasGroup titleCG = titleText.GetComponent<CanvasGroup>();
            if (titleCG == null) titleCG = titleText.gameObject.AddComponent<CanvasGroup>();
            titleCG.alpha = 0f;
        }
    }

    public void ShowRelicUI()
    {
        if (uiVisible) return;
        uiVisible = true;
        StartCoroutine(FadeInUI());
    }

    IEnumerator FadeInUI()
    {
        if (relicCounterPanel != null)
            relicCounterPanel.SetActive(true);

        if (titleText != null)
        {
            CanvasGroup titleCG = titleText.GetComponent<CanvasGroup>();
            if (titleCG == null) titleCG = titleText.gameObject.AddComponent<CanvasGroup>();

            float elapsed = 0f;
            while (elapsed < fadeInDuration / 2f)
            {
                titleCG.alpha = Mathf.Lerp(0f, 1f, elapsed / (fadeInDuration / 2f));
                elapsed += Time.deltaTime;
                yield return null;
            }
            titleCG.alpha = 1f;
        }

        for (int i = 0; i < relicIcons.Length; i++)
        {
            if (relicIcons[i] != null)
            {
                CanvasGroup iconCG = relicIcons[i].GetComponent<CanvasGroup>();
                if (iconCG == null) iconCG = relicIcons[i].gameObject.AddComponent<CanvasGroup>();

                float elapsed = 0f;
                while (elapsed < 0.3f)
                {
                    iconCG.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.3f);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                iconCG.alpha = 1f;
                yield return new WaitForSeconds(0.1f);
            }
        }

        if (panelCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
        }

        UpdateRelicUI();
    }

    void Update()
    {
        if (uiVisible && relicManager != null)
        {
            UpdateRelicUI();
        }
    }

    public void OnRelicCollected(int relicIndex)
    {
        if (!uiVisible) return;
        UpdateRelicUI();

        if (relicIndex >= 0 && relicIndex < relicIcons.Length && relicIcons[relicIndex] != null)
        {
            StartCoroutine(PulseIcon(relicIcons[relicIndex].transform));
        }
    }

    // YENÝ: Relic çalýndýðýnda
    public void OnRelicStolen(int relicIndex)
    {
        if (relicIndex >= 0 && relicIndex < relicIcons.Length && relicIcons[relicIndex] != null)
        {
            relicIcons[relicIndex].color = stolenColor;
            StartCoroutine(ShakeIcon(relicIcons[relicIndex].transform));

            if (warningSound != null)
                AudioSource.PlayClipAtPoint(warningSound, Camera.main.transform.position, 0.5f);
        }
    }

    void UpdateRelicUI()
    {
        if (relicManager == null) return;

        for (int i = 0; i < relicIcons.Length; i++)
        {
            if (relicIcons[i] != null)
            {
                // Çalýnmýþ relic kýrmýzý kalýr, diðerleri normal renk
                if (relicIcons[i].color != stolenColor)
                {
                    relicIcons[i].color = relicManager.IsRelicFound(i) ?
                        collectedColor : notCollectedColor;
                }
            }
        }
    }

    IEnumerator PulseIcon(Transform iconTransform)
    {
        Vector3 originalScale = iconTransform.localScale;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float scale = 1f + Mathf.Sin(elapsed * Mathf.PI * 2 / duration) * 0.3f;
            iconTransform.localScale = originalScale * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }
        iconTransform.localScale = originalScale;
    }

    IEnumerator ShakeIcon(Transform iconTransform)
    {
        Vector3 originalPos = iconTransform.localPosition;
        float duration = 1f;
        float elapsed = 0f;
        float shakeIntensity = 3f;

        while (elapsed < duration)
        {
            float x = originalPos.x + Random.Range(-shakeIntensity, shakeIntensity);
            float y = originalPos.y + Random.Range(-shakeIntensity, shakeIntensity);
            iconTransform.localPosition = new Vector3(x, y, originalPos.z);

            shakeIntensity = Mathf.Lerp(3f, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        iconTransform.localPosition = originalPos;
    }
}