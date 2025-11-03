using UnityEngine;
using System.Collections.Generic;

public class AutoBulletShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float bulletSpeed = 25f; // Daha hızlı
    public float bulletLifetime = 2f;

    [Header("Knockback Settings")]
    public float bulletKnockback = 15f; // Daha güçlü

    [Header("Target Detection")]
    public float detectionRange = 10f;
    public float detectionAngle = 60f;
    public LayerMask enemyLayer = 1;
    public LayerMask obstacleLayer = 1;

    [Header("Debug")]
    public bool showDebug = true;

    private float nextFireTime = 0f;
    private List<Transform> enemiesInSight = new List<Transform>();
    private Weapon weaponScript;

    void Start()
    {
        weaponScript = GetComponent<Weapon>();

        if (firePoint == null)
        {
            CreateFirePoint();
        }

        if (bulletPrefab == null)
        {
            CreateDefaultBulletPrefab();
        }
    }

    void Update()
    {
        if (!enabled) return;

        if (weaponScript == null || !weaponScript.isEquipped)
        {
            return;
        }

        ScanForEnemies();

        if (Time.time >= nextFireTime && enemiesInSight.Count > 0)
        {
            Transform closestEnemy = GetClosestEnemy();
            if (closestEnemy != null)
            {
                ShootAtTarget(closestEnemy);
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
    }

    void ScanForEnemies()
    {
        enemiesInSight.Clear();

        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);

        foreach (Collider enemy in enemies)
        {
            if (enemy.CompareTag("Enemy") && IsEnemyInSight(enemy.transform))
            {
                enemiesInSight.Add(enemy.transform);
            }
        }
    }

    bool IsEnemyInSight(Transform enemy)
    {
        if (!enemy.CompareTag("Enemy")) return false;

        Vector3 directionToEnemy = (enemy.position - transform.position).normalized;
        float angleToEnemy = Vector3.Angle(transform.forward, directionToEnemy);

        if (angleToEnemy > detectionAngle / 2f) return false;

        float distanceToEnemy = Vector3.Distance(transform.position, enemy.position);
        if (Physics.Raycast(transform.position, directionToEnemy, distanceToEnemy, obstacleLayer))
            return false;

        return true;
    }

    Transform GetClosestEnemy()
    {
        Transform closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform enemy in enemiesInSight)
        {
            if (enemy.CompareTag("Enemy"))
            {
                float distance = Vector3.Distance(transform.position, enemy.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = enemy;
                }
            }
        }

        return closest;
    }

    void ShootAtTarget(Transform target)
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Vector3 direction = (target.position - firePoint.position).normalized;

        AutoBullet bulletScript = bullet.GetComponent<AutoBullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(direction);
            bulletScript.knockbackForce = bulletKnockback;

            if (showDebug) Debug.Log("🔫 Mermi atıldı! Hedef: " + target.name);
        }
        else
        {
            Debug.LogError("❌ AutoBullet scripti bulunamadı!");
        }
    }

    void CreateFirePoint()
    {
        GameObject firePointObj = new GameObject("FirePoint");
        firePointObj.transform.SetParent(transform);
        firePointObj.transform.localPosition = new Vector3(0, 0, 0.5f);
        firePoint = firePointObj.transform;

        if (showDebug) Debug.Log("FirePoint otomatik oluşturuldu.");
    }

    void CreateDefaultBulletPrefab()
    {
        if (showDebug) Debug.Log("❌ BulletPrefab atanmamış! Otomatik oluşturuluyor...");

        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "DefaultBullet";
        bullet.transform.localScale = Vector3.one * 0.1f;

        AutoBullet bulletScript = bullet.AddComponent<AutoBullet>();
        bulletScript.damage = 10f;
        bulletScript.bulletSpeed = bulletSpeed;
        bulletScript.bulletLifetime = bulletLifetime;
        bulletScript.knockbackForce = bulletKnockback;

        Collider collider = bullet.GetComponent<Collider>();
        collider.isTrigger = true;

        Rigidbody rb = bullet.AddComponent<Rigidbody>();
        rb.useGravity = false;

        bulletPrefab = bullet;
        bullet.SetActive(false);

        if (showDebug) Debug.Log("✅ Varsayılan mermi prefabı oluşturuldu!");
    }

    void OnDrawGizmos()
    {
        if (!showDebug) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 leftBoundary = Quaternion.Euler(0, -detectionAngle / 2, 0) * transform.forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, detectionAngle / 2, 0) * transform.forward * detectionRange;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        Gizmos.color = Color.red;
        foreach (Transform enemy in enemiesInSight)
        {
            if (enemy != null)
                Gizmos.DrawLine(transform.position, enemy.position);
        }
    }
}