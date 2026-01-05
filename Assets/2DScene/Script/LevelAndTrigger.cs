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

    [Header("Cthulhu Referansı")]
    public CthulhuChase cthulhu; // YENİ: Cthulhu referansı

    [Header("Debug")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.green;

    private bool isTriggered = false;

    void Start()
    {
        // Eğer referans atanmamışsa kendimiz oluşturalım
        if (fadeAndText == null)
        {
            GameObject transitionObj = new GameObject("LevelTransition");
            fadeAndText = transitionObj.AddComponent<ScreenFadeAndText>();

            // Varsayılan ayarları yap
            fadeAndText.fadeDuration = 3f;
            fadeAndText.textColor = new Color(0.2f, 0.8f, 1f, 1f); // Deniz mavisi
            fadeAndText.textSize = 72f;
            fadeAndText.textStayDuration = 2f;
        }

        // Cthulhu'yu otomatik bul
        if (cthulhu == null)
        {
            cthulhu = FindObjectOfType<CthulhuChase>();
            if (cthulhu != null)
            {
                Debug.Log("✅ Cthulhu bulundu: " + cthulhu.gameObject.name);
            }
            else
            {
                Debug.LogWarning("⚠️ Cthulhu bulunamadı!");
            }
        }

        // Text'i ayarla
        fadeAndText.displayText = displayText;

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

            // YENİ: Cthulhu'yu durdur
            StopCthulhuChase();

            // Player'ı durdur (opsiyonel)
            PlayerSwimController swimController = other.GetComponent<PlayerSwimController>();
            if (swimController != null)
            {
                swimController.StopSwimming();
                swimController.enabled = false; // Kontrolü tamamen devre dışı bırak
            }

            // Player collider'ını devre dışı bırak
            Collider2D playerCollider = other.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                playerCollider.enabled = false;
            }

            // Fade ve text sequence'ini başlat
            StartTransitionSequence();
        }
    }

    // YENİ: Cthulhu takibini durdur
    void StopCthulhuChase()
    {
        if (cthulhu != null)
        {
            cthulhu.OnLevelCompleted();
            Debug.Log("🛑 Cthulhu takibi durduruldu!");
        }
        else
        {
            Debug.LogWarning("Cthulhu bulunamadı, takip durdurulamadı!");

            // Acil durum: Tüm Cthulhu'ları bul ve durdur
            CthulhuChase[] allCthulhus = FindObjectsOfType<CthulhuChase>();
            foreach (CthulhuChase c in allCthulhus)
            {
                if (c != null)
                {
                    c.OnLevelCompleted();
                    Debug.Log("🛑 Acil durum: " + c.gameObject.name + " durduruldu");
                }
            }
        }
    }

    void StartTransitionSequence()
    {
        Debug.Log("🎬 Geçiş sequence'i başlatılıyor...");

        // Event bağla
        fadeAndText.OnSequenceComplete += OnTransitionComplete;

        // Sequence'i başlat
        fadeAndText.StartFadeAndTextSequence();
    }

    void OnTransitionComplete()
    {
        Debug.Log($"✅ Geçiş tamamlandı, {nextSceneName} sahnesine geçiliyor...");

        // Event'ten çıkar
        fadeAndText.OnSequenceComplete -= OnTransitionComplete;

        // Yeni sahneye geç
        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("❌ Next Scene Name boş!");
            return;
        }

        // Sahne Build Settings'de var mı kontrol et
        if (IsSceneInBuild(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError($"❌ {nextSceneName} sahnesi Build Settings'de bulunamadı!");
            Debug.Log("📋 Build Settings'deki sahneler:");
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                Debug.Log($"  - {sceneName}");
            }
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
                Gizmos.DrawIcon(transform.position, "LevelEndIcon.png", true);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.5f);
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.DrawCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
        }
    }

    // DEBUG için test butonu
    [ContextMenu("Test Transition")]
    void TestTransition()
    {
        if (fadeAndText != null)
        {
            StopCthulhuChase();

            fadeAndText.OnSequenceComplete += () => {
                Debug.Log("Test transition complete!");
            };
            fadeAndText.StartFadeAndTextSequence();
        }
    }
}