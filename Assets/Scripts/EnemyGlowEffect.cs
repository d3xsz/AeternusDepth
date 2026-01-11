using UnityEngine;
using System.Collections;

public class EnemyGlowEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    public Color glowColor = Color.red;
    public float glowIntensity = 2f;
    public float pulseSpeed = 2f;
    public float minIntensity = 1f;
    public float maxIntensity = 3f;

    [Header("Shader Settings")]
    public bool useStandardShader = true; // Standart shader kullanıyorsan
    public bool createNewMaterial = true; // Yeni materyal oluştur

    private Renderer enemyRenderer;
    private Material glowMaterial;
    private Material originalMaterial; // Orijinal materyali sakla
    private bool isPulsing = true;

    void Start()
    {
        enemyRenderer = GetComponentInChildren<Renderer>();

        if (enemyRenderer != null)
        {
            // Orijinal materyali sakla
            originalMaterial = enemyRenderer.sharedMaterial;

            if (createNewMaterial)
            {
                // Yeni bir materyal oluştur (clone)
                glowMaterial = new Material(enemyRenderer.material);
                glowMaterial.name = originalMaterial.name + "_Glow";
            }
            else
            {
                // Mevcut materyali kullan
                glowMaterial = enemyRenderer.material;
            }

            // Renderer'a atama
            enemyRenderer.material = glowMaterial;

            // Glow özelliklerini ayarla
            SetupGlowMaterial();

            StartCoroutine(PulseGlow());
        }
        else
        {
            Debug.LogError("Renderer bulunamadı!", this);
        }
    }

    void SetupGlowMaterial()
    {
        if (glowMaterial == null) return;

        if (useStandardShader)
        {
            // STANDARD SHADER için (Build'de çalışması için)
            SetupStandardShaderGlow();
        }
        else
        {
            // CUSTOM SHADER veya diğer shader'lar için
            SetupCustomShaderGlow();
        }
    }

    void SetupStandardShaderGlow()
    {
        // Standard shader için özel ayarlar
        // 1. Emission property'yi aktif et
        glowMaterial.EnableKeyword("_EMISSION");

        // 2. Emission color'ı ayarla
        glowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);

        // 3. GI (Global Illumination) için ayarla
        glowMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        // 4. Material'ı dynamic batching için işaretle (build optimizasyonu)
        glowMaterial.enableInstancing = true;

        Debug.Log("✅ Standard Shader glow ayarlandı");
    }

    void SetupCustomShaderGlow()
    {
        // Outline veya custom shader kontrolü
        if (glowMaterial.HasProperty("_Color"))
        {
            glowMaterial.SetColor("_Color", glowColor);
        }

        if (glowMaterial.HasProperty("_EmissionColor"))
        {
            glowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
        }

        if (glowMaterial.HasProperty("_OutlineColor"))
        {
            glowMaterial.SetColor("_OutlineColor", glowColor);
            glowMaterial.SetFloat("_OutlineWidth", 0.05f);
        }
    }

    IEnumerator PulseGlow()
    {
        while (isPulsing)
        {
            if (glowMaterial != null && enemyRenderer != null)
            {
                float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);

                if (useStandardShader && glowMaterial.HasProperty("_EmissionColor"))
                {
                    glowMaterial.SetColor("_EmissionColor", glowColor * currentIntensity);

                    // Build'de emission güncellemesi için
                    DynamicGI.SetEmissive(enemyRenderer, glowColor * currentIntensity);
                }
                else if (glowMaterial.HasProperty("_Color"))
                {
                    // Alternatif: Renk değişimi
                    Color pulseColor = Color.Lerp(glowColor * minIntensity, glowColor * maxIntensity, pulse);
                    glowMaterial.SetColor("_Color", pulseColor);
                }
            }

            yield return null;
        }
    }

    // BUILD'DE ÇALIŞMASI İÇİN EK
    void OnEnable()
    {
        isPulsing = true;
        if (glowMaterial != null && enemyRenderer != null)
        {
            // Emission'ı yeniden aktif et
            glowMaterial.EnableKeyword("_EMISSION");
        }
    }

    void OnDisable()
    {
        isPulsing = false;
    }

    public void SetGlowColor(Color newColor)
    {
        glowColor = newColor;
        if (glowMaterial != null)
        {
            glowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
        }
    }

    public void StopGlow()
    {
        isPulsing = false;

        // Orijinal materyale dön
        if (enemyRenderer != null && originalMaterial != null)
        {
            enemyRenderer.material = originalMaterial;
        }
    }

    public void RestartGlow()
    {
        isPulsing = true;

        if (enemyRenderer != null && glowMaterial != null)
        {
            enemyRenderer.material = glowMaterial;
            SetupGlowMaterial();
        }
    }

    void OnDestroy()
    {
        isPulsing = false;

        // Sadece kendi oluşturduğumuz materyali yok et
        if (createNewMaterial && glowMaterial != null)
        {
            Destroy(glowMaterial);
        }
    }
}