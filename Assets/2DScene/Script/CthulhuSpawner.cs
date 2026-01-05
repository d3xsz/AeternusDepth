using UnityEngine;

public class CthulhuSpawner : MonoBehaviour
{
    [Header("Cthulhu Prefab")]
    public GameObject cthulhuPrefab;

    [Header("Spawn Ayarlarý")]
    public float spawnDelay = 2f;
    public Vector3 spawnOffset = new Vector3(0, -5f, 0);

    private GameObject currentCthulhu;
    private bool hasSpawned = false;  // YENÝ: Sadece bir kez spawn et

    void Start()
    {
        // Invoke yerine Coroutine kullanalým
        StartCoroutine(SpawnCthulhuWithDelay());
    }

    System.Collections.IEnumerator SpawnCthulhuWithDelay()
    {
        yield return new WaitForSeconds(spawnDelay);

        // Eðer zaten spawn ettiysek tekrar etme
        if (hasSpawned || currentCthulhu != null)
        {
            Debug.Log("Cthulhu zaten spawn oldu!");
            yield break;
        }

        SpawnCthulhu();
    }

    void SpawnCthulhu()
    {
        if (cthulhuPrefab == null)
        {
            Debug.LogError("Cthulhu prefabý atanmamýþ!");
            return;
        }

        // Sahnedeki mevcut Cthulhu'larý kontrol et
        GameObject[] existingCthulhus = GameObject.FindGameObjectsWithTag("Cthulhu");
        if (existingCthulhus.Length > 0)
        {
            Debug.Log("Zaten " + existingCthulhus.Length + " tane Cthulhu var!");
            return;
        }

        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player bulunamadý!");
            return;
        }

        // Kameranýn tam altýnda, ortada spawn et
        Camera mainCamera = Camera.main;
        Vector3 spawnPosition;

        if (mainCamera != null)
        {
            // Ekranýn altýnda, ortada (Viewport: 0.5, 0.1)
            spawnPosition = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.1f, 0));
            spawnPosition.z = 0;
        }
        else
        {
            // Kamera yoksa player'ýn altýnda
            spawnPosition = player.position + spawnOffset;
        }

        currentCthulhu = Instantiate(cthulhuPrefab, spawnPosition, Quaternion.identity);
        currentCthulhu.tag = "Cthulhu";  // Tag'ý kesinlikle ata
        hasSpawned = true;

        Debug.Log("Cthulhu spawn oldu! Pozisyon: " + spawnPosition);
    }

    // Debug için
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && currentCthulhu != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentCthulhu.transform.position, 1f);
        }
    }
}