using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DreamHallucinationEffect : MonoBehaviour
{
    [Header("Halüsinasyon Görselleri")]
    [SerializeField] private List<Sprite> hallucinationSprites;

    [Header("SPAWN POZİSYONLARI")]
    [SerializeField] private List<Transform> spawnPositions;

    [Header("Yanıp Sönme Ayarları")]
    [SerializeField] private float minOpacity = 0.1f;
    [SerializeField] private float maxOpacity = 0.3f;
    [SerializeField] private float pulseFrequency = 8f; // Yanıp sönme hızı
    [SerializeField] private float pulseIntensity = 0.5f; // Yanıp sönme şiddeti

    [Header("Boyut Değişim Ayarları")]
    [SerializeField] private float minScale = 0.8f; // Minimum boyut
    [SerializeField] private float maxScale = 1.2f; // Maximum boyut
    [SerializeField] private bool synchronizeScaleWithOpacity = true; // Opacity ile boyut senkronizasyonu
    [SerializeField] private float scaleIntensity = 0.7f; // Boyut değişim şiddeti

    [Header("Sorting Ayarları")]
    [SerializeField] private string sortingLayerName = "Background";
    [SerializeField] private int orderInLayer = -1;

    [Header("Referanslar")]
    [SerializeField] private GameObject hallucinationPrefab;

    private List<GameObject> activeHallucinations = new List<GameObject>();
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();

    void Start()
    {
        // Hata kontrolleri
        if (hallucinationSprites == null || hallucinationSprites.Count == 0)
        {
            Debug.LogError("❌ SPRITE LİSTESİ BOŞ!");
            return;
        }

        if (spawnPositions == null || spawnPositions.Count == 0)
        {
            Debug.LogError("❌ SPAWN POZİSYONLARI BOŞ!");
            return;
        }

        if (hallucinationPrefab == null)
        {
            Debug.LogError("❌ PREFAB ATANMAMIŞ!");
            return;
        }

        // Sorting layer kontrolü
        if (!SortingLayerExists(sortingLayerName))
        {
            sortingLayerName = "Default";
        }

        // TÜM POZİSYONLARA GÖRSELLERİ YERLEŞTİR
        CreateAllHallucinations();
    }

    void CreateAllHallucinations()
    {
        // Önce temizle
        ClearAllHallucinations();

        // Her spawn pozisyonuna bir görsel yerleştir
        foreach (Transform spawnPos in spawnPositions)
        {
            if (spawnPos == null) continue;

            CreatePermanentHallucination(spawnPos.position);
        }

        Debug.Log($"✅ {activeHallucinations.Count} görsel oluşturuldu - SÜREKLİ YANIP SÖNECEK VE BÜYÜYÜP KÜÇÜLECEK");
    }

    void CreatePermanentHallucination(Vector3 position)
    {
        // Rastgele sprite seç
        Sprite randomSprite = hallucinationSprites[Random.Range(0, hallucinationSprites.Count)];

        // Prefab'dan yeni obje oluştur
        GameObject hallucination = Instantiate(hallucinationPrefab, position, Quaternion.identity);
        hallucination.transform.SetParent(transform);

        // Başlangıç boyutunu kaydet
        Vector3 baseScale = hallucination.transform.localScale;
        originalScales[hallucination] = baseScale;

        // SpriteRenderer ayarla
        SpriteRenderer sr = hallucination.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = randomSprite;
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = orderInLayer;

            // Başlangıç opacity'si
            float startOpacity = (minOpacity + maxOpacity) / 2f;
            sr.color = new Color(1, 1, 1, startOpacity);
        }
        else
        {
            Destroy(hallucination);
            return;
        }

        // YANIP SÖNME VE BOYUT DEĞİŞİM EFEKTİNİ BAŞLAT
        StartCoroutine(PermanentPulseAndScaleEffect(hallucination));

        activeHallucinations.Add(hallucination);
    }

    // SÜREKLİ YANIP SÖNME VE BOYUT DEĞİŞİM EFEKTİ
    IEnumerator PermanentPulseAndScaleEffect(GameObject hallucination)
    {
        SpriteRenderer sr = hallucination.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        // Orijinal boyutu al
        if (!originalScales.ContainsKey(hallucination))
        {
            originalScales[hallucination] = hallucination.transform.localScale;
        }

        Vector3 originalScale = originalScales[hallucination];

        // Her görselin kendi rastgele hızı
        float pulseSpeed = Random.Range(pulseFrequency * 0.7f, pulseFrequency * 1.3f);
        float randomOffset = Random.Range(0f, Mathf.PI * 2f); // Farklı başlangıç fazı

        // Boyut için ayrı hız (isteğe bağlı)
        float scaleSpeed = pulseSpeed * Random.Range(0.8f, 1.2f);
        float scaleOffset = randomOffset + Random.Range(0f, 1f);

        while (hallucination != null && sr != null)
        {
            // TIME değeri
            float time = Time.time;

            // YANIP SÖNME HESAPLAMASI
            float pulse = Mathf.Sin((time * pulseSpeed) + randomOffset);
            pulse = (pulse + 1f) * 0.5f; // 0-1 aralığına getir
            float currentOpacity = Mathf.Lerp(minOpacity, maxOpacity, pulse);
            currentOpacity = Mathf.Lerp((minOpacity + maxOpacity) / 2f, currentOpacity, pulseIntensity);

            // BOYUT HESAPLAMASI
            float currentScaleMultiplier;

            if (synchronizeScaleWithOpacity)
            {
                // Opacity ile senkronize boyut değişimi
                // Opacity artarken büyüyecek, azalırken küçülecek
                currentScaleMultiplier = Mathf.Lerp(minScale, maxScale, pulse);
            }
            else
            {
                // Bağımsız boyut değişimi
                float scalePulse = Mathf.Sin((time * scaleSpeed) + scaleOffset);
                scalePulse = (scalePulse + 1f) * 0.5f; // 0-1 aralığına getir
                currentScaleMultiplier = Mathf.Lerp(minScale, maxScale, scalePulse);
            }

            // Scale intensity uygula
            float scaleEffect = Mathf.Lerp(1f, currentScaleMultiplier, scaleIntensity);

            // GÖRSELİ GÜNCELLE
            // 1. Opacity'yi güncelle
            Color color = sr.color;
            color.a = currentOpacity;
            sr.color = color;

            // 2. Boyutu güncelle
            hallucination.transform.localScale = originalScale * scaleEffect;

            yield return null;
        }
    }

    // TÜM GÖRSELLERİ TEMİZLE
    void ClearAllHallucinations()
    {
        foreach (var hallucination in activeHallucinations)
        {
            if (hallucination != null)
            {
                Destroy(hallucination);
            }
        }
        activeHallucinations.Clear();
        originalScales.Clear();
    }

    // SORTING LAYER KONTROLÜ
    bool SortingLayerExists(string layerName)
    {
        foreach (SortingLayer layer in SortingLayer.layers)
        {
            if (layer.name == layerName)
                return true;
        }
        return false;
    }

    // YENİDEN OLUŞTUR
    public void RespawnAll()
    {
        CreateAllHallucinations();
    }

    // AYARLARI DEĞİŞTİR
    public void UpdatePulseSettings(float newMinOpacity, float newMaxOpacity, float newPulseFreq)
    {
        minOpacity = newMinOpacity;
        maxOpacity = newMaxOpacity;
        pulseFrequency = newPulseFreq;
    }

    // BOYUT AYARLARINI DEĞİŞTİR
    public void UpdateScaleSettings(float newMinScale, float newMaxScale, float newScaleIntensity, bool synchronize)
    {
        minScale = newMinScale;
        maxScale = newMaxScale;
        scaleIntensity = newScaleIntensity;
        synchronizeScaleWithOpacity = synchronize;
    }

    void OnDestroy()
    {
        ClearAllHallucinations();
    }

    // DEBUG: Görsellerin durumunu göster
    
}