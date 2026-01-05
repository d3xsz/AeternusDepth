using UnityEngine;

public class TentacleTrap : MonoBehaviour
{
    [Header("Sallanma Ayarlarý")]
    public float swingSpeed = 0.7f;
    public float swingAmount = 40f;

    [Header("Tuzak Ayarlarý")]
    public float slowAmount = 0.5f;
    public float slowDuration = 2f;

    [Header("Görsel")]
    public SpriteRenderer tentacleSprite;

    [Header("Yön Ayarlarý")]
    public bool flipX = false; // Editor'da buradan kontrol et
    public bool flipY = false;

    private float timer = 0f;
    private Vector3 basePosition;
    private Quaternion baseRotation;
    private float directionMultiplier = 1f;

    void Start()
    {
        basePosition = transform.position;
        baseRotation = transform.rotation;

        // Sprite flip ayarlarýný uygula
        if (tentacleSprite != null)
        {
            tentacleSprite.flipX = flipX;
            tentacleSprite.flipY = flipY;
        }

        // Flip durumuna göre yön çarpanýný ayarla
        directionMultiplier = flipX ? -1f : 1f;

        Debug.Log($"Tentacle baþlatýldý. FlipX: {flipX}, Direction: {directionMultiplier}");
    }

    void Update()
    {
        timer += Time.deltaTime * swingSpeed;

        // Yavaþ sallanma
        float swing = Mathf.Sin(timer) * swingAmount * directionMultiplier;

        // Rotasyonu uygula
        transform.rotation = Quaternion.Euler(0, 0, swing);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player dokunacaða çarptý!");
            ApplySlowEffect();
            StartCoroutine(GrabEffect());
        }
    }

    void ApplySlowEffect()
    {
        // DÜZELTÝLMÝÞ KOD: Artýk bu metod GameManager'da var
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerHitByTentacle(slowAmount, slowDuration);
        }
        else
        {
            // Alternatif: direkt PlayerSwimController'a eriþ
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerSwimController swimController = player.GetComponent<PlayerSwimController>();
                if (swimController != null)
                {
                    swimController.ApplySlow(slowAmount, slowDuration);
                }
            }
        }
    }

    System.Collections.IEnumerator GrabEffect()
    {
        if (tentacleSprite != null)
        {
            Color originalColor = tentacleSprite.color;
            tentacleSprite.color = Color.red;

            yield return new WaitForSeconds(0.2f);

            tentacleSprite.color = originalColor;
        }
    }

    // Gizmos ile görselleþtirme
    void OnDrawGizmosSelected()
    {
        // Pivot noktasý
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.15f);

        // Sallanma yönü
        Vector3 direction = (flipX ? Vector3.left : Vector3.right);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, direction * 2f);
    }

    // EDITOR ÝÇÝN: Inspector'da deðiþiklik olduðunda
    void OnValidate()
    {
        if (tentacleSprite != null)
        {
            tentacleSprite.flipX = flipX;
            tentacleSprite.flipY = flipY;
        }
    }
}