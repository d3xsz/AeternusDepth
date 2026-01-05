using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CrabThief : MonoBehaviour
{
    [Header("🔧 TEMEL AYARLAR")]
    [Tooltip("Kaç saniyede bir tarama yapsın?")]
    public float scanCooldown = 20f;

    [Tooltip("Kaç saniyede bir çalabilsin?")]
    public float stealCooldown = 20f;

    [Tooltip("Ne kadar yaklaşınca çalsın?")]
    public float stealRange = 5f;

    [Header("🏃 HAREKET")]
    public float moveSpeed = 5f;

    [Header("🗺️ HARİTA DOLAŞMA")]
    [Tooltip("Yengecin gezinebileceği alanların merkez noktaları")]
    public Transform[] patrolAreas;

    [Tooltip("Her patrol alanı için gezinti yarıçapı")]
    public float patrolRadius = 10f;

    [Tooltip("Bir noktada ne kadar beklesin?")]
    public float waitTimeAtPoint = 3f;

    [Tooltip("Rastgele noktaya gitme sıklığı")]
    public float randomMoveFrequency = 8f;

    [Header("🎯 HEDEFLER")]
    public Transform[] hidingSpots;
    public GameObject[] stolenRelicPrefabs;

    // DEĞİŞKENLER
    private Transform player;
    private RelicManager relicManager;
    private CameraFOVController fovController;
    private VisionEffectController visionController;

    private float lastScanTime = 0f;
    private float lastStealTime = 0f;
    private float lastRandomMoveTime = 0f;
    private bool isScanning = false;
    private bool isMovingToPlayer = false;
    private bool isMovingToHidingSpot = false;
    private bool isPatrolling = false;
    private bool isWaiting = false;

    private int targetRelicID = -1;
    private Vector3 targetPosition;
    private Vector3 patrolTargetPoint;
    private int currentPatrolAreaIndex = 0;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        relicManager = FindObjectOfType<RelicManager>();
        fovController = FindObjectOfType<CameraFOVController>();
        visionController = FindObjectOfType<VisionEffectController>();

        // İlk rastgele noktaya git
        lastRandomMoveTime = Time.time;
        SetRandomPatrolPoint();

        Debug.Log("🦀 Yengeç aktif! Tarama süresi: " + scanCooldown + "s");
    }

    void Update()
    {
        // 1. TARAMA COOLDOWN'U KONTROL ET
        if (Time.time - lastScanTime >= scanCooldown && !isScanning && !isMovingToPlayer && !isMovingToHidingSpot && !isWaiting)
        {
            StartScan();
        }

        // 2. NORMAL DOLAŞMA (TARAMA YOKKEN)
        if (!isScanning && !isMovingToPlayer && !isMovingToHidingSpot && !isWaiting)
        {
            // Belirli aralıklarla rastgele noktaya git
            if (Time.time - lastRandomMoveTime >= randomMoveFrequency)
            {
                SetRandomPatrolPoint();
                lastRandomMoveTime = Time.time;
            }

            // Hedef noktaya doğru hareket et
            MoveToTarget(patrolTargetPoint);

            // Hedefe ulaştıysa bekle
            if (Vector3.Distance(transform.position, patrolTargetPoint) <= 1f && !isWaiting)
            {
                StartCoroutine(WaitAtPoint());
            }
        }

        // 3. OYUNCUYA HAREKET (ÇALMA MODU)
        if (isMovingToPlayer)
        {
            MoveToTarget(player.position);

            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= stealRange)
            {
                TrySteal();
            }
        }

        // 4. HIDING SPOT'A HAREKET (KAÇMA MODU)
        if (isMovingToHidingSpot)
        {
            MoveToTarget(targetPosition);

            float distance = Vector3.Distance(transform.position, targetPosition);
            if (distance <= 1f)
            {
                DropRelicAndReset();
            }
        }
    }

    void SetRandomPatrolPoint()
    {
        if (patrolAreas == null || patrolAreas.Length == 0)
        {
            Debug.LogWarning("🦀 Patrol alanı tanımlanmamış! Yengeç sabit kalacak.");
            patrolTargetPoint = transform.position;
            return;
        }

        // Rastgele bir patrol alanı seç
        currentPatrolAreaIndex = Random.Range(0, patrolAreas.Length);
        Transform selectedArea = patrolAreas[currentPatrolAreaIndex];

        // O alan içinde rastgele bir nokta seç
        Vector3 randomOffset = new Vector3(
            Random.Range(-patrolRadius, patrolRadius),
            0,
            Random.Range(-patrolRadius, patrolRadius)
        );

        patrolTargetPoint = selectedArea.position + randomOffset;

        // Yükseklik ayarla (terrain'e uyum sağlaması için)
        RaycastHit hit;
        if (Physics.Raycast(patrolTargetPoint + Vector3.up * 10f, Vector3.down, out hit, 20f))
        {
            patrolTargetPoint.y = hit.point.y + 0.5f;
        }

        Debug.Log("🦀 Yeni patrol noktası: " + patrolTargetPoint);
    }

    IEnumerator WaitAtPoint()
    {
        isWaiting = true;
        Debug.Log("🦀 Noktada bekleniyor...");

        yield return new WaitForSeconds(waitTimeAtPoint);

        isWaiting = false;
        // Bekleme bitti, yeni nokta belirle
        SetRandomPatrolPoint();
    }

    void StartScan()
    {
        isScanning = true;
        Debug.Log("🦀 Tarama başladı...");

        // Çalma cooldown'u kontrol et
        if (Time.time - lastStealTime < stealCooldown)
        {
            Debug.Log("🦀 Çalma cooldown'da, " + (stealCooldown - (Time.time - lastStealTime)).ToString("F1") + "s kaldı");
            StartCoroutine(WaitAndResetScan());
            return;
        }

        // Oyuncunun relic'leri var mı?
        if (!CanStealAnyRelic())
        {
            Debug.Log("🦀 Oyuncuda çalınabilir relic yok");
            StartCoroutine(WaitAndResetScan());
            return;
        }

        // Her şey uygun, oyuncuya git
        isMovingToPlayer = true;
        isScanning = false;
        Debug.Log("🦀 Relic bulundu! Oyuncuya gidiliyor...");
    }

    IEnumerator WaitAndResetScan()
    {
        yield return new WaitForSeconds(2f);
        ResetScan();
    }

    void ResetScan()
    {
        lastScanTime = Time.time;
        isScanning = false;
        Debug.Log("🦀 Tarama bitti, " + scanCooldown + "s sonra tekrar...");
    }

    void TrySteal()
    {
        // Çalma cooldown'u kontrol et
        if (Time.time - lastStealTime < stealCooldown)
        {
            Debug.Log("🦀 Daha yeni çaldın, " + (stealCooldown - (Time.time - lastStealTime)).ToString("F1") + "s bekle!");
            ResetAll();
            return;
        }

        // Relic seç
        List<int> availableRelics = GetAvailableRelics();
        if (availableRelics.Count == 0)
        {
            Debug.Log("🦀 Çalacak relic kalmadı!");
            ResetAll();
            return;
        }

        targetRelicID = availableRelics[Random.Range(0, availableRelics.Count)];
        Debug.Log("🦀 " + targetRelicID + ". relic çalınıyor...");

        // RELIC'İ ÇAL
        StealRelic();
    }

    void StealRelic()
    {
        // RelicManager'dan sil
        if (relicManager != null)
        {
            relicManager.RelicStolen(targetRelicID);
        }

        // FOV azalt
        if (fovController != null)
        {
            fovController.DecreaseFOV(4f);
        }

        // Madness artır
        if (visionController != null)
        {
            visionController.IncreaseMadness(0.098f);
        }

        // Hiding spot seç
        if (hidingSpots != null && hidingSpots.Length > 0)
        {
            int randomSpot = Random.Range(0, hidingSpots.Length);
            targetPosition = hidingSpots[randomSpot].position;

            // Hiding spot'a git
            isMovingToPlayer = false;
            isMovingToHidingSpot = true;

            Debug.Log("🦀 Relic çalındı! Hiding spot'a gidiliyor...");
        }
        else
        {
            Debug.LogError("🦀 Hiding spot yok!");
            ResetAll();
        }

        // Çalma zamanını kaydet
        lastStealTime = Time.time;
    }

    void DropRelicAndReset()
    {
        // Prefab spawn et
        if (stolenRelicPrefabs != null && targetRelicID >= 0 && targetRelicID < stolenRelicPrefabs.Length)
        {
            if (stolenRelicPrefabs[targetRelicID] != null)
            {
                GameObject prefab = Instantiate(
                    stolenRelicPrefabs[targetRelicID],
                    targetPosition,
                    Quaternion.identity
                );

                prefab.name = "StolenRelic_" + targetRelicID;

                // Script ekle
                StolenRelicPickup pickup = prefab.GetComponent<StolenRelicPickup>();
                if (pickup == null) pickup = prefab.AddComponent<StolenRelicPickup>();

                pickup.relicID = targetRelicID;
                pickup.fovIncrease = 4f;
                pickup.madnessReduce = 0.098f;

                Debug.Log("🦀 Relic hiding spot'a bırakıldı!");
            }
        }

        // Reset
        ResetAll();
    }

    void ResetAll()
    {
        isMovingToPlayer = false;
        isMovingToHidingSpot = false;
        isScanning = false;
        isWaiting = false;
        targetRelicID = -1;
        lastScanTime = Time.time;

        // Normal dolaşmaya geri dön
        SetRandomPatrolPoint();

        Debug.Log("🦀 Resetlendi! " + scanCooldown + "s sonra tekrar tarayacak...");
    }

    void MoveToTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 5f
            );
        }

        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    bool CanStealAnyRelic()
    {
        if (relicManager == null) return false;

        for (int i = 0; i < 5; i++)
        {
            if (relicManager.IsRelicFound(i))
            {
                return true;
            }
        }
        return false;
    }

    List<int> GetAvailableRelics()
    {
        List<int> relics = new List<int>();

        if (relicManager != null)
        {
            for (int i = 0; i < 5; i++)
            {
                if (relicManager.IsRelicFound(i))
                {
                    relics.Add(i);
                }
            }
        }

        return relics;
    }

    void OnDrawGizmosSelected()
    {
        // Patrol alanlarını görselleştir
        if (patrolAreas != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform area in patrolAreas)
            {
                if (area != null)
                {
                    Gizmos.DrawWireSphere(area.position, patrolRadius);
                    Gizmos.DrawIcon(area.position + Vector3.up * 2f, "CrabArea.png", true);
                }
            }
        }

        // Hiding spotları görselleştir
        if (hidingSpots != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform spot in hidingSpots)
            {
                if (spot != null)
                {
                    Gizmos.DrawWireCube(spot.position, Vector3.one * 2f);
                    Gizmos.DrawIcon(spot.position + Vector3.up * 2f, "CrabHide.png", true);
                }
            }
        }

        // Mevcut hedef noktayı göster
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, patrolTargetPoint);
            Gizmos.DrawSphere(patrolTargetPoint, 0.5f);
        }
    }

    [ContextMenu("🔍 TEST: Hemen Tara")]
    void TestScan()
    {
        lastScanTime = Time.time - scanCooldown - 1f;
        Debug.Log("🦀 Manuel tarama başlatıldı...");
    }

    [ContextMenu("🎯 TEST: Hemen Çal")]
    void TestSteal()
    {
        lastStealTime = Time.time - stealCooldown - 1f;
        lastScanTime = Time.time - scanCooldown - 1f;

        if (CanStealAnyRelic())
        {
            List<int> relics = GetAvailableRelics();
            targetRelicID = relics[0];
            StealRelic();
        }
        else
        {
            Debug.Log("🦀 Test: Çalacak relic yok!");
        }
    }

    [ContextMenu("📍 TEST: Yeni Nokta Belirle")]
    void TestNewPoint()
    {
        SetRandomPatrolPoint();
        Debug.Log("🦀 Yeni patrol noktası: " + patrolTargetPoint);
    }

    [ContextMenu("📊 DEBUG: Durum")]
    void DebugStatus()
    {
        Debug.Log("=== 🦀 DURUM ===");
        Debug.Log("Tarama cooldown: " + (Time.time - lastScanTime).ToString("F1") + "/" + scanCooldown + "s");
        Debug.Log("Çalma cooldown: " + (Time.time - lastStealTime).ToString("F1") + "/" + stealCooldown + "s");
        Debug.Log("Oyuncuya gidiyor: " + isMovingToPlayer);
        Debug.Log("Hiding spot'a gidiyor: " + isMovingToHidingSpot);
        Debug.Log("Taramada: " + isScanning);
        Debug.Log("Bekliyor: " + isWaiting);
        Debug.Log("Mevcut patrol alanı: " + currentPatrolAreaIndex);
        Debug.Log("Hedef nokta: " + patrolTargetPoint);
        Debug.Log("Oyuncuda relic var mı: " + CanStealAnyRelic());
    }
}