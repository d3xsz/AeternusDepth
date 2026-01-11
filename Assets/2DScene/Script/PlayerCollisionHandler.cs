using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // Sadece Anchor veya diğer nesneler için çarpışma
        // Cthulhu artık yok

        if (other.CompareTag("Anchor"))
        {
            // Anchor çarpışması
            Debug.Log("⚓ Anchor'a çarptın!");

            // İstersen burada bir şey yapabilirsin
        }
    }

    void ClearAnchors()
    {
        AnchorSpawner[] allSpawners = FindObjectsOfType<AnchorSpawner>();
        foreach (AnchorSpawner spawner in allSpawners)
            if (spawner != null) spawner.ClearAllAnchors();
    }
}