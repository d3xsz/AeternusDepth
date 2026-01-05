using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSwimController : MonoBehaviour
{
    [Header("Yüzme Ayarları")]
    [SerializeField] private float swimSpeed = 8f;
    [SerializeField] private float swimAcceleration = 15f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float maxRotationAngle = 30f;

    [Header("Hız Boost Ayarları")]
    public float maxBoostMultiplier = 3f;
    private float currentBoostMultiplier = 1f;
    private Coroutine boostCoroutine;
    private bool isBoosted = false;

    [Header("Scale Ayarları")]
    [SerializeField] private float boostScaleMultiplier = 1.2f; // Yeşil pot için scale büyütme
    [SerializeField] private float poisonScaleMultiplier = 0.9f; // Kırmızı pot için scale küçültme
    [SerializeField] private float scaleChangeDuration = 0.3f; // Scale değişim süresi

    [Header("Referanslar")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Vector2 inputDirection;
    private Vector2 currentVelocity;
    private bool isSwimming = true;
    private float currentSwimSpeed = 0f;
    private float targetRotationZ = 0f;
    private float currentRotationZ = 0f;
    private float originalSwimSpeed;
    private Color originalSpriteColor;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Coroutine scaleCoroutine;

    // YENİ BASİT SİSTEM: Sadece 1 tane aktif poison olacak
    private float poisonMultiplier = 1f;
    private Coroutine activePoisonCoroutine;
    private Coroutine poisonVisualCoroutine; // <-- YENİ: Görsel efekti ayrı tut
    private bool isPoisoned = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalSwimSpeed = swimSpeed;
        originalScale = transform.localScale;
        targetScale = originalScale;

        if (spriteRenderer != null)
        {
            originalSpriteColor = spriteRenderer.color;
        }

        SetupRigidbody();
    }

    void Update()
    {
        HandleInput();
        HandleSpriteFlip();
        UpdateAnimations();
        HandleRotation();
    }

    void FixedUpdate()
    {
        if (!isSwimming) return;
        SwimMovement();
    }

    void SwimMovement()
    {
        float currentMaxSpeed = swimSpeed * currentBoostMultiplier * poisonMultiplier;

        Vector2 targetVelocity = inputDirection * currentMaxSpeed;
        currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, swimAcceleration * Time.fixedDeltaTime);
        rb.linearVelocity = currentVelocity;
        currentSwimSpeed = currentVelocity.magnitude / currentMaxSpeed;
    }

    // HIZ BOOST (YEŞİL POT)
    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if (boostCoroutine != null)
            StopCoroutine(boostCoroutine);

        boostCoroutine = StartCoroutine(SpeedBoostEffect(multiplier, duration));
    }

    private IEnumerator SpeedBoostEffect(float multiplier, float duration)
    {
        isBoosted = true;
        currentBoostMultiplier = Mathf.Min(multiplier, maxBoostMultiplier);
        Debug.Log($"🟢🟢🟢 HIZ BOOST BAŞLADI: {currentBoostMultiplier}x, Süre: {duration}s");

        // Scale büyütme efektini başlat
        ChangeScale(originalScale * boostScaleMultiplier);

        yield return new WaitForSeconds(duration);

        currentBoostMultiplier = 1f;
        isBoosted = false;
        Debug.Log($"🟢🟢🟢 HIZ BOOST BİTTİ ({duration}s sonra)");

        // Scale'i normale döndür
        ChangeScale(originalScale);
    }

    // POISON (KIRMIZI POT)
    public void ApplyPoison(float slowMultiplier, float duration)
    {
        Debug.Log($"🔴🔴🔴 POISON ALINDI: Çarpan={slowMultiplier}, Süre={duration}s");

        // Önceki poison'ı durdur
        if (activePoisonCoroutine != null)
        {
            StopCoroutine(activePoisonCoroutine);
        }

        // Görsel efekti de durdur
        if (poisonVisualCoroutine != null)
        {
            StopCoroutine(poisonVisualCoroutine);
            spriteRenderer.color = originalSpriteColor; // <-- RENGİ HEMEN SIFIRLA
        }

        // YENİ BASİT KURAL: ASLA 0.6'dan düşük olmasın!
        float safeMultiplier = Mathf.Max(0.6f, slowMultiplier);
        Debug.Log($"🎯 GÜVENLİ ÇARPAN: {safeMultiplier} (%{safeMultiplier * 100} hız)");

        // Poison efektini başlat
        activePoisonCoroutine = StartCoroutine(PoisonEffect(safeMultiplier, duration));
    }

    private IEnumerator PoisonEffect(float multiplier, float duration)
    {
        isPoisoned = true;
        poisonMultiplier = multiplier;
        Debug.Log($"🔴 POISON AKTİF: Çarpan={poisonMultiplier}, {duration}s sürecek");

        // Scale küçültme efekti
        ChangeScale(originalScale * poisonScaleMultiplier);

        // Görsel efekti başlat
        poisonVisualCoroutine = StartCoroutine(PoisonVisualEffect(duration));

        yield return new WaitForSeconds(duration);

        // Poison bitince
        poisonMultiplier = 1f;
        isPoisoned = false;
        Debug.Log($"✅ POISON BİTTİ ({duration}s sonra) - Normal hıza dönüldü");

        // Görsel efekti temizle
        if (poisonVisualCoroutine != null)
        {
            StopCoroutine(poisonVisualCoroutine);
        }
        spriteRenderer.color = originalSpriteColor;

        // Scale'i normale döndür (hala hız boost varsa ona göre ayarla)
        if (isBoosted)
        {
            ChangeScale(originalScale * boostScaleMultiplier);
        }
        else
        {
            ChangeScale(originalScale);
        }
    }

    // DÜZELTİLMİŞ: TİTREMEYEN Poison görsel efekti
    private IEnumerator PoisonVisualEffect(float duration)
    {
        if (spriteRenderer == null) yield break;

        Color poisonColor = new Color(1f, 0.3f, 0.3f, 1f);
        float timer = 0f;
        float pulseSpeed = 3f; // Renk pulsasyon hızı

        while (timer < duration && isPoisoned)
        {
            timer += Time.deltaTime;

            // Renk pulsasyonu
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            Color currentColor = Color.Lerp(poisonColor, new Color(1f, 0.5f, 0.5f, 1f), pulse);
            spriteRenderer.color = currentColor;

            yield return null;
        }

        // Süre bittiğinde orijinal renge dön
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalSpriteColor;
        }
    }

    // YAVAŞLATMA (AnchorTrap) - Basit versiyon
    public void ApplySlow(float slowMultiplier, float duration)
    {
        StartCoroutine(SlowEffect(slowMultiplier, duration));
    }

    private IEnumerator SlowEffect(float multiplier, float duration)
    {
        float originalPoison = poisonMultiplier;
        poisonMultiplier = Mathf.Min(poisonMultiplier, multiplier);
        Debug.Log($"🔵 SLOW: {multiplier}x, {duration}s");

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.5f, 0.5f, 1f, 1f);
        }

        yield return new WaitForSeconds(duration);

        poisonMultiplier = originalPoison;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalSpriteColor;
        }
        Debug.Log($"✅ SLOW BİTTİ ({duration}s sonra)");
    }

    // Scale değişim fonksiyonu (yumuşak geçiş)
    private void ChangeScale(Vector3 newScale)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleChangeCoroutine(newScale));
    }

    private IEnumerator ScaleChangeCoroutine(Vector3 targetScale)
    {
        float timer = 0f;
        Vector3 startScale = transform.localScale;
        this.targetScale = targetScale;

        while (timer < scaleChangeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / scaleChangeDuration;
            t = Mathf.SmoothStep(0f, 1f, t); // Yumuşak geçiş

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    public void ResetAllEffects()
    {
        Debug.Log("🔄 TÜM EFEKTLER SIFIRLANDI");

        if (boostCoroutine != null) StopCoroutine(boostCoroutine);
        if (activePoisonCoroutine != null) StopCoroutine(activePoisonCoroutine);
        if (poisonVisualCoroutine != null) StopCoroutine(poisonVisualCoroutine);
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);

        currentBoostMultiplier = 1f;
        poisonMultiplier = 1f;
        swimSpeed = originalSwimSpeed;
        isPoisoned = false;

        // Scale'i hemen normale döndür
        transform.localScale = originalScale;
        targetScale = originalScale;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalSpriteColor; // <-- BU ÇOK ÖNEMLİ!
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // YENİ: Hız kontrolü için public property
    public float GetCurrentSpeedMultiplier()
    {
        return currentBoostMultiplier * poisonMultiplier;
    }

    // Kalan metodlar aynı...
    void HandleInput()
    {
        inputDirection = Vector2.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputDirection.y = 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputDirection.y = -1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputDirection.x = -1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputDirection.x = 1;
        if (inputDirection.magnitude > 1) inputDirection.Normalize();
        UpdateTargetRotation();
    }

    void UpdateTargetRotation()
    {
        if (inputDirection.x > 0.1f) targetRotationZ = -maxRotationAngle;
        else if (inputDirection.x < -0.1f) targetRotationZ = maxRotationAngle;
        else targetRotationZ = 0f;

        if (inputDirection.y > 0.1f) targetRotationZ *= 0.7f;
        else if (inputDirection.y < -0.1f) targetRotationZ *= 0.7f;
    }

    void HandleRotation()
    {
        currentRotationZ = Mathf.Lerp(currentRotationZ, targetRotationZ, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, currentRotationZ);
    }

    void HandleSpriteFlip()
    {
        if (inputDirection.x > 0) spriteRenderer.flipX = false;
        else if (inputDirection.x < 0) spriteRenderer.flipX = true;
    }

    void UpdateAnimations()
    {
        if (animator == null) return;
        animator.SetBool("IsSwimming", isSwimming);
        animator.SetFloat("SwimSpeed", currentSwimSpeed);
        animator.SetFloat("Horizontal", inputDirection.x);
        animator.SetFloat("Vertical", inputDirection.y);
    }

    void SetupRigidbody()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public bool IsMoving() => inputDirection.magnitude > 0.1f;
    public Vector2 GetInputDirection() => inputDirection;
    public void StartSwimming() { isSwimming = true; if (animator != null) animator.SetBool("IsSwimming", true); }
    public void StopSwimming() { isSwimming = false; rb.linearVelocity = Vector2.zero; if (animator != null) animator.SetBool("IsSwimming", false); }
    public bool IsSwimming() => isSwimming;
    public void ApplyExternalForce(Vector2 force) { if (rb != null && isSwimming) rb.AddForce(force, ForceMode2D.Force); }
}