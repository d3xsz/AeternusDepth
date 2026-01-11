using UnityEngine;
using System.Collections;

public class TentacleTrap : MonoBehaviour
{
    [Header("Sallanma Ayarları")]
    public float swingSpeed = 0.7f;
    public float swingAmount = 40f;

    [Header("Tuzak Ayarları")]
    public float slowAmount = 0.5f;
    public float slowDuration = 2f;
    public float activationCooldown = 3f; // YENİ: Cooldown süresi

    [Header("Görsel")]
    public SpriteRenderer tentacleSprite;

    [Header("Yön Ayarları")]
    public bool flipX = false;
    public bool flipY = false;

    [Header("Efektler")]
    public ParticleSystem grabParticles;
    public AudioClip grabSound;

    private float timer = 0f;
    private float directionMultiplier = 1f;
    private bool isActive = true;
    private Color originalColor;

    void Start()
    {
        // Sprite flip ayarlarını uygula
        if (tentacleSprite != null)
        {
            tentacleSprite.flipX = flipX;
            tentacleSprite.flipY = flipY;
            originalColor = tentacleSprite.color;
        }

        // Flip durumuna göre yön çarpanını ayarla
        directionMultiplier = flipX ? -1f : 1f;

        Debug.Log($"Tentacle başlatıldı. FlipX: {flipX}, Direction: {directionMultiplier}");
    }

    void Update()
    {
        if (!isActive) return; // Cooldown'da sallanma durur

        timer += Time.deltaTime * swingSpeed;

        // Yavaş sallanma
        float swing = Mathf.Sin(timer) * swingAmount * directionMultiplier;

        // Rotasyonu uygula
        transform.rotation = Quaternion.Euler(0, 0, swing);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return; // Cooldown'da tetiklenmez

        if (other.CompareTag("Player"))
        {
            Debug.Log("🦑 Player tentacle'a çarptı!");

            // Yavaşlatma efektini uygula
            ApplySlowEffect(other.gameObject);

            // Görsel ve ses efektleri
            StartCoroutine(GrabEffect());

            // Cooldown başlat
            StartCoroutine(CooldownRoutine());
        }
    }

    void ApplySlowEffect(GameObject playerObject)
    {
        // Direkt PlayerSwimController'a eriş
        PlayerSwimController swimController = playerObject.GetComponent<PlayerSwimController>();
        if (swimController != null)
        {
            swimController.ApplySlow(slowAmount, slowDuration);
            Debug.Log($"🦑 Tentacle: Player %{(1 - slowAmount) * 100} yavaşladı, {slowDuration}s süreyle");
        }
        else
        {
            Debug.LogWarning("⚠️ TentacleTrap: PlayerSwimController bulunamadı!");
        }
    }

    IEnumerator GrabEffect()
    {
        // Kırmızı parlama efekti
        if (tentacleSprite != null)
        {
            tentacleSprite.color = Color.red;
        }

        // Partikül efekti
        if (grabParticles != null)
        {
            grabParticles.Play();
        }

        // Ses efekti
        if (grabSound != null)
        {
            AudioSource.PlayClipAtPoint(grabSound, transform.position);
        }

        yield return new WaitForSeconds(0.2f);

        // Rengi normale döndür
        if (tentacleSprite != null)
        {
            tentacleSprite.color = originalColor;
        }
    }

    IEnumerator CooldownRoutine()
    {
        isActive = false;

        // Görsel olarak devre dışı göster (gri renk)
        if (tentacleSprite != null)
        {
            tentacleSprite.color = Color.gray;
        }

        Debug.Log($"⏳ Tentacle devre dışı, {activationCooldown}s sonra aktif");

        yield return new WaitForSeconds(activationCooldown);

        isActive = true;

        // Görsel olarak aktif göster
        if (tentacleSprite != null)
        {
            tentacleSprite.color = originalColor;
        }

        Debug.Log("✅ Tentacle yeniden aktif");
    }

    // Gizmos ile görselleştirme
    void OnDrawGizmosSelected()
    {
        // Pivot noktası
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.15f);

        // Sallanma yönü
        Vector3 direction = (flipX ? Vector3.left : Vector3.right);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, direction * 2f);

        // Trigger alanı (Collider'ın boyutu)
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);

            if (collider is BoxCollider2D)
            {
                BoxCollider2D boxCollider = (BoxCollider2D)collider;
                Gizmos.DrawCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
            }
            else if (collider is CircleCollider2D)
            {
                CircleCollider2D circleCollider = (CircleCollider2D)collider;
                Gizmos.DrawSphere(transform.position + (Vector3)circleCollider.offset, circleCollider.radius);
            }
        }
    }

    // EDITOR İÇİN: Inspector'da değişiklik olduğunda
    void OnValidate()
    {
        if (tentacleSprite != null)
        {
            tentacleSprite.flipX = flipX;
            tentacleSprite.flipY = flipY;
        }
    }

    // MANUEL AKTİVASYON (test için)
    public void TestTrigger()
    {
        if (isActive)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                ApplySlowEffect(player);
                StartCoroutine(GrabEffect());
                StartCoroutine(CooldownRoutine());
            }
        }
    }

    // RESET (respawn sırasında)
    public void ResetTrap()
    {
        StopAllCoroutines();
        isActive = true;

        if (tentacleSprite != null)
        {
            tentacleSprite.color = originalColor;
        }

        transform.rotation = Quaternion.identity;
        timer = 0f;
    }
}