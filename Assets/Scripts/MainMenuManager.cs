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
    public AudioClip menuMusic; // Menü müziği
    public AudioClip creditsMusic; // Credits müziği (isteğe bağlı)
    public float musicVolume = 0.5f;
    public float sfxVolume = 0.7f;
    private AudioSource audioSource;
    private AudioSource musicSource; // Müzik için ayrı AudioSource

    [Header("Button Effects")]
    public float hoverScale = 1.1f;
    private Dictionary<Button, Vector3> originalButtonScales = new Dictionary<Button, Vector3>();
    private bool isCreditsActive = false;
    private bool isMusicPlaying = false;

    void Start()
    {
        // AudioSource'ları ayarla
        SetupAudio();

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

        // Menü müziğini başlat
        PlayMenuMusic();

        // Fareyi göster
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void SetupAudio()
    {
        // SFX için AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // Müzik için ayrı AudioSource
        GameObject musicObject = new GameObject("MenuMusicSource");
        musicObject.transform.SetParent(transform);
        musicSource = musicObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
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
            PlaySFX(buttonHoverSound);

            // Büyüt
            button.transform.localScale = originalButtonScales[button] * hoverScale;
        }
        else
        {
            // Küçült
            button.transform.localScale = originalButtonScales[button];
        }
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, sfxVolume);
        }
    }

    void PlayMenuMusic()
    {
        if (menuMusic != null && musicSource != null && !isCreditsActive)
        {
            // Eğer zaten menü müziği çalıyorsa, yeniden başlatma
            if (musicSource.isPlaying && musicSource.clip == menuMusic)
                return;

            musicSource.Stop();
            musicSource.clip = menuMusic;
            musicSource.volume = musicVolume;
            musicSource.Play();
            isMusicPlaying = true;

            Debug.Log("🔊 Menu music started");
        }
    }

    void PlayCreditsMusic()
    {
        if (creditsMusic != null && musicSource != null && isCreditsActive)
        {
            // Eğer zaten credits müziği çalıyorsa, yeniden başlatma
            if (musicSource.isPlaying && musicSource.clip == creditsMusic)
                return;

            musicSource.Stop();
            musicSource.clip = creditsMusic;
            musicSource.volume = musicVolume * 0.7f; // Credits müziği biraz daha kısık
            musicSource.Play();
            isMusicPlaying = true;

            Debug.Log("🔊 Credits music started");
        }
    }

    void StopAllMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            isMusicPlaying = false;
            Debug.Log("🔇 All music stopped");
        }
    }

    void PlayClickSound()
    {
        PlaySFX(buttonClickSound);
    }

    public void StartGame()
    {
        Debug.Log("Oyun başlatılıyor...");

        // Müziği durdur
        StopAllMusic();

        // Tıklama sesi
        PlaySFX(buttonClickSound);

        // Küçük bekleme (sesin çalınması için)
        StartCoroutine(LoadGameSceneWithDelay(0.3f));
    }

    IEnumerator LoadGameSceneWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        Debug.Log("Ayarlar açılıyor...");
        PlaySFX(buttonClickSound);
        // Settings panelini buraya ekleyebilirsin
    }

    public void OpenCredits()
    {
        if (isCreditsActive) return;

        Debug.Log("Credits açılıyor...");
        PlaySFX(buttonClickSound);

        isCreditsActive = true;

        // Credits müziğini başlat
        PlayCreditsMusic();

        // Credits panelini aç
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);

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
        PlaySFX(buttonClickSound);

        isCreditsActive = false;

        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        // Menü müziğini başlat
        PlayMenuMusic();

        // Credits ile ilgili coroutine'leri durdur
        StopAllCoroutines();
    }

    public void ExitGame()
    {
        Debug.Log("Oyun kapatılıyor...");
        PlaySFX(buttonClickSound);

        // Müziği durdur
        StopAllMusic();

        // Küçük bekleme
        StartCoroutine(QuitWithDelay(0.3f));
    }

    IEnumerator QuitWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

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

        // Müzik ayarlarını kontrol et (debug için)
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (musicSource.isPlaying)
            {
                StopAllMusic();
            }
            else
            {
                if (isCreditsActive)
                    PlayCreditsMusic();
                else
                    PlayMenuMusic();
            }
        }

        // Ses seviyesi ayarları (debug için)
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            musicVolume = Mathf.Clamp01(musicVolume + 0.1f);
            if (musicSource != null)
            {
                // Credits müziğinde farklı volume kullanıyoruz
                if (isCreditsActive && musicSource.clip == creditsMusic)
                    musicSource.volume = musicVolume * 0.7f;
                else
                    musicSource.volume = musicVolume;
            }
        }

        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            musicVolume = Mathf.Clamp01(musicVolume - 0.1f);
            if (musicSource != null)
            {
                // Credits müziğinde farklı volume kullanıyoruz
                if (isCreditsActive && musicSource.clip == creditsMusic)
                    musicSource.volume = musicVolume * 0.7f;
                else
                    musicSource.volume = musicVolume;
            }
        }
    }

    // Public method'lar ses ayarları için
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            // Credits müziğinde farklı volume kullanıyoruz
            if (isCreditsActive && musicSource.clip == creditsMusic)
                musicSource.volume = musicVolume * 0.7f;
            else
                musicSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
}