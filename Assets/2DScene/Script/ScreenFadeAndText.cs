using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ScreenFadeAndText : MonoBehaviour
{
    [Header("Fade Ayarları")]
    public float fadeDuration = 3f;
    public Color fadeColor = Color.black;
    public float fadeDelay = 0f;

    [Header("Yazı Ayarları")]
    public string displayText = "BÖLÜM TAMAMLANDI";
    public float textAppearDelay = 1f;
    public float textRevealDuration = 2f;
    public Color textColor = new Color(0.2f, 0.8f, 1f, 1f); // Deniz mavisi
    public TMP_FontAsset textFont;
    public float textSize = 72f;
    public float textStayDuration = 2f;

    [Header("UI Katmanları")]
    public int fadeSortingOrder = 100;
    public int textSortingOrder = 101;

    [Header("Event")]
    public System.Action OnSequenceComplete;

    // Private referanslar
    private Image fadeImage;
    private TextMeshProUGUI displayTextUI;
    private CanvasGroup textCanvasGroup;
    private GameObject fadeCanvasObj;
    private GameObject textCanvasObj;
    private bool isSequenceActive = false;

    void Awake()
    {
        // DontDestroyOnLoad yapmak istersen
        // DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Başlangıçta canvas'ları oluşturma
        // Sadece StartFadeAndTextSequence çağrıldığında oluştur
    }

    public void StartFadeAndTextSequence()
    {
        if (isSequenceActive) return;

        isSequenceActive = true;
        Debug.Log("🎬 Ekran kararma ve yazı sequence'i başladı");

        // Canvas'ları oluştur
        CreateFadeCanvas();
        CreateTextCanvas();

        fadeCanvasObj.SetActive(true);
        textCanvasObj.SetActive(true);

        StartCoroutine(SequenceCoroutine());
    }

    void CreateFadeCanvas()
    {
        // Fade için Canvas
        fadeCanvasObj = new GameObject("FadeCanvas");
        Canvas canvas = fadeCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = fadeSortingOrder;

        CanvasScaler scaler = fadeCanvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        CanvasGroup canvasGroup = fadeCanvasObj.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // Fade Image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(fadeCanvasObj.transform);

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        fadeImage.raycastTarget = false;

        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    void CreateTextCanvas()
    {
        // Yazı için Canvas
        textCanvasObj = new GameObject("TextCanvas");
        Canvas textCanvas = textCanvasObj.AddComponent<Canvas>();
        textCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        textCanvas.sortingOrder = textSortingOrder;

        CanvasScaler textScaler = textCanvasObj.AddComponent<CanvasScaler>();
        textScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        textScaler.referenceResolution = new Vector2(1920, 1080);

        textCanvasGroup = textCanvasObj.AddComponent<CanvasGroup>();
        textCanvasGroup.alpha = 0f;
        textCanvasGroup.blocksRaycasts = false;
        textCanvasGroup.interactable = false;

        // TextMeshPro Text
        GameObject textObj = new GameObject("DisplayText");
        textObj.transform.SetParent(textCanvasObj.transform);

        displayTextUI = textObj.AddComponent<TextMeshProUGUI>();
        displayTextUI.text = displayText;
        displayTextUI.color = textColor;
        displayTextUI.fontSize = textSize;
        displayTextUI.alignment = TextAlignmentOptions.Center;
        displayTextUI.enableWordWrapping = true;
        displayTextUI.overflowMode = TextOverflowModes.Overflow;
        displayTextUI.raycastTarget = false;

        if (textFont != null)
        {
            displayTextUI.font = textFont;
        }

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.4f);
        textRect.anchorMax = new Vector2(0.9f, 0.6f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
    }

    IEnumerator SequenceCoroutine()
    {
        // Bekleme süresi
        if (fadeDelay > 0)
        {
            yield return new WaitForSeconds(fadeDelay);
        }

        Debug.Log("1️⃣ Ekran kararmaya başlıyor...");

        // Kararma efekti
        yield return StartCoroutine(FadeScreen(0f, 1f, fadeDuration));

        Debug.Log("2️⃣ Yazı gösteriliyor...");

        // Yazı efekti
        yield return StartCoroutine(ShowText());

        // Yazının kalma süresi
        yield return new WaitForSeconds(textStayDuration);

        Debug.Log("3️⃣ Yazı kayboluyor...");

        // Yazıyı yavaşça kaybet
        yield return StartCoroutine(HideText(1f));

        Debug.Log("✅ Sequence tamamlandı");

        // Event tetikle
        OnSequenceComplete?.Invoke();

        // Canvas'ları temizle
        Cleanup();
    }

    IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (fadeImage != null)
            {
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, endAlpha);
        }

        Debug.Log("🌑 Ekran tamamen karardı");
    }

    IEnumerator ShowText()
    {
        if (textCanvasObj == null || displayTextUI == null) yield break;

        yield return new WaitForSeconds(textAppearDelay);

        string fullText = displayTextUI.text;
        displayTextUI.text = "";
        displayTextUI.maxVisibleCharacters = 0;

        float elapsed = 0f;

        while (elapsed < textRevealDuration)
        {
            textCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / textRevealDuration);

            int charsToShow = Mathf.FloorToInt((elapsed / textRevealDuration) * fullText.Length);
            displayTextUI.maxVisibleCharacters = charsToShow;
            displayTextUI.text = fullText.Substring(0, Mathf.Min(charsToShow, fullText.Length));

            elapsed += Time.deltaTime;
            yield return null;
        }

        displayTextUI.text = fullText;
        displayTextUI.maxVisibleCharacters = fullText.Length;
        textCanvasGroup.alpha = 1f;

        Debug.Log("✅ Yazı tamamen gösterildi");
    }

    IEnumerator HideText(float duration)
    {
        float elapsed = 0f;
        float startAlpha = textCanvasGroup.alpha;

        while (elapsed < duration)
        {
            textCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        textCanvasGroup.alpha = 0f;
    }

    void Cleanup()
    {
        // Canvas'ları yok et
        if (fadeCanvasObj != null)
        {
            Destroy(fadeCanvasObj);
        }

        if (textCanvasObj != null)
        {
            Destroy(textCanvasObj);
        }

        isSequenceActive = false;
    }

    void OnDestroy()
    {
        Cleanup();
    }
}