using UnityEngine;

public class RelicPickup : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private CameraFOVController fovController;
    [SerializeField] private VisionEffectController visionController;

    [Header("Pickup Settings")]
    [SerializeField] private float fovIncrease = 4f;
    [SerializeField] private float madnessReduce = 0.098f;

    [Header("Relic Identity")]
    [SerializeField] private int relicID = 0;

    [Header("Effects")]
    [SerializeField] private ParticleSystem pickupEffect;
    [SerializeField] private AudioClip pickupSound;

    private bool canInteract = true;

    public string GetInteractPrompt()
    {
        return "Examine";
    }

    public void OnInteract()
    {
        if (!canInteract) return;
        PickupRelic();
    }

    public bool CanInteract()
    {
        return canInteract;
    }

    void PickupRelic()
    {
        // 1. FOV ARTIR
        if (fovController != null)
            fovController.IncreaseFOV(fovIncrease);

        // 2. MADNESS AZALT
        if (visionController != null)
            visionController.ReduceMadness(madnessReduce);

        // 3. RELIC MANAGER'A HABER VER
        if (RelicManager.Instance != null)
        {
            RelicManager.Instance.RelicFound(relicID);
        }
        else
        {
            Debug.LogWarning("RelicManager bulunamadı!", gameObject);
        }

        // 4. EFEKTLER
        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, 0.7f);

        // 5. BAŞARI MESAJI
        PlayerInteraction playerInteraction = FindObjectOfType<PlayerInteraction>();
        if (playerInteraction != null)
        {
            playerInteraction.ShowSuccess();
        }

        // 6. YOK ET
        canInteract = false;
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (relicID >= 0)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
#endif
}