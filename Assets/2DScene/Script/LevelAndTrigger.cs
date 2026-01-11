using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelEndTrigger : MonoBehaviour
{
    [Header("Geçiş Ayarları")]
    public string nextSceneName = "Level2";
    public string displayText = "BÖLÜM TAMAMLANDI";

    [Header("Referanslar")]
    public ScreenFadeAndText fadeAndText;

    [Header("Debug")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.green;

    private bool isTriggered = false;
    private OxygenSystem playerOxygenSystem;
    private PlayerSwimController playerSwimController;

    void Start()
    {
        // Trigger'ı ayarla
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("🎯 Level sonuna ulaşıldı!");
            isTriggered = true;

            // Player component'lerini al
            GameObject player = other.gameObject;
            playerOxygenSystem = player.GetComponent<OxygenSystem>();
            playerSwimController = player.GetComponent<PlayerSwimController>();

            // OKSİJEN SİSTEMİNİ DURDUR
            StopOxygenSystem();

            // Player'ı durdur
            DisablePlayerMovement();

            // BEKLE ve SONRA sahne değiştir
            StartCoroutine(WaitAndChangeScene());
        }
    }

    IEnumerator WaitAndChangeScene()
    {
        Debug.Log("⏱️ Bekleme başlıyor...");

        // BEKLEME 1: Fade'in tamamlanmasını bekle
        // FadeAndText varsa, onun süresini bekle
        if (fadeAndText != null)
        {
            // FadeAndText'in tahmini süresini bekle
            float estimatedFadeTime = 3f; // Yaklaşık fade süresi
            Debug.Log($"⏱️ Fade süresi beklenecek: {estimatedFadeTime}s");

            // Event varsa bağla
            fadeAndText.OnSequenceComplete += OnFadeComplete;

            // Fade'i başlat
            fadeAndText.StartFadeAndTextSequence();

            // Ekstra 0.5 saniye daha bekle (güvenlik için)
            yield return new WaitForSeconds(estimatedFadeTime + 0.5f);
        }
        else
        {
            // FadeAndText yoksa, sabit bir süre bekle
            Debug.Log("⚠️ FadeAndText yok, sabit 2 saniye beklenecek");
            yield return new WaitForSeconds(2f);
        }

        Debug.Log("✅ Bekleme tamamlandı, sahne değiştiriliyor...");

        // SAHNE DEĞİŞTİR
        LoadNextScene();
    }

    void OnFadeComplete()
    {
        Debug.Log("✅ Fade animasyonu tamamlandı (event)");
        // Bu event geldiğinde sahne değişimi zaten başlayacak
    }

    void StopOxygenSystem()
    {
        if (playerOxygenSystem != null)
        {
            playerOxygenSystem.StopOxygenSystem();
            Debug.Log("🛑 Oksijen sistemi durduruldu!");
        }
    }

    void DisablePlayerMovement()
    {
        if (playerSwimController != null)
        {
            playerSwimController.StopSwimming();
            playerSwimController.enabled = false;
            Debug.Log("🛑 Player hareketi durduruldu!");
        }

        GameObject player = playerSwimController?.gameObject;
        if (player != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                playerCollider.enabled = false;
            }
        }
    }

    void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("❌ Next Scene Name boş!");
            return;
        }

        if (IsSceneInBuild(nextSceneName))
        {
            // DOĞRUDAN YÜKLE - hiçbir şey bekleme
            SceneManager.LoadScene(nextSceneName);
            Debug.Log($"🔄 {nextSceneName} sahnesine geçildi!");
        }
        else
        {
            Debug.LogError($"❌ {nextSceneName} sahnesi Build Settings'de bulunamadı!");

            // Eğer sahne yoksa player'ı geri aç
            if (playerSwimController != null)
            {
                playerSwimController.enabled = true;
                playerSwimController.StartSwimming();
            }

            if (playerOxygenSystem != null)
            {
                playerOxygenSystem.StartOxygenSystem();
            }

            isTriggered = false;
        }
    }

    bool IsSceneInBuild(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (name == sceneName)
                return true;
        }
        return false;
    }

    void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Gizmos.color = gizmoColor;
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
            }
        }
    }

    [ContextMenu("Test Transition")]
    void TestTransition()
    {
        if (isTriggered) return;

        isTriggered = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerOxygenSystem = player.GetComponent<OxygenSystem>();
            playerSwimController = player.GetComponent<PlayerSwimController>();
        }

        StartCoroutine(WaitAndChangeScene());
    }
}