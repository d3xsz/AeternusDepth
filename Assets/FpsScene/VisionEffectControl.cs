using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VisionEffectController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume volume;
    [SerializeField] private CameraShake cameraShake;

    [Header("Vignette Settings")]
    [SerializeField, Range(0f, 1f)] private float startIntensity = 0.75f;
    [SerializeField, Range(0f, 1f)] private float minIntensity = 0.2f;
    [SerializeField, Range(0f, 1f)] private float maxIntensity = 0.75f;

    [Header("Shake Settings")]
    [SerializeField] private float maxShakeFromMadness = 0.03f;
    [SerializeField] private float shakeReductionPerRelic = 0.1f;

    [Header("Smooth Settings")]
    [SerializeField] private float lerpSpeed = 2f;

    private Vignette vignette;
    private float targetIntensity;
    private float currentMadness = 0f;

    void Start()
    {
        if (volume.profile.TryGet(out vignette))
        {
            vignette.intensity.value = startIntensity;
            targetIntensity = startIntensity;
            currentMadness = startIntensity;
        }

        if (cameraShake != null)
        {
            cameraShake.AddShake(currentMadness);
        }
    }

    void Update()
    {
        if (vignette == null) return;

        vignette.intensity.value = Mathf.Lerp(
            vignette.intensity.value,
            targetIntensity,
            Time.deltaTime * lerpSpeed
        );

        currentMadness = vignette.intensity.value;
        UpdateShakeFromMadness();
    }

    void UpdateShakeFromMadness()
    {
        if (cameraShake != null)
        {
            float shakeAmount = (currentMadness - minIntensity) / (maxIntensity - minIntensity);
            shakeAmount = Mathf.Clamp01(shakeAmount) * maxShakeFromMadness;
            cameraShake.AddShake(shakeAmount);
        }
    }

    public void ReduceMadness(float amount)
    {
        targetIntensity -= amount;
        targetIntensity = Mathf.Clamp(targetIntensity, minIntensity, maxIntensity);

        if (cameraShake != null)
        {
            cameraShake.ReduceShake(shakeReductionPerRelic);
        }
    }

    // YENÝ: Madness artýrma (yengeç çalýnca)
    public void IncreaseMadness(float amount)
    {
        targetIntensity += amount;
        targetIntensity = Mathf.Clamp(targetIntensity, minIntensity, maxIntensity);
    }

    public void SetMadness(float madnessLevel)
    {
        targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, madnessLevel);
        currentMadness = madnessLevel;
    }
}