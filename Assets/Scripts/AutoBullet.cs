using UnityEngine;

public class AutoBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float damage = 10f;
    public float bulletSpeed = 25f; // Daha hýzlý
    public float bulletLifetime = 2f;
    public GameObject hitEffect;

    [Header("Knockback Settings")]
    public float knockbackForce = 15f; // Daha güçlü

    [Header("Bullet Visual")]
    public Mesh bulletMesh;
    public Material bulletMaterial;
    public float bulletSize = 0.1f;

    private Vector3 direction;
    private Rigidbody rb;
    private bool hasHit = false;
    private GameObject bulletVisual;

    void Start()
    {
        CreateBulletVisual();

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }

        Destroy(gameObject, bulletLifetime);
    }

    void CreateBulletVisual()
    {
        if (transform.childCount == 0)
        {
            bulletVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulletVisual.transform.SetParent(transform);
            bulletVisual.transform.localPosition = Vector3.zero;
            bulletVisual.transform.localScale = Vector3.one * bulletSize;

            Renderer renderer = bulletVisual.GetComponent<Renderer>();
            if (bulletMaterial != null)
            {
                renderer.material = bulletMaterial;
            }
            else
            {
                renderer.material.color = Color.red;
            }

            Collider visualCollider = bulletVisual.GetComponent<Collider>();
            if (visualCollider != null) Destroy(visualCollider);
        }
    }

    public void Initialize(Vector3 dir)
    {
        direction = dir.normalized;
        transform.forward = direction;
    }

    void Update()
    {
        if (rb == null && !hasHit)
        {
            transform.position += direction * bulletSpeed * Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        if (!hasHit)
        {
            hasHit = true;

            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // Merminin yönünde GÜÇLÜ geri tepme uygula
                enemyHealth.TakeDamage(damage, direction);
            }
            else
            {
                enemyHealth = other.GetComponentInParent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage, direction);
                }
            }

            // Çarpma efekti
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + direction * 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.05f);
    }
}