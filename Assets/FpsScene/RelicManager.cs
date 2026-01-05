using UnityEngine;
using System.Collections.Generic;

public class RelicManager : MonoBehaviour
{
    public static RelicManager Instance;

    [Header("Relic Settings")]
    [SerializeField] private int totalRelics = 5;

    [Header("References")]
    [SerializeField] private MermaidNPC mermaidNPC;
    [SerializeField] private UIManager uiManager;

    private List<bool> foundRelics = new List<bool>();
    private int relicsFound = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Listeyi başlat
        for (int i = 0; i < totalRelics; i++)
        {
            foundRelics.Add(false);
        }

        Debug.Log($"RelicManager başlatıldı. Toplam {totalRelics} relic.");
    }

    // Relic bulunduğunda
    public void RelicFound(int relicID = -1)
    {
        if (relicID == -1)
        {
            // Otomatik index ata
            for (int i = 0; i < foundRelics.Count; i++)
            {
                if (!foundRelics[i])
                {
                    relicID = i;
                    break;
                }
            }
        }

        // Geçerli bir ID ve henüz bulunmamışsa
        if (relicID >= 0 && relicID < foundRelics.Count && !foundRelics[relicID])
        {
            foundRelics[relicID] = true;
            relicsFound++;

            Debug.Log($"🎯 Relic {relicID} bulundu! Toplam: {relicsFound}/{totalRelics}");

            // Mermaid NPC'yi güncelle
            if (mermaidNPC != null)
            {
                mermaidNPC.OnRelicFound(relicID);
            }

            // UI Manager'ı güncelle
            if (uiManager != null)
            {
                uiManager.OnRelicCollected(relicID);
            }

            // Tüm relic'ler bulundu mu kontrol et
            if (relicsFound >= totalRelics)
            {
                AllRelicsFound();
            }
        }
        else
        {
            Debug.LogWarning($"Geçersiz veya zaten bulunmuş relic ID: {relicID}");
        }
    }

    // YENİ: Relic çalındığında
    public void RelicStolen(int relicID)
    {
        if (relicID >= 0 && relicID < foundRelics.Count && foundRelics[relicID])
        {
            foundRelics[relicID] = false;
            relicsFound--;

            Debug.Log($"⚠️ Relic {relicID} çalındı! Kalan: {relicsFound}/{totalRelics}");

            // UI'ı güncelle
            if (uiManager != null)
            {
                uiManager.OnRelicStolen(relicID);
            }
        }
    }

    // YENİ: Çalınan relic geri alındığında
    public void RelicRecovered(int relicID)
    {
        if (relicID >= 0 && relicID < foundRelics.Count && !foundRelics[relicID])
        {
            foundRelics[relicID] = true;
            relicsFound++;

            Debug.Log($"✅ Çalınan relic {relicID} geri alındı! Toplam: {relicsFound}/{totalRelics}");

            // UI'ı güncelle
            if (uiManager != null)
            {
                uiManager.OnRelicCollected(relicID);
            }
        }
    }

    // Belirli bir relic'in bulunup bulunmadığını kontrol et
    public bool IsRelicFound(int index)
    {
        if (index >= 0 && index < foundRelics.Count)
            return foundRelics[index];

        Debug.LogWarning($"Geçersiz relic index: {index}");
        return false;
    }

    // Tüm relic'ler bulunduğunda
    void AllRelicsFound()
    {
        Debug.Log("🎉🎉🎉 TÜM RELIC'LER BULUNDU! 🎉🎉🎉");
        Debug.Log("Madness etkileri tamamen temizlendi!");
    }

    // Bulunan relic sayısını döndür
    public int GetFoundRelicCount()
    {
        return relicsFound;
    }

    // Toplam relic sayısını döndür
    public int GetTotalRelicCount()
    {
        return totalRelics;
    }

    // Relic ID'lerini debug için göster
    [ContextMenu("Debug: Show Relic Status")]
    void DebugRelicStatus()
    {
        string status = "Relic Durumu:\n";
        for (int i = 0; i < foundRelics.Count; i++)
        {
            status += $"Relic {i}: {(foundRelics[i] ? "✅ BULUNDU" : "❌ BULUNAMADI")}\n";
        }
        status += $"Toplam: {relicsFound}/{totalRelics}";
        Debug.Log(status);
    }

    [ContextMenu("Debug: Reset All Relics")]
    void DebugResetAllRelics()
    {
        for (int i = 0; i < foundRelics.Count; i++)
        {
            foundRelics[i] = false;
        }
        relicsFound = 0;

        // UI'yı güncelle
        if (uiManager != null)
        {
            uiManager.OnRelicCollected(-1);
        }
        Debug.Log("Tüm relic'ler resetlendi.");
    }

    [ContextMenu("Debug: Find All Relics")]
    void DebugFindAllRelics()
    {
        for (int i = 0; i < foundRelics.Count; i++)
        {
            if (!foundRelics[i])
            {
                RelicFound(i);
            }
        }
    }
}