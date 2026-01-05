using UnityEngine;
using System.Collections;

public class AnchorTrap : MonoBehaviour
{
    [Header("Çapa Ayarları")]
    public float fallSpeed = 8f;
    public float rotationSpeed = 180f;

    [Header("Yavaşlatma")]
    public float slowAmount = 0.5f;
    public float slowDuration = 2f;

    [Header("Görsel")]
    public SpriteRenderer anchorSprite;

    private bool hasHit = false;

    void Update()
    {
        if (!hasHit)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }

        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    // AnchorTrap.cs'de değiştirme, ApplySlow kalsın
    // AnchorTrap.cs'de değiştirme, ApplySlow kalsın
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (other.CompareTag("Player"))
        {
            hasHit = true;

            PlayerSwimController swimController = other.GetComponent<PlayerSwimController>();
            if (swimController != null)
            {
                swimController.ApplySlow(slowAmount, slowDuration); // LACİVERT RENK
            }

            StartCoroutine(HitEffect());
            Destroy(gameObject, 0.3f);
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }

    IEnumerator HitEffect()
    {
        if (anchorSprite != null)
        {
            Color originalColor = anchorSprite.color;
            anchorSprite.color = Color.yellow;
            yield return new WaitForSeconds(0.1f);
            anchorSprite.color = originalColor;
        }
    }
}