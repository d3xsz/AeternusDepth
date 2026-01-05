using UnityEngine;
using System.Collections.Generic;

public class PotManager : MonoBehaviour
{
    public static PotManager Instance;

    [Header("Pot Ayarları")]
    public GameObject potPrefab;
    public Transform[] spawnPositions;

    [Header("Dağıtım Oranı")]
    [Range(0, 100)] public int poisonPotChance = 30;

    [Header("Debug")]
    public bool showDebug = true;

    private List<GameObject> allPots = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        SpawnAllPots();
    }

    void SpawnAllPots()
    {
        ClearAllPots();

        if (spawnPositions == null || spawnPositions.Length == 0)
        {
            Debug.LogError("Spawn pozisyonları atanmamış!");
            return;
        }

        if (potPrefab == null)
        {
            Debug.LogError("Pot prefab'ı atanmamış!");
            return;
        }

        foreach (Transform spawnPos in spawnPositions)
        {
            if (spawnPos != null)
            {
                GameObject pot = Instantiate(potPrefab, spawnPos.position, Quaternion.identity);
                pot.transform.parent = transform;

                PotItem potScript = pot.GetComponent<PotItem>();
                if (potScript != null)
                {
                    bool isPoison = Random.Range(0, 100) < poisonPotChance;
                    potScript.ChangePotType(isPoison);

                    if (showDebug)
                    {
                        string potType = isPoison ? "🔴 ZEHİR" : "🟢 HIZ";
                        Debug.Log($"{potType} pot oluşturuldu: {spawnPos.position}");
                    }
                }

                allPots.Add(pot);
            }
        }

        Debug.Log($"✅ Toplam {allPots.Count} pot oluşturuldu");
    }

    public void OnPotCollected(GameObject pot)
    {
        if (pot != null && allPots.Contains(pot))
        {
            allPots.Remove(pot);
            Destroy(pot);
        }
    }

    public void RespawnAllPots()
    {
        Debug.Log("🔄 PotManager: Tüm potlar yeniden oluşturuluyor");
        SpawnAllPots();
    }

    void ClearAllPots()
    {
        foreach (GameObject pot in allPots)
        {
            if (pot != null)
            {
                Destroy(pot);
            }
        }
        allPots.Clear();
    }
}