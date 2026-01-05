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

    [Header("Hareket Ayarları")]
    public float horizontalMoveSpeed = 1f;
    public float moveDistance = 2f;
    public float directionChangeInterval = 3f;

    [Header("Rotation Ayarları")]
    public float rotationAngle = 15f; // Sağa/sola dönüş açısı
    public float rotationSpeed = 5f; // Rotation hızı
    public bool smoothRotation = true; // Yumuşak geçiş

    [Header("Efektler")]
    public GameObject collectEffectPrefab;

    [Header("Deniz Anası Animasyonları")]
    public Sprite[] idleFramesLeft;
    public Sprite[] idleFramesRight;
    public float frameDuration = 0.2f;

    private Vector3 startPosition;
    private bool isCollected = false;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private int currentFrame = 0;
    private float frameTimer = 0f;
    private float directionTimer = 0f;
    private bool isMovingRight = true;
    private Vector3 movementStartPos;
    private Quaternion targetRotation;
    private Quaternion leftRotation;
    private Quaternion rightRotation;

    void Start()
    {
        startPosition = transform.position;
        movementStartPos = startPosition;
        originalScale = transform.localScale;
        originalRotation = transform.rotation;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Rotation değerlerini hesapla
        leftRotation = Quaternion.Euler(0, 0, rotationAngle);
        rightRotation = Quaternion.Euler(0, 0, -rotationAngle);

        // Başlangıç rotation'ı
        targetRotation = isMovingRight ? rightRotation : leftRotation;
        if (!smoothRotation)
        {
            transform.rotation = targetRotation;
        }

        SetupPotType();
        gameObject.tag = "Pot";

        // Başlangıç animasyonu
        UpdateSprite();
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

        // Yön değiştirme kontrolü
        directionTimer += Time.deltaTime;
        if (directionTimer >= directionChangeInterval)
        {
            ChangeDirection();
            directionTimer = 0f;
        }

        // Yatay hareket
        float horizontalMovement = isMovingRight ? horizontalMoveSpeed : -horizontalMoveSpeed;
        transform.position += new Vector3(horizontalMovement * Time.deltaTime, 0, 0);

        // Hareket sınırı kontrolü
        float currentDistance = Mathf.Abs(transform.position.x - movementStartPos.x);
        if (currentDistance >= moveDistance)
        {
            ChangeDirection();
            movementStartPos = transform.position;
        }

        // Yukarı-aşağı sallanma efekti
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, startPosition.y + bob, transform.position.z);

        // Yumuşak rotation
        if (smoothRotation)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Animasyon güncelleme
        UpdateAnimation();
    }

    void ChangeDirection()
    {
        isMovingRight = !isMovingRight;

        // Yöne göre target rotation'ı güncelle
        targetRotation = isMovingRight ? rightRotation : leftRotation;

        // Yumuşak rotation kapalıysa anında uygula
        if (!smoothRotation)
        {
            transform.rotation = targetRotation;
        }

        // Frame sıfırlama
        currentFrame = 0;
        frameTimer = 0f;

        // Sprite'ı güncelle
        UpdateSprite();
    }

    void UpdateSprite()
    {
        if (spriteRenderer == null) return;

        // Yöne göre doğru frame array'ini seç
        Sprite[] currentFrames = isMovingRight ? idleFramesRight : idleFramesLeft;

        if (currentFrames != null && currentFrames.Length > 0)
        {
            spriteRenderer.sprite = currentFrames[0];
            spriteRenderer.flipX = false; // Rotation ile yön değiştiği için flip gerekmez
        }
    }

    void UpdateAnimation()
    {
        if (isCollected) return;

        // Yöne göre doğru frame array'ini seç
        Sprite[] currentFrames = isMovingRight ? idleFramesRight : idleFramesLeft;

        if (currentFrames == null || currentFrames.Length == 0) return;

        // Frame güncelleme
        frameTimer += Time.deltaTime;

        if (frameTimer >= frameDuration)
        {
            frameTimer = 0f;

            // Sonraki frame'e geç
            currentFrame = (currentFrame + 1) % currentFrames.Length;

            // Sprite'ı güncelle (flip yapmıyoruz çünkü rotation ile yön değiştiriyoruz)
            spriteRenderer.sprite = currentFrames[currentFrame];
        }
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
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);

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
        movementStartPos = startPosition;

        // Varsayılan yön
        isMovingRight = true;
        targetRotation = rightRotation;
        directionTimer = 0f;
        currentFrame = 0;
        frameTimer = 0f;

        if (!smoothRotation)
        {
            transform.rotation = targetRotation;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            spriteRenderer.flipX = false;
            UpdateSprite();
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

    public void SetMovementSettings(float moveSpeed, float distance, float changeInterval)
    {
        horizontalMoveSpeed = moveSpeed;
        moveDistance = distance;
        directionChangeInterval = changeInterval;
    }

    public void SetAnimationFrames(Sprite[] framesLeft, Sprite[] framesRight)
    {
        idleFramesLeft = framesLeft;
        idleFramesRight = framesRight;
        UpdateSprite();
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
        targetRotation = isMovingRight ? rightRotation : leftRotation;
    }

    // Pot tipini öğrenmek için public getter
    public bool IsPoisonPot()
    {
        return isPoisonPot;
    }
}