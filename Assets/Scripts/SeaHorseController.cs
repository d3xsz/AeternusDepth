using UnityEngine;

public class SeahorseController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float swimSpeed = 2f;
    public float detectionRange = 5f;
    public float rotationLerpSpeed = 3f;

    [Header("Face Direction")]
    public Transform frontPoint; // Denizatının ön tarafı

    [Header("Debug")]
    public bool showDebug = false;
    public bool alwaysShowGizmos = true;

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

        // Rigidbody ayarlarını yap - gravity KAPALI, sadece yatay hareket
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        rb.linearDamping = 1f;
        rb.angularDamping = 2f;

        // Player'ı bul
        FindPlayer();

        if (showDebug && player == null)
            Debug.LogError("Seahorse: Player bulunamadı! Player'ın 'Player' tag'i olduğundan emin olun.");
    }

    void Update()
    {
        // Player yoksa bulmaya çalış
        if (player == null)
        {
            FindPlayer();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (showDebug)
        {
            Debug.Log($"Seahorse: Player mesafesi: {distanceToPlayer}, Takip: {isFollowing}");
        }

        // Player takip menzilindeyse takip et
        if (!isFollowing && distanceToPlayer <= detectionRange)
        {
            isFollowing = true;
            if (showDebug) Debug.Log("Seahorse: Player takip başladı!");
        }

        if (isFollowing)
        {
            FollowPlayer();
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

        // Denizatının ön yönünü hesapla
        Vector3 seahorseForward = GetSeahorseForward();

        // Player'a doğru yönel (Y eksenini sıfırla - sadece yatay düzlemde)
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0f;

        if (directionToPlayer != Vector3.zero)
        {
            // Mevcut ön yön ile player yönü arasındaki açıyı hesapla
            float angle = Vector3.SignedAngle(seahorseForward, directionToPlayer, Vector3.up);

            // Açıya göre dönüş uygula
            transform.Rotate(0f, angle * rotationLerpSpeed * Time.deltaTime, 0f, Space.World);
        }

        // Denizatının ön yönüne göre hareket et (SADECE YATAY)
        Vector3 moveDirection = GetSeahorseForward();
        moveDirection.y = 0f; // Y eksenini sıfırla
        moveDirection.Normalize();

        if (rb != null)
        {
            // Sadece X ve Z eksenlerinde hareket
            Vector3 newVelocity = new Vector3(moveDirection.x * swimSpeed, 0f, moveDirection.z * swimSpeed);
            rb.linearVelocity = newVelocity;
        }
        else
        {
            transform.position += moveDirection * swimSpeed * Time.deltaTime;
        }
    }

    Vector3 GetSeahorseForward()
    {
        // FrontPoint varsa onun yönünü kullan, yoksa normal forward'u kullan
        if (frontPoint != null)
        {
            Vector3 direction = (frontPoint.position - transform.position).normalized;
            direction.y = 0f; // Y eksenini sıfırla
            return direction;
        }

        Vector3 forward = transform.forward;
        forward.y = 0f; // Y eksenini sıfırla
        return forward.normalized;
    }

    // Çarpışma tespiti
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (showDebug) Debug.Log("Seahorse: Player'a çarptı!");

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

    // Gizmos - HER ZAMAN göster (seçili olmasa bile)
    void OnDrawGizmos()
    {
        if (!alwaysShowGizmos && !showDebug) return;

        // Algılama alanı
        Gizmos.color = isFollowing ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Denizatının ön yönünü göster
        Vector3 faceDirection = GetSeahorseForward();
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