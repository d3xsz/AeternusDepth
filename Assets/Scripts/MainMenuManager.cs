using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu UI")]
    public Button startButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button exitButton;
    public TextMeshProUGUI gameTitleText;

    [Header("Credits")]
    public GameObject creditsPanel; // Tam ekran siyah panel
    public TextMeshProUGUI creditsText; // Kayacak olan yazı
    public float scrollSpeed = 30f;

    [Header("Scene Settings")]
    public string gameSceneName = "GameScene";

    [Header("Audio")]
    public AudioClip buttonClickSound;
    public AudioClip buttonHoverSound;
    private AudioSource audioSource;

    [Header("Button Effects")]
    public float hoverScale = 1.1f;
    private Dictionary<Button, Vector3> originalButtonScales = new Dictionary<Button, Vector3>();
    private bool isCreditsActive = false;

    void Start()
    {
        // Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Buton ayarları
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (creditsButton != null) creditsButton.onClick.AddListener(OpenCredits);
        if (exitButton != null) exitButton.onClick.AddListener(ExitGame);

        // Hover efektleri
        SetupButtonHoverEffects();

        // Credits paneli başlangıçta kapalı
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);

            // Credits text'i panelin en altına al
            if (creditsText != null)
            {
                RectTransform textRT = creditsText.rectTransform;
                textRT.anchoredPosition = new Vector2(0, -Screen.height * 0.5f - textRT.sizeDelta.y);
            }
        }

        // Fareyi göster
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void SetupButtonHoverEffects()
    {
        Button[] buttons = { startButton, settingsButton, creditsButton, exitButton };

        foreach (Button btn in buttons)
        {
            if (btn != null)
            {
                // Orijinal boyutu kaydet
                originalButtonScales[btn] = btn.transform.localScale;

                // EventTrigger ekle
                EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
                if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();

                // Hover giriş
                EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                enterEntry.eventID = EventTriggerType.PointerEnter;
                enterEntry.callback.AddListener((data) =>
                {
                    OnButtonHover(btn, true);
                });
                trigger.triggers.Add(enterEntry);

                // Hover çıkış
                EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                exitEntry.eventID = EventTriggerType.PointerExit;
                exitEntry.callback.AddListener((data) =>
                {
                    OnButtonHover(btn, false);
                });
                trigger.triggers.Add(exitEntry);

                // Click sesi ekle
                btn.onClick.AddListener(PlayClickSound);
            }
        }
    }

    void OnButtonHover(Button button, bool isEntering)
    {
        if (button == null || !originalButtonScales.ContainsKey(button))
            return;

        if (isEntering)
        {
            // Hover sesi
            if (buttonHoverSound != null && audioSource != null)
                audioSource.PlayOneShot(buttonHoverSound);

            // Büyüt
            button.transform.localScale = originalButtonScales[button] * hoverScale;
        }
        else
        {
            // Küçült
            button.transform.localScale = originalButtonScales[button];
        }
    }

    void PlayClickSound()
    {
        if (buttonClickSound != null && audioSource != null)
            audioSource.PlayOneShot(buttonClickSound);
    }

    public void StartGame()
    {
        Debug.Log("Oyun başlatılıyor...");

        if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        Debug.Log("Ayarlar açılıyor...");
        // Settings panelini buraya ekleyebilirsin
    }

    public void OpenCredits()
    {
        if (isCreditsActive) return;

        Debug.Log("Credits açılıyor...");

        // Credits panelini aç
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
            isCreditsActive = true;

            // Credits text'i en alta al
            if (creditsText != null)
            {
                RectTransform textRT = creditsText.rectTransform;
                textRT.anchoredPosition = new Vector2(0, -Screen.height * 0.5f - textRT.sizeDelta.y);

                // Kaydırmayı başlat
                StartCoroutine(ScrollCreditsText());
            }
        }
    }

    IEnumerator ScrollCreditsText()
    {
        if (creditsText == null) yield break;

        RectTransform textRT = creditsText.rectTransform;
        Vector2 startPos = new Vector2(0, -Screen.height * 0.5f - textRT.sizeDelta.y);
        Vector2 endPos = new Vector2(0, Screen.height * 0.5f + textRT.sizeDelta.y);

        textRT.anchoredPosition = startPos;

        float duration = Vector2.Distance(startPos, endPos) / scrollSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < duration && isCreditsActive)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Yumuşak kaydırma
            textRT.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            // ESC veya tıklama ile çık
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
            {
                CloseCredits();
                yield break;
            }

            yield return null;
        }

        // Yazılar bittiğinde 2 saniye bekle ve kapat
        yield return new WaitForSeconds(2f);
        CloseCredits();
    }

    public void CloseCredits()
    {
        if (!isCreditsActive) return;

        Debug.Log("Credits kapatılıyor...");

        isCreditsActive = false;

        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        // Tüm coroutine'leri durdur
        StopAllCoroutines();
    }

    public void ExitGame()
    {
        Debug.Log("Oyun kapatılıyor...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    void Update()
    {
        // ESC ile credits'i kapat
        if (isCreditsActive && Input.GetKeyDown(KeyCode.Escape))
            CloseCredits();
    }
}