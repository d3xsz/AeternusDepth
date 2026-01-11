using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int score = 0;
    public AutoScrollCamera scrollCamera;
    public Transform player;

    // TextMeshPro referansları
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI speedLevelText;
    public TextMeshProUGUI deathCountText;
    public TextMeshProUGUI survivalTimerText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public float difficultyIncreaseInterval = 10f;
    public float cameraSpeedIncrease = 0.5f;

    private bool isGameOver = false;
    private float difficultyTimer = 0f;
    private int deathCount = 0;
    private Vector3 originalPlayerPosition = new Vector3(0, 2, 0);
    private PlayerRespawn playerRespawn;

    // Survival Timer için değişkenler
    private float totalSurvivalTime = 0f;
    private float currentLifeStartTime = 0f;
    private bool isTimerRunning = false;
    private float bestSurvivalTime = 0f;

    // Property'ler
    public int DeathCount
    {
        get { return deathCount; }
        private set { deathCount = value; }
    }

    public float TotalSurvivalTime
    {
        get { return totalSurvivalTime; }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        InitializeGame();

        // Component'leri bul
        FindComponents();

        // Event bağlantılarını kur
        SetupEventListeners();

        // Başlangıçta UI'ları güncelle
        UpdateDeathCountUI();
        UpdateSurvivalTimerUI();

        // Oyun başladığında timer'ı başlat
        StartSurvivalTimer();

        Debug.Log("✅ GameManager başlatıldı");
    }

    void FindComponents()
    {
        if (scrollCamera == null) scrollCamera = FindObjectOfType<AutoScrollCamera>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("✅ Player bulundu: " + player.name);
            }
            else
            {
                Debug.LogError("❌ Player tag'ine sahip GameObject bulunamadı!");
            }
        }
    }

    void SetupEventListeners()
    {
        if (player != null)
        {
            playerRespawn = player.GetComponent<PlayerRespawn>();
            if (playerRespawn != null)
            {
                // Tüm event'leri bağla
                playerRespawn.OnRespawnComplete += HandleRespawnComplete;
                playerRespawn.OnDeath += HandlePlayerDeath;
                playerRespawn.OnRespawnStart += HandleRespawnStart;

                Debug.Log("✅ PlayerRespawn event'leri bağlandı");
            }
            else
            {
                Debug.LogError("❌ PlayerRespawn scripti bulunamadı!");
            }
        }
    }

    void OnDestroy()
    {
        if (playerRespawn != null)
        {
            playerRespawn.OnRespawnComplete -= HandleRespawnComplete;
            playerRespawn.OnDeath -= HandlePlayerDeath;
            playerRespawn.OnRespawnStart -= HandleRespawnStart;
        }
    }

    void InitializeGame()
    {
        score = 0;
        deathCount = 0;
        totalSurvivalTime = 0f;
        isGameOver = false;
        difficultyTimer = 0f;

        UpdateUI();
        UpdateDeathCountUI();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            Debug.Log("✅ Game Over paneli kapatıldı");
        }

        Time.timeScale = 1f;

        // Başlangıçta potları oluştur
        RespawnAllPotsImmediately();

        Debug.Log("🎮 Oyun başlatıldı");
    }

    void Update()
    {
        if (isGameOver) return;

        // Zorluk artışı (sadece kamera hızı artacak)
        difficultyTimer += Time.deltaTime;
        if (difficultyTimer >= difficultyIncreaseInterval)
        {
            IncreaseDifficulty();
            difficultyTimer = 0f;
        }

        // UI güncelleme
        UpdateUI();
        UpdateSurvivalTimer();

        // DEBUG: Test için
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("🔄 R tuşu - Test Respawn");
            PlayerDied();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("⏱️ T tuşu - Timer testi");
            Debug.Log($"Toplam Süre: {FormatTime(totalSurvivalTime)}, Çalışıyor: {isTimerRunning}");
        }
    }

    void UpdateUI()
    {
        // Score güncelle
        if (scoreText != null)
            scoreText.text = "SCORE: " + score.ToString();

        // Speed level güncelle
        if (speedLevelText != null && scrollCamera != null)
        {
            int speedLevel = scrollCamera.GetSpeedLevel();
            float speedMultiplier = Mathf.Pow(2, speedLevel);
            speedLevelText.text = "SPEED: x" + speedMultiplier.ToString("F1");
        }
    }

    void UpdateSurvivalTimer()
    {
        if (isTimerRunning && survivalTimerText != null)
        {
            // Anlık toplam süreyi hesapla
            float currentTotalTime = totalSurvivalTime + (Time.time - currentLifeStartTime);

            // UI'ı güncelle
            UpdateSurvivalTimerUI(currentTotalTime);
        }
    }

    void UpdateSurvivalTimerUI(float currentTime = -1)
    {
        if (survivalTimerText != null)
        {
            float timeToDisplay = currentTime >= 0 ? currentTime : totalSurvivalTime;

            // Format: "TIME: 1:23.45"
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeToDisplay);
            string formattedTime = string.Format("{0}:{1:00}.{2:00}",
                (int)timeSpan.TotalMinutes,
                timeSpan.Seconds,
                timeSpan.Milliseconds / 10);

            survivalTimerText.text = "TIME: " + formattedTime;
        }
    }

    void UpdateDeathCountUI()
    {
        if (deathCountText != null)
        {
            deathCountText.text = "DEATHS: " + deathCount.ToString();
        }
    }

    // Survival Timer kontrol metodları
    void StartSurvivalTimer()
    {
        if (!isTimerRunning)
        {
            isTimerRunning = true;
            currentLifeStartTime = Time.time;
            Debug.Log($"⏱️ Survival Timer başladı: {FormatTime(totalSurvivalTime)}'dan devam");
        }
    }

    void StopSurvivalTimer()
    {
        if (isTimerRunning)
        {
            isTimerRunning = false;
            totalSurvivalTime += (Time.time - currentLifeStartTime);
            Debug.Log($"⏱️ Survival Timer durduruldu. Bu hayat: {Time.time - currentLifeStartTime:F2}s, Toplam: {FormatTime(totalSurvivalTime)}");
        }
    }

    // Event Handler'lar
    void HandlePlayerDeath()
    {
        deathCount++;
        Debug.Log($"💀 Ölüm sayacı arttı: {deathCount}");
        UpdateDeathCountUI();

        // Timer'ı durdur
        StopSurvivalTimer();
    }

    void HandleRespawnStart()
    {
        Debug.Log("👻 Respawn başladı - Hayalet animasyonu");
        // Timer zaten durmuş durumda
    }

    void HandleRespawnComplete()
    {
        Debug.Log("✅ PlayerRespawn tamamlandı, timer yeniden başlıyor...");

        // Timer'ı yeniden başlat
        StartSurvivalTimer();

        // Potları respawn et
        StartCoroutine(DelayedPotRespawn());
    }

    IEnumerator DelayedPotRespawn()
    {
        yield return new WaitForSeconds(0.5f);
        RespawnAllPotsImmediately();
    }

    // Public metodlar
    public void PlayerDied()
    {
        if (isGameOver) return;

        Debug.Log($"💀 PLAYER ÖLDÜ! Toplam ölüm: {deathCount + 1}");

        // PlayerRespawn scriptini bul ve hayalet respawn başlat
        if (player != null)
        {
            PlayerRespawn respawn = player.GetComponent<PlayerRespawn>();
            if (respawn != null)
            {
                Debug.Log("👻 Hayalet Respawn başlatılıyor...");
                respawn.StartGhostRespawn(player.position);
            }
            else
            {
                Debug.LogWarning("⚠️ PlayerRespawn scripti bulunamadı, direkt respawn...");
                RespawnPlayerImmediate();
            }
        }
    }

    void RespawnPlayerImmediate()
    {
        Debug.Log("⚡ DIRECT RESPAWN BAŞLIYOR ===");

        ClearAllAnchors();

        if (player != null)
        {
            PlayerSwimController swimController = player.GetComponent<PlayerSwimController>();
            if (swimController != null) swimController.ResetAllEffects();

            PlayerRespawn playerRespawn = player.GetComponent<PlayerRespawn>();
            if (playerRespawn != null)
            {
                playerRespawn.spawnPosition = originalPlayerPosition;
                playerRespawn.RespawnPlayer();
            }
            else
            {
                player.position = originalPlayerPosition;
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;

                if (swimController != null) swimController.StartSwimming();

                // Timer'ı başlat (event sistemi yoksa)
                StartSurvivalTimer();
            }
        }

        // Cthulhu yok artık, kaldırıldı
        if (scrollCamera != null) scrollCamera.InstantResetToPlayer();

        // Potları hemen respawn et
        RespawnAllPotsImmediately();

        Debug.Log("=== DIRECT RESPAWN TAMAMLANDI ===");
    }

    void RespawnAllPotsImmediately()
    {
        Debug.Log("🔄 POTLAR YENİDEN OLUŞTURULUYOR...");

        // Önce mevcut tüm potları temizle
        ClearAllPots();

        // PotManager ile yeniden oluştur
        if (PotManager.Instance != null)
        {
            PotManager.Instance.RespawnAllPots();
            Debug.Log("✅ PotManager ile potlar oluşturuldu");
        }
        else
        {
            Debug.LogError("❌ PotManager.Instance bulunamadı!");
            ManualRespawnPots();
        }
    }

    void ClearAllPots()
    {
        GameObject[] allPots = GameObject.FindGameObjectsWithTag("Pot");
        int potCount = allPots.Length;
        foreach (GameObject pot in allPots)
        {
            if (pot != null) Destroy(pot);
        }
        if (potCount > 0) Debug.Log($"🗑️ {potCount} pot temizlendi");
    }

    void ManualRespawnPots()
    {
        PotManager potManager = FindObjectOfType<PotManager>();
        if (potManager != null && potManager.spawnPositions != null)
        {
            int spawnedCount = 0;
            foreach (Transform spawnPoint in potManager.spawnPositions)
            {
                if (spawnPoint != null && potManager.potPrefab != null)
                {
                    GameObject pot = Instantiate(potManager.potPrefab,
                                                spawnPoint.position,
                                                Quaternion.identity);

                    PotItem potItem = pot.GetComponent<PotItem>();
                    if (potItem != null)
                    {
                        // DÜZELTME: UnityEngine.Random kullan
                        bool isPoison = UnityEngine.Random.Range(0, 100) < potManager.poisonPotChance;
                        potItem.ChangePotType(isPoison);
                        spawnedCount++;
                    }
                }
            }
            Debug.Log($"✅ Manuel olarak {spawnedCount} pot oluşturuldu");
        }
    }

    void ClearAllAnchors()
    {
        AnchorSpawner[] allSpawners = FindObjectsOfType<AnchorSpawner>();
        foreach (AnchorSpawner spawner in allSpawners)
            if (spawner != null) spawner.ClearAllAnchors();

        GameObject[] allAnchors = GameObject.FindGameObjectsWithTag("Anchor");
        foreach (GameObject anchor in allAnchors)
            Destroy(anchor);
    }

    void IncreaseDifficulty()
    {
        if (scrollCamera != null)
        {
            scrollCamera.SetScrollSpeed(scrollCamera.GetCurrentSpeed() + cameraSpeedIncrease);
            Debug.Log($"📈 Zorluk arttı! Kamera hızı: {scrollCamera.GetCurrentSpeed():F2}");
        }
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateUI();
        Debug.Log($"💰 Score eklendi: +{points}, Toplam: {score}");
    }

    public void GameOver()
    {
        StopSurvivalTimer();
        isGameOver = true;

        // En iyi süreyi güncelle
        if (totalSurvivalTime > bestSurvivalTime)
        {
            bestSurvivalTime = totalSurvivalTime;
            Debug.Log($"🎉 YENİ EN İYİ SÜRE: {FormatTime(bestSurvivalTime)}");
        }

        // Game Over panelini göster
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (finalScoreText != null)
            {
                TimeSpan totalTime = TimeSpan.FromSeconds(totalSurvivalTime);
                string timeString = FormatTime(totalSurvivalTime);

                finalScoreText.text =
                    $"<size=40><b>GAME OVER</b></size>\n\n" +
                    $"<size=30>SCORE: {score}\n" +
                    $"TIME: {timeString}\n" +
                    $"DEATHS: {deathCount}</size>\n\n" +
                    $"<size=20>BEST TIME: {FormatTime(bestSurvivalTime)}</size>";
            }
        }

        Debug.Log($"🛑 GAME OVER - Score: {score}, Time: {FormatTime(totalSurvivalTime)}, Deaths: {deathCount}");
    }

    // Yardımcı metodlar
    string FormatTime(float seconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        return string.Format("{0}:{1:00}.{2:00}",
            (int)timeSpan.TotalMinutes,
            timeSpan.Seconds,
            timeSpan.Milliseconds / 10);
    }

    // DEBUG için
    void OnGUI()
    {
        if (showDebugInfo)
        {
            GUI.Label(new Rect(10, 100, 300, 20), $"Timer Running: {isTimerRunning}");
            GUI.Label(new Rect(10, 120, 300, 20), $"Total Time: {FormatTime(totalSurvivalTime)}");
            GUI.Label(new Rect(10, 140, 300, 20), $"Current Life: {Time.time - currentLifeStartTime:F2}s");
        }
    }

    // Debug değişkeni
    private bool showDebugInfo = false;

    // Debug için Inspector'dan kontrol
    [ContextMenu("Toggle Debug Info")]
    void ToggleDebugInfo()
    {
        showDebugInfo = !showDebugInfo;
        Debug.Log($"Debug Info: {showDebugInfo}");
    }

    [ContextMenu("Test Game Over")]
    void TestGameOver()
    {
        GameOver();
    }
}