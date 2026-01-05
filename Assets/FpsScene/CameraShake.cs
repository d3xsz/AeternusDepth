using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float maxShakeAmount = 0.05f; // ÇOK HAFÝF
    [SerializeField] private float shakeDecreaseSpeed = 1.5f;

    private float currentShakeAmount = 0f;
    private Vector3 originalPosition;
    private Transform camTransform;

    void Start()
    {
        camTransform = GetComponent<Transform>();
        originalPosition = camTransform.localPosition;
    }

    void Update()
    {
        if (currentShakeAmount > 0)
        {
            // Rastgele titreme
            Vector3 shakeOffset = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ) * currentShakeAmount;

            camTransform.localPosition = originalPosition + shakeOffset;

            // Titremeyi zamanla azalt
            currentShakeAmount -= Time.deltaTime * shakeDecreaseSpeed;
            currentShakeAmount = Mathf.Clamp01(currentShakeAmount);
        }
        else
        {
            // Orijinal pozisyona dön
            camTransform.localPosition = originalPosition;
        }
    }

    // Delilik seviyesine göre shake ekle
    public void AddShake(float madnessLevel)
    {
        currentShakeAmount = Mathf.Clamp01(currentShakeAmount + (madnessLevel * maxShakeAmount));
    }

    // Relic toplayýnca shake'i azalt
    public void ReduceShake(float reduction)
    {
        currentShakeAmount = Mathf.Clamp01(currentShakeAmount - reduction);
    }
}