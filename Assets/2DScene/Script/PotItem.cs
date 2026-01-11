using UnityEngine;
using System.Collections;

public class PotItem : MonoBehaviour
{
    [Header("Pot Ayarları")]
    public bool isPoisonPot = false;
    public float effectMultiplier = 1.5f;

    [Header("Pot Süreleri")]
    [SerializeField] private float speedPotDuration = 4f;
    [SerializeField] private float poisonPotDuration = 2f;

    [Header("Görsel Ayarları")]
    public SpriteRenderer spriteRenderer;
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;

    [Header("Rotation Ayarları")]
    public float rotationAngle = 15f; // Sağa/sola dönüş açısı
    public float rotationSpeed = 5f; // Rotation hızı
    public bool smoothRotation = true; // Yumuşak geçiş

    [Header("Scale Animasyon Ayarları")]
    public float scaleAnimationSpeed = 2f; // Scale animasyon hızı
    public float minScaleMultiplier = 0.8f; // Minimum scale çarpanı (kendi scale'inin yüzdesi)
    public float maxScaleMultiplier = 1.2f; // Maximum scale çarpanı (kendi scale'inin yüzdesi)

    [Header("Efektler")]
    public GameObject collectEffectPrefab;

    private Vector3 startPosition;
    private bool isCollected = false;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private float rotationTimer = 0f;
    private bool isRotatingRight = true;
    private Quaternion targetRotation;
    private Quaternion leftRotation;
    private Quaternion rightRotation;
    private float scaleAnimationTimer = 0f;

    void Start()
    {
        startPosition = transform.position;
        originalScale = transform.localScale;
        originalRotation = transform.rotation;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Rotation değerlerini hesapla
        leftRotation = Quaternion.Euler(0, 0, rotationAngle);
        rightRotation = Quaternion.Euler(0, 0, -rotationAngle);

        // Başlangıç rotation'ı
        targetRotation = isRotatingRight ? rightRotation : leftRotation;
        if (!smoothRotation)
        {
            transform.rotation = targetRotation;
        }

        SetupPotType();
        gameObject.tag = "Pot";
    }

    void SetupPotType()
    {
        if (isPoisonPot)
        {
            effectMultiplier = 0.8f; // %80 hız (%20 yavaşlama)
            Debug.Log($"🐌 YAVAŞLATAN DENİZ ANASI: %80 hız ({effectMultiplier}x), Süre: {poisonPotDuration}s");
        }
        else
        {
            effectMultiplier = 1.5f; // %150 hız
            Debug.Log($"⚡ HIZLANDIRAN DENİZ ANASI: %150 hız ({effectMultiplier}x), Süre: {speedPotDuration}s");
        }
    }

    void Update()
    {
        if (isCollected) return;

        // Rotation yön değiştirme kontrolü
        rotationTimer += Time.deltaTime;
        if (rotationTimer >= 3f) // Her 3 saniyede bir yön değiştir
        {
            ChangeRotationDirection();
            rotationTimer = 0f;
        }

        // Yukarı-aşağı sallanma efekti
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, startPosition.y + bob, startPosition.z);

        // Yumuşak rotation
        if (smoothRotation)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Scale animasyonu
        UpdateScaleAnimation();
    }

    void ChangeRotationDirection()
    {
        isRotatingRight = !isRotatingRight;

        // Yöne göre target rotation'ı güncelle
        targetRotation = isRotatingRight ? rightRotation : leftRotation;

        // Yumuşak rotation kapalıysa anında uygula
        if (!smoothRotation)
        {
            transform.rotation = targetRotation;
        }

        // Scale animasyon timer'ını sıfırla
        scaleAnimationTimer = 0f;
    }

    void UpdateScaleAnimation()
    {
        if (isCollected) return;

        // Scale animasyon timer'ını güncelle
        scaleAnimationTimer += Time.deltaTime * scaleAnimationSpeed;

        // Sinüs dalgası kullanarak scale değerini hesapla (0-1 arasında)
        float scaleFactor = (Mathf.Sin(scaleAnimationTimer) + 1f) * 0.5f; // 0-1 arasına normalize et

        // İki scale değeri arasında interpolasyon yap
        float currentScaleMultiplier = Mathf.Lerp(minScaleMultiplier, maxScaleMultiplier, scaleFactor);

        // Yeni scale'i uygula
        transform.localScale = originalScale * currentScaleMultiplier;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            CollectPot(other.gameObject);
        }
    }

    void CollectPot(GameObject player)
    {
        isCollected = true;

        PlayerSwimController swimController = player.GetComponent<PlayerSwimController>();
        if (swimController != null)
        {
            float duration = isPoisonPot ? poisonPotDuration : speedPotDuration;

            if (isPoisonPot)
            {
                Debug.Log($"🐌🐌🐌 YAVAŞLATAN DENİZ ANASI YENDİ: Çarpan = {effectMultiplier}, Süre = {duration}s");
                swimController.ApplyPoison(effectMultiplier, duration);
            }
            else
            {
                Debug.Log($"⚡⚡⚡ HIZLANDIRAN DENİZ ANASI YENDİ: Çarpan = {effectMultiplier}, Süre = {duration}s");
                swimController.ApplySpeedBoost(effectMultiplier, duration);
            }
        }

        StartCoroutine(CollectAnimation());

        if (PotManager.Instance != null)
        {
            PotManager.Instance.OnPotCollected(gameObject);
        }
    }

    IEnumerator CollectAnimation()
    {
        float duration = 0.5f;
        float elapsedTime = 0f;

        if (collectEffectPrefab != null)
        {
            GameObject effect = Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
        }

        // Yok olma animasyonu
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Küçülme efekti
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, t);

            // Ekstra dönme efekti
            transform.Rotate(0, 0, 720f * Time.deltaTime);

            if (spriteRenderer != null)
            {
                Color newColor = spriteRenderer.color;
                newColor.a = Mathf.Lerp(1f, 0f, t);
                spriteRenderer.color = newColor;
            }

            yield return null;
        }

        gameObject.SetActive(false);
    }

    public void ResetPot()
    {
        isCollected = false;

        transform.localScale = originalScale;
        transform.position = startPosition;
        transform.rotation = originalRotation;

        // Varsayılan rotation yönü
        isRotatingRight = true;
        targetRotation = rightRotation;
        rotationTimer = 0f;
        scaleAnimationTimer = 0f;

        if (!smoothRotation)
        {
            transform.rotation = targetRotation;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    public void ChangePotType(bool makeItPoison)
    {
        isPoisonPot = makeItPoison;
        SetupPotType();
    }

    public void SetPotDurations(float speedDuration, float poisonDuration)
    {
        speedPotDuration = speedDuration;
        poisonPotDuration = poisonDuration;
        Debug.Log($"⏱️ Deniz anası süreleri ayarlandı: Hız={speedDuration}s, Yavaş={poisonDuration}s");
    }

    // Rotation ayarlarını değiştirmek için
    public void SetRotationSettings(float angle, float speed, bool smooth)
    {
        rotationAngle = angle;
        rotationSpeed = speed;
        smoothRotation = smooth;

        // Yeni rotation değerlerini hesapla
        leftRotation = Quaternion.Euler(0, 0, rotationAngle);
        rightRotation = Quaternion.Euler(0, 0, -rotationAngle);

        // Güncel target rotation'ı güncelle
        targetRotation = isRotatingRight ? rightRotation : leftRotation;
    }

    // Scale animasyon ayarlarını değiştirmek için
    public void SetScaleAnimationSettings(float speed, float minScale, float maxScale)
    {
        scaleAnimationSpeed = speed;
        minScaleMultiplier = minScale;
        maxScaleMultiplier = maxScale;

        // Orijinal scale'i kaydet
        originalScale = transform.localScale;
    }

    // Bob hareketi ayarlarını değiştirmek için
    public void SetBobSettings(float speed, float height)
    {
        bobSpeed = speed;
        bobHeight = height;
    }

    // Pot tipini öğrenmek için public getter
    public bool IsPoisonPot()
    {
        return isPoisonPot;
    }
}