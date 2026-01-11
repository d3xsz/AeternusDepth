using UnityEngine;
using System.Collections.Generic;

public class AutoBulletShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 25f;
    public float bulletLifetime = 2f;

    [Header("Knockback Settings")]
    public float bulletKnockback = 15f;

    [Header("Target Detection")]
    public float detectionRange = 10f;
    public float detectionAngle = 60f;
    public LayerMask enemyLayer = 1;
    public LayerMask obstacleLayer = 1;

    [Header("Debug")]
    public bool showDebug = true;

    private List<Transform> enemiesInSight = new List<Transform>();
    private Weapon weaponScript;

    // REWARD SİSTEM
    private float baseBulletSpeed;
    private float baseBulletKnockback;
    private float baseDamage = 10f;

    void Start()
    {
        weaponScript = GetComponent<Weapon>();

        // BAŞLANGIÇ DEĞERLERİNİ KAYDET
        baseBulletSpeed = bulletSpeed;
        baseBulletKnockback = bulletKnockback;

        Debug.Log($"🔫 AutoBulletShooter Başlangıç: BaseSpeed={baseBulletSpeed}");

        if (firePoint == null) CreateFirePoint();
        if (bulletPrefab == null) CreateDefaultBulletPrefab();
    }

    void Update()
    {
        if (!enabled) return;
        if (weaponScript == null || !weaponScript.isEquipped) return;

        // SADECE SOL TIKLA ATEŞ ET - BEKLEME SÜRESİ OLMADAN
        if (Input.GetMouseButtonDown(0))
        {
            ShootForward();
        }

        // SÜREKLİ ATEŞ İSTİYORSANIZ BU KODU KULLANIN:
        // if (Input.GetMouseButton(0))
        // {
        //     ShootForward();
        // }
    }

    void ShootForward()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // ATEŞ SESİNİ ÇAL
        if (weaponScript != null)
        {
            weaponScript.PlayShootSound();
        }

        // BONUSLARI HESAPLA
        float damageMultiplier = PlayerStats.Instance?.GetDamageMultiplier() ?? 1f;
        float speedMultiplier = PlayerStats.Instance?.GetBulletSpeedMultiplier() ?? 1f;

        float finalSpeed = baseBulletSpeed * speedMultiplier;
        float finalDamage = baseDamage * damageMultiplier;
        float finalKnockback = baseBulletKnockback * damageMultiplier;

        Debug.Log($"🔫 Ateş Ediliyor: Speed={finalSpeed} (Base: {baseBulletSpeed} x {speedMultiplier})");

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        AutoBullet bulletScript = bullet.GetComponent<AutoBullet>();

        if (bulletScript != null)
        {
            // Mermiyi başlat - KRİTİK: finalSpeed parametresini gönder
            bulletScript.Initialize(firePoint.forward, finalSpeed, finalDamage);
            bulletScript.knockbackForce = finalKnockback;
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
        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "DefaultBullet";
        bullet.transform.localScale = Vector3.one * 0.1f;

        AutoBullet bulletScript = bullet.AddComponent<AutoBullet>();
        bulletScript.damage = baseDamage;
        bulletScript.bulletSpeed = baseBulletSpeed;
        bulletScript.bulletLifetime = bulletLifetime;
        bulletScript.knockbackForce = baseBulletKnockback;

        Collider collider = bullet.GetComponent<Collider>();
        collider.isTrigger = true;
        Rigidbody rb = bullet.AddComponent<Rigidbody>();
        rb.useGravity = false;

        bulletPrefab = bullet;
        bullet.SetActive(false);
        Debug.Log("✅ Varsayılan mermi prefabı oluşturuldu!");
    }

    void OnDrawGizmos()
    {
        if (!showDebug) return;
        Gizmos.color = Color.green;
        Gizmos.DrawRay(firePoint.position, firePoint.forward * 5f);
    }
}