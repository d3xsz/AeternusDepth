using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Cthulhu"))
        {
            Debug.Log("💀 Cthulhu'ya çarptın!");

            PlayerSwimController swimController = GetComponent<PlayerSwimController>();
            if (swimController != null) swimController.ResetAllEffects();

            PlayerRespawn respawn = GetComponent<PlayerRespawn>();
            if (respawn != null)
            {
                respawn.StartGhostRespawn(transform.position);

                CthulhuChase cthulhu = other.GetComponent<CthulhuChase>();
                if (cthulhu != null) cthulhu.ResetCthulhuImmediately();

                ClearAnchors();
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerDied();
            }
        }
    }

    void ClearAnchors()
    {
        AnchorSpawner[] allSpawners = FindObjectsOfType<AnchorSpawner>();
        foreach (AnchorSpawner spawner in allSpawners)
            if (spawner != null) spawner.ClearAllAnchors();
    }
}