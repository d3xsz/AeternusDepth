using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isInvincible = false;
    public float invincibilityTime = 1f;

    [Header("3D Damage Flash Effect")]
    public SkinnedMeshRenderer skinnedRenderer;
    public Color flashColor = new Color(2f, 0.5f, 0.5f, 1f); // PARLAK kırmızı
    public float flashIntensity = 3f; // Çok daha yüksek
    public float flashDuration = 0.15f;
    public int flashCount = 4;

    [Header("Shader Properties")]
    public string emissionProperty = "_EmissionColor";
    public string colorProperty = "_Color";

    [Header("Alternative Effects")]
    public ParticleSystem damageParticles;
    public GameObject damageVFX;
    public AudioClip damageSound;

    [Header("Events")]
    public UnityEvent OnDamageTaken;
    public UnityEvent OnDeath;
    public UnityEvent<int> OnHealthChanged;

    [Header("Debug")]
    public bool showDebug = true;

    private float lastDamageTime;
    private Material[] originalMaterials;
    private Color[] originalEmissionColors;
    private bool isFlashing = false;

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);

        // Renderer'ı bul
        FindRenderer();

        // Orijinal materyalleri sakla
        if (skinnedRenderer != null)
        {
            originalMaterials = skinnedRenderer.materials;
            originalEmissionColors = new Color[originalMaterials.Length];

            for (int i = 0; i < originalMaterials.Length; i++)
            {
                if (originalMaterials[i].HasProperty(emissionProperty))
                {
                    originalEmissionColors[i] = originalMaterials[i].GetColor(emissionProperty);
                }
            }

            Debug.Log($"PlayerHealth: {originalMaterials.Length} materyal bulundu");
        }

        ApplyHealthBonuses();
    }

    void FindRenderer()
    {
        // Önce SkinnedMeshRenderer'ı bul
        if (skinnedRenderer == null)
            skinnedRenderer = GetComponent<SkinnedMeshRenderer>();

        if (skinnedRenderer == null)
            skinnedRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        if (skinnedRenderer == null)
        {
            Debug.LogError("PlayerHealth: SkinnedMeshRenderer bulunamadı! Flash çalışmayacak.");
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isInvincible || currentHealth <= 0)
            return;

        if (Time.time - lastDamageTime < invincibilityTime)
            return;

        currentHealth -= damageAmount;
        lastDamageTime = Time.time;

        OnHealthChanged?.Invoke(currentHealth);
        OnDamageTaken?.Invoke();

        // FLASH EFEKTİ - 3D İÇİN
        Start3DFlashEffect();

        // Ek efektler
        PlayDamageEffects();

        if (showDebug) Debug.Log($"PlayerHealth: {damageAmount} hasar aldı! Kalan can: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    void Start3DFlashEffect()
    {
        if (isFlashing) return;

        StartCoroutine(Flash3DCoroutine());
    }

    IEnumerator Flash3DCoroutine()
    {
        isFlashing = true;

        if (skinnedRenderer == null || originalMaterials == null)
        {
            isFlashing = false;
            yield break;
        }

        // YENİ MATERYALLER OLUŞTUR (clone)
        Material[] flashMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            flashMaterials[i] = new Material(originalMaterials[i]);
        }

        for (int flashIndex = 0; flashIndex < flashCount; flashIndex++)
        {
            // PARLAT
            for (int i = 0; i < flashMaterials.Length; i++)
            {
                // Emission property'si varsa kullan
                if (flashMaterials[i].HasProperty(emissionProperty))
                {
                    flashMaterials[i].EnableKeyword("_EMISSION");
                    flashMaterials[i].SetColor(emissionProperty, flashColor * flashIntensity);
                }
                // Yoksa base color'ı değiştir
                else if (flashMaterials[i].HasProperty(colorProperty))
                {
                    flashMaterials[i].SetColor(colorProperty, flashColor);
                }
            }

            // Material'ları uygula
            skinnedRenderer.materials = flashMaterials;

            // Global Illumination'ı güncelle (build için önemli)
            DynamicGI.UpdateEnvironment();

            yield return new WaitForSeconds(flashDuration);

            // NORMALE DÖN
            skinnedRenderer.materials = originalMaterials;

            // Kısa bekleme
            if (flashIndex < flashCount - 1)
                yield return new WaitForSeconds(flashDuration * 0.3f);
        }

        // Temizlik
        foreach (var mat in flashMaterials)
        {
            if (Application.isPlaying)
                Destroy(mat);
            else
                DestroyImmediate(mat);
        }

        isFlashing = false;
    }

    // DAHA BASİT VE ETKİLİ YÖNTEM - Tüm modeli beyaz yap
    IEnumerator SimpleFlashCoroutine()
    {
        isFlashing = true;

        // 1. TÜM MODELİ BEYAZ YAP
        Material whiteMaterial = new Material(Shader.Find("Standard"));
        whiteMaterial.color = Color.white;
        whiteMaterial.SetColor("_EmissionColor", Color.white * 5f);
        whiteMaterial.EnableKeyword("_EMISSION");

        Material[] whiteMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < whiteMaterials.Length; i++)
        {
            whiteMaterials[i] = whiteMaterial;
        }

        for (int i = 0; i < flashCount; i++)
        {
            // BEYAZ YAP
            skinnedRenderer.materials = whiteMaterials;
            yield return new WaitForSeconds(flashDuration);

            // NORMALE DÖN
            skinnedRenderer.materials = originalMaterials;

            if (i < flashCount - 1)
                yield return new WaitForSeconds(flashDuration * 0.3f);
        }

        if (Application.isPlaying)
            Destroy(whiteMaterial);
        else
            DestroyImmediate(whiteMaterial);

        isFlashing = false;
    }

    void PlayDamageEffects()
    {
        // Particle efekti
        if (damageParticles != null)
        {
            damageParticles.Play();
        }

        // VFX prefab'ı
        if (damageVFX != null)
        {
            Instantiate(damageVFX, transform.position, Quaternion.identity);
        }

        // Ses efekti
        if (damageSound != null)
        {
            AudioSource.PlayClipAtPoint(damageSound, transform.position);
        }
    }

    // Diğer metodlar aynı kalacak...
    public void Heal(int healAmount)
    {
        int actualMaxHealth = maxHealth;
        PlayerStats playerStats = PlayerStats.Instance;
        if (playerStats != null) actualMaxHealth += playerStats.maxHealthBonus;

        currentHealth = Mathf.Min(currentHealth + healAmount, actualMaxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void RestoreFullHealth()
    {
        int actualMaxHealth = maxHealth;
        PlayerStats playerStats = PlayerStats.Instance;
        if (playerStats != null) actualMaxHealth += playerStats.maxHealthBonus;

        currentHealth = actualMaxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void AddMaxHealth(int bonusHealth)
    {
        int oldMaxHealth = maxHealth;
        maxHealth += bonusHealth;
        currentHealth += bonusHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void ApplyHealthBonuses()
    {
        PlayerStats playerStats = PlayerStats.Instance;
        if (playerStats != null && playerStats.maxHealthBonus > 0)
        {
            AddMaxHealth(playerStats.maxHealthBonus);
        }
    }

    private void Die()
    {
        if (showDebug) Debug.Log("PlayerHealth: Player öldü!");
        OnDeath?.Invoke();

        // ÖLÜM EFEKTİ - Kırmızı ton
        if (skinnedRenderer != null && originalMaterials != null)
        {
            foreach (var mat in originalMaterials)
            {
                if (mat.HasProperty(colorProperty))
                    mat.SetColor(colorProperty, new Color(0.5f, 0f, 0f, 1f));
            }
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        // Invincibility sırasında yanıp sönme
        StartCoroutine(InvincibilityFlashCoroutine());

        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
    }

    IEnumerator InvincibilityFlashCoroutine()
    {
        float endTime = Time.time + invincibilityTime;

        while (Time.time < endTime && isInvincible)
        {
            if (skinnedRenderer != null && originalMaterials != null)
            {
                // Yarı saydam yap
                foreach (var mat in originalMaterials)
                {
                    Color color = mat.color;
                    color.a = 0.3f;
                    mat.color = color;
                }

                yield return new WaitForSeconds(0.1f);

                // Normal yap
                foreach (var mat in originalMaterials)
                {
                    Color color = mat.color;
                    color.a = 1f;
                    mat.color = color;
                }

                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                yield return null;
            }
        }
    }

    // TEST İÇİN
    [ContextMenu("Test Flash Effect")]
    public void TestFlashEffect()
    {
        StartCoroutine(SimpleFlashCoroutine());
    }

    [ContextMenu("Test Damage")]
    public void TestDamage()
    {
        TakeDamage(10);
    }
}