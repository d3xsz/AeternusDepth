using UnityEngine;
using System.Collections.Generic;

public class AnchorSpawner : MonoBehaviour
{
    public GameObject anchorPrefab;
    public float spawnInterval = 2f;
    public int maxAnchors = 5;

    [Header("Spawn Bölgesi")]
    public float spawnHeight = 10f;
    public float spawnWidth = 5f;

    private List<GameObject> activeAnchors = new List<GameObject>();
    private Transform player;
    private float spawnTimer = 0f;
    private bool isActive = true;
    private PlayerRespawn playerRespawn;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRespawn = playerObj.GetComponent<PlayerRespawn>();
        }
    }

    void Update()
    {
        if (!isActive || anchorPrefab == null) return;

        if (!IsPlayerAlive())
        {
            spawnTimer = 0f;
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            SpawnAnchor();
            spawnTimer = 0f;
        }

        CleanupAnchors();
    }

    bool IsPlayerAlive()
    {
        if (player == null) return false;
        if (playerRespawn != null) return playerRespawn.HasPlayerRespawned();
        return player.gameObject.activeInHierarchy;
    }

    void SpawnAnchor()
    {
        if (activeAnchors.Count >= maxAnchors) return;
        if (player == null) return;
        if (!IsPlayerAlive()) return;

        float randomX = Random.Range(-spawnWidth, spawnWidth);
        Vector3 spawnPos = new Vector3(
            player.position.x + randomX,
            player.position.y + spawnHeight,
            0
        );

        GameObject newAnchor = Instantiate(anchorPrefab, spawnPos, Quaternion.identity);
        activeAnchors.Add(newAnchor);
    }

    void CleanupAnchors()
    {
        for (int i = activeAnchors.Count - 1; i >= 0; i--)
        {
            if (activeAnchors[i] == null)
                activeAnchors.RemoveAt(i);
        }
    }

    public void ClearAllAnchors()
    {
        foreach (GameObject anchor in activeAnchors)
        {
            if (anchor != null)
                Destroy(anchor);
        }
        activeAnchors.Clear();
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (!active) ClearAllAnchors();
    }

    public void StopSpawning() => SetActive(false);
    public void StartSpawning() => SetActive(true);
}