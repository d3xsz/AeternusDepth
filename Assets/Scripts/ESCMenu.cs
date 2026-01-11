using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ESCMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject escMenuPanel;  // Canvas paneli
    public Transform rewardsContent;
    public GameObject rewardItemPrefab;
    public TextMeshProUGUI totalStatsText;
    public TextMeshProUGUI rewardsText;

    [Header("Hover UI Reference")]
    public GameObject hoverEnergyUI;

    [Header("Audio Settings")]
    public AudioClip buttonClickSound;
    public AudioClip menuAmbienceSound;
    [Range(0f, 1f)] public float ambienceVolume = 0.5f;
    [Range(0f, 1f)] public float buttonSoundVolume = 0.7f;

    private AudioSource audioSource;
    private bool wasHoverUIActive = true;

    // Diğer scriptler için public özellikler
    public bool isMenuOpen { get; private set; } = false;

    void Start()
    {
        // AudioSource oluştur
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.volume = ambienceVolume;

        // Buton event'lerini ayarla
        SetupButtons();

        // ESC menüsünü başlangıçta kapat
        if (escMenuPanel != null)
            escMenuPanel.SetActive(false);

        // EventSystem kontrolü
        EnsureEventSystemExists();
    }

    void SetupButtons()
    {
        // Tüm butonları bul ve click event'lerini ayarla
        Button[] buttons = escMenuPanel.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            // Buton adına göre fonksiyon ata
            string buttonName = button.gameObject.name.ToLower();

            if (buttonName.Contains("continue") || buttonName.Contains("resume"))
                button.onClick.AddListener(ResumeGame);
            else if (buttonName.Contains("main") || buttonName.Contains("menu"))
                button.onClick.AddListener(MainMenu);
            else if (buttonName.Contains("quit") || buttonName.Contains("exit"))
                button.onClick.AddListener(QuitGame);

            // Tüm butonlara click sesi ekle
            button.onClick.AddListener(PlayButtonClickSound);
        }
    }

    void EnsureEventSystemExists()
    {
        if (EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    void Update()
    {
        // Reward panel açıksa ESC'yi engelle
        RewardUIManager rewardManager = FindObjectOfType<RewardUIManager>();
        if (rewardManager != null && rewardManager.IsRewardPanelOpen())
            return;

        // ESC tuşu ile menüyü aç/kapat
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isMenuOpen)
                OpenMenu();
            else
                CloseMenu();
        }
    }

    void OpenMenu()
    {
        // Durumu güncelle
        isMenuOpen = true;

        // Canvas'ı aç
        escMenuPanel.SetActive(true);

        // Ambience müziğini başlat
        PlayAmbience();

        // Hover UI'ı gizle
        if (hoverEnergyUI != null)
        {
            wasHoverUIActive = hoverEnergyUI.activeSelf;
            hoverEnergyUI.SetActive(false);
        }

        // Timer'ı durdur
        if (GameTimer.Instance != null)
            GameTimer.Instance.PauseTimer();

        // Oyunu durdur
        Time.timeScale = 0f;

        // Fareyi göster
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Rewards'ı güncelle
        UpdateRewardsDisplay();
    }

    void CloseMenu()
    {
        // Durumu güncelle
        isMenuOpen = false;

        // Ambience müziğini durdur
        StopAmbience();

        // Canvas'ı kapat
        escMenuPanel.SetActive(false);

        // Hover UI'ı geri getir
        if (hoverEnergyUI != null && wasHoverUIActive)
            hoverEnergyUI.SetActive(true);

        // Timer'ı devam ettir
        if (GameTimer.Instance != null)
            GameTimer.Instance.ResumeTimer();

        // Oyunu devam ettir
        Time.timeScale = 1f;

        // Fareyi gizle
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Diğer scriptler için public ToggleMenu metodu
    public void ToggleMenu()
    {
        if (!isMenuOpen)
            OpenMenu();
        else
            CloseMenu();
    }

    void PlayAmbience()
    {
        // Eğer müzik çalmıyorsa ve müzik dosyası varsa
        if (!audioSource.isPlaying && menuAmbienceSound != null)
        {
            audioSource.clip = menuAmbienceSound;
            audioSource.Play();
        }
    }

    void StopAmbience()
    {
        // Müzik çalıyorsa durdur
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }

    void UpdateRewardsDisplay()
    {
        if (rewardsText != null && PlayerStats.Instance != null)
        {
            rewardsText.text = "ACQUIRED UPGRADES\n\n";
            List<string> rewards = PlayerStats.Instance.GetAllAcquiredRewards();

            if (rewards.Count > 0)
            {
                foreach (string reward in rewards)
                    rewardsText.text += $"• {reward}\n\n";
            }
            else
            {
                rewardsText.text += "• No upgrades acquired yet\n";
            }
        }

        if (totalStatsText != null && PlayerStats.Instance != null)
            totalStatsText.text = PlayerStats.Instance.GetTotalStatsSummary();
    }

    public void ResumeGame()
    {
        // Continue butonuna basıldığında menüyü kapat
        CloseMenu();
    }

    public void MainMenu()
    {
        // Müziği durdur
        StopAmbience();

        // Timer'ı durdur
        if (GameTimer.Instance != null)
            GameTimer.Instance.StopTimer();

        // Oyunu devam ettir
        Time.timeScale = 1f;

        // Hover UI'ı geri getir
        if (hoverEnergyUI != null)
            hoverEnergyUI.SetActive(true);

        // Main menu sahnesine geç
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        // Müziği durdur
        StopAmbience();

        // Oyunu devam ettir
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void PlayButtonClickSound()
    {
        if (buttonClickSound != null)
        {
            // Geçici bir AudioSource oluştur (ana AudioSource'u etkilememek için)
            AudioSource tempSource = gameObject.AddComponent<AudioSource>();
            tempSource.playOnAwake = false;
            tempSource.volume = buttonSoundVolume;
            tempSource.PlayOneShot(buttonClickSound);

            // Ses bittikten sonra temizle
            Destroy(tempSource, buttonClickSound.length + 0.1f);
        }
    }

    // Canvas aktif/pasif olduğunda otomatik olarak müziği kontrol et
    void OnEnable()
    {
        // Component enable olduğunda bir şey yapma
    }

    void OnDisable()
    {
        // Component disable olduğunda müziği durdur
        StopAmbience();
    }

    void OnDestroy()
    {
        // Object destroy olduğunda müziği durdur
        StopAmbience();
    }

    // Diğer scriptler için public metod
    public bool IsMenuOpen()
    {
        return isMenuOpen;
    }
}