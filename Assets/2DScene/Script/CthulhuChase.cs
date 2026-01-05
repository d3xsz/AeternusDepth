using UnityEngine;

public class CthulhuChase : MonoBehaviour
{
    [Header("Takip Ayarları")]
    public Transform player;
    public float chaseSpeed = 4f;
    public float acceleration = 2f;
    public float minDistanceFromPlayer = 3f;
    public float maxDistanceFromPlayer = 10f;

    [Header("Saldırı Ayarları")]
    public float attackRange = 1.5f;

    [Header("Görsel Ayarlar")]
    public SpriteRenderer cthulhuSprite;
    public Animator animator;

    [Header("Respawn Pozisyonu")]
    public Vector3 respawnPosition = new Vector3(0, -3f, 0);

    private Rigidbody2D rb;
    private Vector2 currentVelocity;
    private bool isChasing = false;
    private bool isPlayerReady = false;
    private Vector2 targetVelocity;
    private float smoothTime = 0.1f;

    // YENİ: Level sonu kontrolü
    private bool isLevelCompleted = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.position = respawnPosition;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (cthulhuSprite == null) cthulhuSprite = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        if (cthulhuSprite != null) cthulhuSprite.color = Color.white;

        CheckPlayerReadyAtStart();
    }

    void CheckPlayerReadyAtStart()
    {
        if (player != null)
        {
            PlayerRespawn playerRespawn = player.GetComponent<PlayerRespawn>();
            if (playerRespawn != null && playerRespawn.HasPlayerRespawned())
            {
                isPlayerReady = true;
                isChasing = true;
                Debug.Log("✅ Cthulhu: Player hazır, takip BAŞLADI");
            }
            else
            {
                Debug.Log("⏳ Cthulhu: Player hazır değil, bekleniyor...");
            }
        }
        else
        {
            isPlayerReady = true;
            isChasing = true;
            Debug.Log("⚠️ Cthulhu: Player yok, zorunlu takip");
        }
    }

    void Update()
    {
        // YENİ: Level tamamlandıysa hiçbir şey yapma
        if (isLevelCompleted) return;

        CheckPlayerReady();

        if (!isChasing || player == null || !isPlayerReady)
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetBool("IsAttacking", false);
            }
            return;
        }

        LookAtPlayer();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        // YENİ: Level tamamlandıysa takip etme
        if (isLevelCompleted) return;

        if (!isChasing || player == null || !isPlayerReady) return;
        ChasePlayer();
    }

    void CheckPlayerReady()
    {
        if (player == null) return;

        PlayerRespawn playerRespawn = player.GetComponent<PlayerRespawn>();
        if (playerRespawn != null)
        {
            bool playerReady = playerRespawn.HasPlayerRespawned();

            if (playerReady && !isPlayerReady)
            {
                isPlayerReady = true;
                isChasing = true;
                Debug.Log("🎯 Cthulhu: Player HAZIR, takip BAŞLIYOR!");
            }
            else if (!playerReady && isPlayerReady)
            {
                isPlayerReady = false;
                isChasing = false;
            }
        }
    }

    void ChasePlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 targetPosition = player.position;

        if (cthulhuSprite != null && !cthulhuSprite.flipX)
            targetPosition.x -= 3f;
        else
            targetPosition.x += 3f;

        targetPosition.y = player.position.y - 1f;
        Vector2 directionToTarget = (targetPosition - (Vector2)transform.position).normalized;

        float speedMultiplier = 1f;
        if (distanceToPlayer > maxDistanceFromPlayer) speedMultiplier = 1.8f;
        else if (distanceToPlayer < minDistanceFromPlayer) speedMultiplier = 0.6f;

        targetVelocity = directionToTarget * chaseSpeed * speedMultiplier;
        currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime * smoothTime);
        currentVelocity = Vector2.ClampMagnitude(currentVelocity, chaseSpeed * 2f);
        rb.linearVelocity = currentVelocity;

        if (distanceToPlayer <= attackRange) AttackPlayer();
    }

    void LookAtPlayer()
    {
        if (player == null || cthulhuSprite == null) return;
        Vector3 direction = player.position - transform.position;
        cthulhuSprite.flipX = direction.x < -0.1f;
    }

    void UpdateAnimations()
    {
        if (animator == null) return;
        animator.SetFloat("Speed", currentVelocity.magnitude);
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        animator.SetBool("IsAttacking", distanceToPlayer <= attackRange * 1.5f);
    }

    void AttackPlayer()
    {
        // YENİ: Level tamamlandıysa saldırma
        if (isLevelCompleted) return;

        Debug.Log("🐙 Cthulhu player'ı YAKALADI!");
        ResetCthulhuImmediately();

        if (player != null)
        {
            PlayerSwimController swimController = player.GetComponent<PlayerSwimController>();
            if (swimController != null) swimController.ResetAllEffects();

            PlayerRespawn respawn = player.GetComponent<PlayerRespawn>();
            if (respawn != null)
            {
                respawn.StartGhostRespawn(player.position);
                isPlayerReady = false;
            }
        }

        if (animator != null) animator.SetTrigger("Attack");
        ClearAnchors();
    }

    void ClearAnchors()
    {
        AnchorSpawner[] allSpawners = FindObjectsOfType<AnchorSpawner>();
        foreach (AnchorSpawner spawner in allSpawners)
            if (spawner != null) spawner.ClearAllAnchors();
    }

    public void OnPlayerRespawnComplete()
    {
        isPlayerReady = true;
        isChasing = true;
        Debug.Log("🎯 Cthulhu: Player hazır, takip BAŞLADI");
    }

    public void ResetCthulhuImmediately()
    {
        currentVelocity = Vector2.zero;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        transform.position = respawnPosition;
        isChasing = false;
        isPlayerReady = false;
    }

    // YENİ: Level tamamlandığında çağrılacak
    public void OnLevelCompleted()
    {
        isLevelCompleted = true;
        isChasing = false;

        // Hızı sıfırla
        currentVelocity = Vector2.zero;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Animasyonları durdur
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsAttacking", false);
        }

        Debug.Log("🎉 Cthulhu: LEVEL TAMAMLANDI, takip DURDURULDU!");
    }

    public void ResetCthulhu() => ResetCthulhuImmediately();
    public void SetChaseSpeed(float newSpeed) => chaseSpeed = newSpeed;
}