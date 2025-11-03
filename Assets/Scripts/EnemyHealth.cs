using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 30f;
    public GameObject deathEffect;

    [Header("Knockback Settings")]
    public float knockbackForce = 15f; // Daha güçlü geri tepme
    public float knockbackDuration = 0.3f;

    [Header("Debug")]
    public bool showDebug = true;

    private float currentHealth;
    private bool isDead = false;
    private Rigidbody rb;
    private Collider enemyCollider;
    private Vector3 knockbackDirection;
    private float knockbackTimer = 0f;
    private bool isKnockback = false;
    private Vector3 lastHitDirection;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        enemyCollider = GetComponent<Collider>();

        if (!gameObject.CompareTag("Enemy"))
        {
            gameObject.tag = "Enemy";
        }

        // Rigidbody ayarlarını kontrol et
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    void Update()
    {
        // Geri tepme süresini kontrol et
        if (isKnockback)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockback = false;
            }
        }
    }

    void FixedUpdate()
    {
        // Geri tepme sırasında sürekli itme kuvveti uygula
        if (isKnockback && rb != null)
        {
            rb.AddForce(lastHitDirection * knockbackForce * 0.5f, ForceMode.Force);
        }
    }

    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (showDebug)
        {
            Debug.Log(gameObject.name + " " + damage + " hasar aldı! Kalan can: " + currentHealth);
        }

        // HER ZAMAN geri tepme uygula (ölse bile)
        ApplyKnockback(hitDirection);

        if (currentHealth <= 0)
        {
            Die(); // ANINDA ÖL
        }
    }

    void ApplyKnockback(Vector3 direction)
    {
        if (rb == null) return;

        lastHitDirection = direction.normalized;
        isKnockback = true;
        knockbackTimer = knockbackDuration;

        // Önceki kuvvetleri temizle
        rb.linearVelocity = Vector3.zero;

        // Güçlü ve anlık itme kuvveti
        rb.AddForce(lastHitDirection * knockbackForce, ForceMode.Impulse);

        if (showDebug) Debug.Log("🔴 GERİ TEPME: " + lastHitDirection * knockbackForce);
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (showDebug) Debug.Log(gameObject.name + " ANINDA ÖLDÜ!");

        // Ölüm efekti (ANINDA)
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Tüm script'leri devre dışı bırak
        DisableAllScripts();

        // Collider'ı devre dışı bırak
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        // ANINDA YOK ET - DELAY YOK!
        Destroy(gameObject);
    }

    void DisableAllScripts()
    {
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this && script.enabled)
            {
                script.enabled = false;
            }
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsInKnockback()
    {
        return isKnockback;
    }
}