using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OxygenSystem : MonoBehaviour
{
    [Header("Oksijen Ayarları")]
    [SerializeField] private float maxOxygen = 100f;
    [SerializeField] private float oxygenDrainRate = 15f; // Saniyede azalma miktarı

    [Header("UI Referansları - IMAGE FILL")]
    [SerializeField] private Image oxygenFillImage;      // BU ÖNEMLİ: Filled Image
    [SerializeField] private Color normalColor = Color.cyan;
    [SerializeField] private Color lowColor = Color.red;
    [SerializeField] private float lowOxygenThreshold = 30f; // %30

    [Header("Efektler")]
    [SerializeField] private AudioClip lowOxygenSound;
    [SerializeField] private ParticleSystem bubblesParticles;
    [SerializeField] private float bubbleRateNormal = 5f;
    [SerializeField] private float bubbleRateLow = 20f;

    [Header("Görsel Ayarları")]
    public bool useGradientColor = true;
    public Color mediumColor = new Color(0f, 0.5f, 1f, 1f); // Orta mavi

    // Değişkenler
    private float currentOxygen;
    private bool isOxygenLow = false;
    private AudioSource audioSource;
    private Coroutine oxygenRoutine;
    private bool isSystemRunning = false;

    // Event'ler
    public System.Action OnOxygenDepleted;
    public System.Action OnOxygenLow;
    public System.Action OnOxygenNormal;

    void Start()
    {
        currentOxygen = maxOxygen;
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Image kontrolü
        ValidateImageSettings();

        UpdateUI();
        StartOxygenSystem();
    }

    void ValidateImageSettings()
    {
        if (oxygenFillImage != null)
        {
            // Image Type kontrolü
            if (oxygenFillImage.type != Image.Type.Filled)
            {
                Debug.LogWarning($"⚠️ OxygenFillImage Image Type: {oxygenFillImage.type}, 'Filled' olmalı!");
                oxygenFillImage.type = Image.Type.Filled;
            }

            // Fill Method kontrolü
            if (oxygenFillImage.fillMethod != Image.FillMethod.Horizontal)
            {
                Debug.LogWarning($"⚠️ OxygenFillImage FillMethod: {oxygenFillImage.fillMethod}, 'Horizontal' olmalı!");
                oxygenFillImage.fillMethod = Image.FillMethod.Horizontal;
            }

            // Fill Origin kontrolü
            if (oxygenFillImage.fillOrigin != (int)Image.OriginHorizontal.Left)
            {
                Debug.LogWarning($"⚠️ OxygenFillImage FillOrigin: {oxygenFillImage.fillOrigin}, 'Left' olmalı!");
                oxygenFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            }

            // Başlangıçta tam dolu
            oxygenFillImage.fillAmount = 1f;
            Debug.Log("✅ OxygenFillImage ayarları kontrol edildi ve düzeltildi");
        }
        else
        {
            Debug.LogError("❌ OxygenFillImage referansı bağlanmamış!");
        }
    }

    // OKSİJEN SİSTEMİNİ BAŞLAT
    public void StartOxygenSystem()
    {
        if (isSystemRunning) return;

        isSystemRunning = true;

        if (oxygenRoutine != null)
            StopCoroutine(oxygenRoutine);

        oxygenRoutine = StartCoroutine(OxygenUpdateRoutine());
        Debug.Log("▶️ OxygenSystem başlatıldı");
    }

    // OKSİJEN SİSTEMİNİ DURDUR
    public void StopOxygenSystem()
    {
        if (!isSystemRunning) return;

        isSystemRunning = false;

        if (oxygenRoutine != null)
        {
            StopCoroutine(oxygenRoutine);
            oxygenRoutine = null;
        }

        // Ses efekti durdur
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        Debug.Log("⏹️ OxygenSystem durduruldu");
    }

    IEnumerator OxygenUpdateRoutine()
    {
        while (isSystemRunning)
        {
            // HER ZAMAN azalıyor
            if (currentOxygen > 0)
            {
                currentOxygen -= oxygenDrainRate * Time.deltaTime;
                currentOxygen = Mathf.Max(currentOxygen, 0);

                if (currentOxygen <= lowOxygenThreshold && !isOxygenLow)
                {
                    OnLowOxygen();
                }
                else if (currentOxygen > lowOxygenThreshold && isOxygenLow)
                {
                    OnOxygenRestored();
                }

                if (currentOxygen <= 0)
                {
                    Debug.Log("💀 OKSİJEN BİTTİ - ÖLÜYORSUN!");

                    // Event'i tetikle
                    OnOxygenDepleted?.Invoke();

                    // Ölümü tetikle
                    PlayerRespawn respawn = GetComponent<PlayerRespawn>();
                    if (respawn != null)
                    {
                        respawn.HandleDeath();
                    }

                    // Sistem durdurulacak (PlayerRespawn'da yapılacak)
                    break;
                }
            }

            UpdateUI();
            UpdateEffects();
            yield return null;
        }
    }

    void OnLowOxygen()
    {
        isOxygenLow = true;
        OnOxygenLow?.Invoke();

        if (lowOxygenSound != null)
        {
            audioSource.clip = lowOxygenSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        Debug.Log("⚠️ DÜŞÜK OKSİJEN UYARISI!");
    }

    void OnOxygenRestored()
    {
        isOxygenLow = false;
        OnOxygenNormal?.Invoke();

        if (audioSource.isPlaying)
            audioSource.Stop();

        Debug.Log("✅ OKSİJEN NORMALE DÖNDÜ");
    }

    void UpdateUI()
    {
        if (oxygenFillImage != null)
        {
            // Fill amount'u güncelle
            float oxygenPercent = currentOxygen / maxOxygen;
            oxygenFillImage.fillAmount = oxygenPercent;

            // Renk güncelle
            UpdateColor(oxygenPercent);
        }
    }

    void UpdateColor(float oxygenPercent)
    {
        if (oxygenFillImage == null) return;

        if (useGradientColor)
        {
            // Gradient renk efekti
            if (oxygenPercent > 0.5f)
            {
                // %50'nin üstü: Normal -> Medium
                float t = (oxygenPercent - 0.5f) * 2f;
                oxygenFillImage.color = Color.Lerp(mediumColor, normalColor, t);
            }
            else
            {
                // %50'nin altı: Medium -> Low
                float t = oxygenPercent * 2f;
                oxygenFillImage.color = Color.Lerp(lowColor, mediumColor, t);
            }
        }
        else
        {
            // Basit renk geçişi
            oxygenFillImage.color = Color.Lerp(lowColor, normalColor, oxygenPercent);
        }
    }

    void UpdateEffects()
    {
        if (bubblesParticles != null)
        {
            var emission = bubblesParticles.emission;
            emission.rateOverTime = isOxygenLow ? bubbleRateLow : bubbleRateNormal;

            if (currentOxygen <= 0)
                emission.rateOverTime = 0;
        }
    }

    // OKSİJENİ TAMAMEN DOLDUR
    public void RefillOxygen()
    {
        currentOxygen = maxOxygen;
        isOxygenLow = false;

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        // UI'ı hemen güncelle
        if (oxygenFillImage != null)
        {
            oxygenFillImage.fillAmount = 1f;
            UpdateColor(1f);
        }

        Debug.Log("✅ OKSİJEN TAMAMEN DOLDU");
    }

    // OKSİJENİ BOŞALT
    public void DepleteOxygen()
    {
        currentOxygen = 0;

        if (oxygenFillImage != null)
        {
            oxygenFillImage.fillAmount = 0f;
            UpdateColor(0f);
        }

        Debug.Log("⚠️ OKSİJEN BOŞALTILDI");
    }

    // HARİCİ ETKİLER İÇİN (potion gibi)
    public void ModifyDrainRate(float multiplier, float duration)
    {
        StartCoroutine(TemporaryDrainModifier(multiplier, duration));
    }

    IEnumerator TemporaryDrainModifier(float multiplier, float duration)
    {
        float originalRate = oxygenDrainRate;
        oxygenDrainRate *= multiplier;

        Debug.Log($"🧪 Oksijen tüketimi {multiplier}x değişti: {oxygenDrainRate}/s");

        yield return new WaitForSeconds(duration);

        oxygenDrainRate = originalRate;
        Debug.Log($"✅ Oksijen tüketimi normale döndü: {oxygenDrainRate}/s");
    }

    // GETTER METODLARI
    public float GetOxygenPercent() => currentOxygen / maxOxygen;
    public bool IsOxygenLow() => isOxygenLow;
    public float GetCurrentOxygen() => currentOxygen;
    public bool IsSystemRunning() => isSystemRunning;

    // YÜZEY KONTROLÜ (isteğe bağlı)
    public void SetAtSurface(bool atSurface)
    {
        Debug.Log(atSurface ? "🌊 YÜZEYDESİN" : "🐠 DERİNDESİN");
    }

    // DEBUG METODLARI
    [ContextMenu("Test Oksijen %50")]
    void TestOxygen50()
    {
        currentOxygen = maxOxygen * 0.5f;
        UpdateUI();
        Debug.Log($"🧪 Test: Oksijen %50'ye ayarlandı");
    }

    [ContextMenu("Test Oksijen %20")]
    void TestOxygen20()
    {
        currentOxygen = maxOxygen * 0.2f;
        UpdateUI();
        Debug.Log($"🧪 Test: Oksijen %20'ye ayarlandı");
    }

    [ContextMenu("Test Oksijen Tam Doldur")]
    void TestRefill()
    {
        RefillOxygen();
    }

    [ContextMenu("Test Sistem Başlat")]
    void TestStartSystem()
    {
        StartOxygenSystem();
    }

    [ContextMenu("Test Sistem Durdur")]
    void TestStopSystem()
    {
        StopOxygenSystem();
    }

    // ON DESTROY
    void OnDestroy()
    {
        StopOxygenSystem();
    }
}