using UnityEngine;

public class JellySlime : MonoBehaviour
{
    [Header("Movement Settings")]
    public float bounceSpeed = 4f;
    public float bounceAmount = 0.2f;
    public float jumpHeight = 0.5f;
    public float jumpFrequency = 1f;
    public float detectionRange = 3f;
    public float moveSpeed = 2f;
    public float rotationLerpSpeed = 5f;

    [Header("Face Direction")]
    public Transform frontPoint; // �n taraf� belirleyen GameObject

    [Header("Debug")]
    public bool showDebug = false;
    public bool alwaysShowGizmos = true; // Yeni: Her zaman gizmos g�ster

    private Vector3 originalScale;
    private Vector3 startPosition;
    private float timeOffset;
    private Transform player;
    private bool isFollowing = false;
    private Rigidbody rb;

    void Start()
    {
        originalScale = transform.localScale;
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 100f);

        // Rigidbody yoksa ekle, varsa al
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Rigidbody ayarlar�n� yap
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearDamping = 2f; // linearDamping yerine drag
        rb.angularDamping = 2f; // angularDamping yerine angularDrag

        // Player'� bul
        FindPlayer();

        if (showDebug && player == null)
            Debug.LogError("Slime: Player bulunamad�! Player'�n 'Player' tag'i oldu�undan emin olun.");

        if (showDebug && frontPoint == null)
            Debug.LogWarning("Slime: FrontPoint atanmam��! Slime'�n �n y�n� transform.forward olarak kullan�lacak.");
    }

    void Update()
    {
        // Player yoksa bulmaya �al��
        if (player == null)
        {
            FindPlayer();
            if (player == null)
            {
                NormalAnimation();
                return;
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Player takip menzilindeyse takip et
        if (!isFollowing && distanceToPlayer <= detectionRange)
        {
            isFollowing = true;
            if (showDebug) Debug.Log("Slime: Player takip ba�lad�!");
        }

        // Takip ediyorsa player'a do�ru hareket et
        if (isFollowing)
        {
            FollowPlayer();
        }
        else
        {
            NormalAnimation();
        }

        // Her durumda bounce animasyonu uygula
        ApplyBounceAnimation();
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void NormalAnimation()
    {
        float time = Time.time + timeOffset;

        // Z�plama efekti (sadece Y pozisyonunda)
        float jump = Mathf.Abs(Mathf.Sin(time * jumpFrequency)) * jumpHeight;
        Vector3 newPosition = new Vector3(
            transform.position.x,
            startPosition.y + jump,
            transform.position.z
        );

        // Rigidbody ile hareket
        if (rb != null)
        {
            rb.MovePosition(newPosition);
        }
        else
        {
            transform.position = newPosition;
        }
    }

    void FollowPlayer()
    {
        if (player == null) return;

        // Slime'�n �n y�n�n� hesapla
        Vector3 slimeForward = GetSlimeForward();

        // Player'a do�ru y�nel
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0f; // Y eksenini s�f�rla

        if (directionToPlayer != Vector3.zero)
        {
            // Mevcut �n y�n ile player y�n� aras�ndaki a��y� hesapla
            float angle = Vector3.SignedAngle(slimeForward, directionToPlayer, Vector3.up);

            // A��ya g�re d�n�� uygula
            transform.Rotate(0f, angle * rotationLerpSpeed * Time.deltaTime, 0f, Space.World);
        }

        // Slime'�n �n y�n�ne g�re hareket et
        Vector3 moveDirection = GetSlimeForward();

        if (rb != null)
        {
            rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
        }
        else
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }

    Vector3 GetSlimeForward()
    {
        // FrontPoint varsa onun y�n�n� kullan, yoksa normal forward'u kullan
        if (frontPoint != null)
        {
            return (frontPoint.position - transform.position).normalized;
        }
        return transform.forward;
    }

    void ApplyBounceAnimation()
    {
        float time = Time.time + timeOffset;
        float bounce = Mathf.Sin(time * bounceSpeed) * bounceAmount;
        transform.localScale = new Vector3(
            originalScale.x - bounce * 0.5f,
            originalScale.y + bounce,
            originalScale.z - bounce * 0.5f
        );
    }

    // �arp��ma tespiti
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (showDebug) Debug.Log("Slime: Player'a �arpt�!");

            // �arpma sonras� hareketi durdur
            isFollowing = false;

            // Rigidbody h�z�n� s�f�rla
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    // Gizmos - HER ZAMAN g�ster (se�ili olmasa bile)
    void OnDrawGizmos()
    {
        if (!alwaysShowGizmos && !showDebug) return;

        // Alg�lama alan�
        Gizmos.color = isFollowing ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Slime'�n �n y�n�n� g�ster
        Vector3 faceDirection = GetSlimeForward();
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + faceDirection * 2f);

        // FrontPoint'i g�ster
        if (frontPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(frontPoint.position, 0.1f);
            Gizmos.DrawLine(transform.position, frontPoint.position);
        }

        // Player'a olan y�n� g�ster
        if (isFollowing && player != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }

    // Se�ildi�inde daha kal�n �izgilerle g�ster
    void OnDrawGizmosSelected()
    {
        if (!showDebug) return;

        // Daha kal�n �izgiler i�in iki kez �iz
        Gizmos.color = isFollowing ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 faceDirection = GetSlimeForward();
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + faceDirection * 2f);
        Gizmos.DrawLine(transform.position, transform.position + faceDirection * 2f);

        if (frontPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(frontPoint.position, 0.1f);
            Gizmos.DrawWireSphere(frontPoint.position, 0.1f);
            Gizmos.DrawLine(transform.position, frontPoint.position);
            Gizmos.DrawLine(transform.position, frontPoint.position);
        }
    }
}