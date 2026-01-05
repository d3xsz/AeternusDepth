using UnityEngine;
using System.Collections;

public class WaterCurrent : MonoBehaviour
{
    [Header("Akıntı Kuvveti")]
    public Vector2 currentForce = new Vector2(-5f, 0f);
    public float forceMultiplier = 80f;

    [Header("Hızlandırma Etkisi")] // DEĞİŞTİ: Yavaşlatma -> Hızlandırma
    public bool applySpeedEffect = true; // DEĞİŞTİ: applySlowEffect -> applySpeedEffect
    public float speedBoostMultiplier = 1.5f; // DEĞİŞTİ: slowAmount -> speedBoostMultiplier (1.0 = normal, 1.5 = %50 daha hızlı)
    public float speedDuration = 1.5f;
    public float speedRefreshRate = 0.5f; // DEĞİŞTİ: slowRefreshRate -> speedRefreshRate

    [Header("Görsel")]
    public SpriteRenderer currentVisual;
    public Color activeColor = new Color(0.3f, 0.5f, 1f, 0.8f);
    public Color speedBoostColor = new Color(0f, 1f, 0.5f, 0.8f); // YENİ: Hızlandırma rengi (yeşilimsi)

    private Color originalColor;
    private GameObject currentPlayer;
    private bool isPlayerInside = false;
    private Coroutine speedCoroutine; // DEĞİŞTİ: slowCoroutine -> speedCoroutine
    private float lastSpeedTime = 0f; // DEĞİŞTİ: lastSlowTime -> lastSpeedTime

    void Start()
    {
        if (currentVisual != null)
        {
            originalColor = currentVisual.color;
        }

        Debug.Log($"WaterCurrent başlatıldı. Speed Boost: {applySpeedEffect}, Force: {currentForce}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player akıntıya GİRDİ!");
            currentPlayer = other.gameObject;
            isPlayerInside = true;

            // Görsel efekti - Hızlandırma için yeşilimsi renk
            if (currentVisual != null)
            {
                currentVisual.color = applySpeedEffect ? speedBoostColor : activeColor;
            }

            // Hızlandırma efekti başlat
            if (applySpeedEffect)
            {
                StartContinuousSpeedBoost(other.gameObject);
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Akıntı kuvvetini sürekli uygula
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 force = currentForce * forceMultiplier * Time.fixedDeltaTime;
                rb.AddForce(force);

                // Debug: Kuvvet uygulandığını göster
                // Debug.DrawRay(other.transform.position, force.normalized * 0.5f, Color.blue);
            }

            // Sürekli hızlandırma (yenileme)
            if (applySpeedEffect && isPlayerInside)
            {
                TryRefreshSpeedBoost(other.gameObject);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player akıntıdan ÇIKTI!");
            currentPlayer = null;
            isPlayerInside = false;

            // Görseli eski haline döndür
            if (currentVisual != null)
            {
                currentVisual.color = originalColor;
            }

            // Hızlandırma coroutine'ini durdur
            if (speedCoroutine != null)
            {
                StopCoroutine(speedCoroutine);
                speedCoroutine = null;
            }

            // Debug: Hızlandırma bitti
            Debug.Log("Akıntı hızlandırması durdu.");
        }
    }

    // SÜREKLİ hızlandırma efekti
    void StartContinuousSpeedBoost(GameObject player)
    {
        if (speedCoroutine != null)
        {
            StopCoroutine(speedCoroutine);
        }

        speedCoroutine = StartCoroutine(ContinuousSpeedEffect(player));
    }

    IEnumerator ContinuousSpeedEffect(GameObject player)
    {
        Debug.Log("Sürekli hızlandırma başladı...");

        while (isPlayerInside && player != null)
        {
            ApplySpeedBoostToPlayer(player);
            lastSpeedTime = Time.time;

            // Belirli aralıklarla yenile
            yield return new WaitForSeconds(speedRefreshRate);
        }

        Debug.Log("Sürekli hızlandırma bitti.");
    }

    // Hızlandırmayı yenile
    void TryRefreshSpeedBoost(GameObject player)
    {
        if (Time.time - lastSpeedTime >= speedRefreshRate)
        {
            ApplySpeedBoostToPlayer(player);
            lastSpeedTime = Time.time;
            Debug.Log("Hızlandırma yenilendi.");
        }
    }

    // Player'a hızlandırma uygula
    void ApplySpeedBoostToPlayer(GameObject player)
    {
        PlayerSwimController swimController = player.GetComponent<PlayerSwimController>();
        if (swimController != null)
        {
            // PlayerSwimController'da bir ApplySpeedBoost metodu olmalı
            // Eğer yoksa, ApplySlow metodunu kullanarak tersini yapabiliriz:
            // 1.0/speedBoostMultiplier yerine doğrudan speedBoostMultiplier kullan
            swimController.ApplySpeedBoost(speedBoostMultiplier, speedDuration);
            Debug.Log($"Akıntı hızlandırması uygulandı: x{speedBoostMultiplier}");
        }
        else
        {
            Debug.LogWarning("PlayerSwimController bulunamadı!");
        }
    }

    // Debug için
    void Update()
    {
        if (isPlayerInside && currentPlayer != null)
        {
            // Debug çizgisi
            Debug.DrawLine(transform.position, currentPlayer.transform.position, Color.green); // DEĞİŞTİ: Color.cyan -> Color.green
        }
    }

    void OnDrawGizmosSelected()
    {
        // Akıntı alanı
        Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Gizmos.DrawCube(transform.position, new Vector3(collider.size.x, collider.size.y, 0.1f));
        }

        // Akıntı yönü
        Gizmos.color = Color.blue;
        Vector3 directionEnd = transform.position + (Vector3)currentForce.normalized * 2f;
        Gizmos.DrawLine(transform.position, directionEnd);
        Gizmos.DrawWireSphere(directionEnd, 0.2f);

        // Hızlandırma bölgesi
        if (applySpeedEffect)
        {
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.2f); // DEĞİŞTİ: Turuncu -> Yeşilimsi
            if (collider != null)
            {
                Gizmos.DrawWireCube(transform.position, new Vector3(collider.size.x, collider.size.y, 0.1f));
            }
        }
    }
}