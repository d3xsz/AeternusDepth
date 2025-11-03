using UnityEngine;

public class RollingCharacter : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;         // Player'a doğru hareket hızı
    public float taklaSpeed = 360f;      // X ekseninde takla hızı
    public float detectionRange = 3f;    // Player'ı algılama mesafesi
    public float rotationLerpSpeed = 5f; // Player'a yönelme hızı

    [Header("Face Direction")]
    public Transform frontPoint; // Ön tarafı belirleyen GameObject

    [Header("Debug")]
    public bool showDebug = true;

    private Transform player;
    private bool isFollowing = false;
    private Rigidbody rb;

    void Start()
    {
        // Rigidbody yoksa ekle, varsa al
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Rigidbody ayarlarını yap
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearDamping = 2f;
        rb.angularDamping = 2f;

        // Player'ı bul
        FindPlayer();

        if (showDebug && player == null)
            Debug.LogError("RollingCharacter: Player bulunamadı! Player'ın 'Player' tag'i olduğundan emin olun.");

        if (showDebug && frontPoint == null)
            Debug.LogWarning("RollingCharacter: FrontPoint atanmamış! Karakterin ön yönü transform.forward olarak kullanılacak.");
    }

    void Update()
    {
        // Player yoksa bulmaya çalış
        if (player == null)
        {
            FindPlayer();
            if (player == null)
            {
                // Player yoksa sadece takla at
                if (!isFollowing)
                {
                    transform.Rotate(taklaSpeed * Time.deltaTime, 0f, 0f, Space.Self);
                }
                return;
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Player takip menzilindeyse takip et
        if (!isFollowing && distanceToPlayer <= detectionRange)
        {
            isFollowing = true;
            if (showDebug) Debug.Log("RollingCharacter: Player takip başladı!");
        }

        // Takip ediyorsa player'a doğru hareket et
        if (isFollowing)
        {
            FollowPlayer();
        }
        else
        {
            // Takip etmiyorsa sadece takla at
            transform.Rotate(taklaSpeed * Time.deltaTime, 0f, 0f, Space.Self);
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void FollowPlayer()
    {
        if (player == null) return;

        // Karakterin ön yönünü hesapla
        Vector3 characterForward = GetCharacterForward();

        // Player'a doğru yönel
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0f; // Y eksenini sıfırla

        if (directionToPlayer != Vector3.zero)
        {
            // Mevcut ön yön ile player yönü arasındaki açıyı hesapla
            float angle = Vector3.SignedAngle(characterForward, directionToPlayer, Vector3.up);

            // Açıya göre dönüş uygula
            transform.Rotate(0f, angle * rotationLerpSpeed * Time.deltaTime, 0f, Space.World);
        }

        // Karakterin ön yönüne göre hareket et
        Vector3 moveDirection = GetCharacterForward();

        if (rb != null)
        {
            rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
        }
        else
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }

        // X ekseninde sürekli takla
        transform.Rotate(taklaSpeed * Time.deltaTime, 0f, 0f, Space.Self);
    }

    Vector3 GetCharacterForward()
    {
        // FrontPoint varsa onun yönünü kullan, yoksa normal forward'u kullan
        if (frontPoint != null)
        {
            return (frontPoint.position - transform.position).normalized;
        }
        return transform.forward;
    }

    // Çarpışma tespiti
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (showDebug) Debug.Log("RollingCharacter: Player'a çarptı!");

            // Çarpma sonrası hareketi durdur
            isFollowing = false;

            // Rigidbody hızını sıfırla
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    // Gizmos ile algılama alanını ve yönü göster
    void OnDrawGizmosSelected()
    {
        // Algılama alanı
        Gizmos.color = isFollowing ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Karakterin ön yönünü göster
        Vector3 faceDirection = GetCharacterForward();
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + faceDirection * 2f);

        // FrontPoint'i göster
        if (frontPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(frontPoint.position, 0.1f);
            Gizmos.DrawLine(transform.position, frontPoint.position);
        }

        // Player'a olan yönü göster
        if (isFollowing && player != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}