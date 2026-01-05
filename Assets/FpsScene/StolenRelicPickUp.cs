using UnityEngine;
using System.Collections;

public class StolenRelicPickup : MonoBehaviour, IInteractable
{
    [Header("Relic Identity")]
    public int relicID = -1;

    [Header("Settings")]
    public float fovIncrease = 4f;
    public float madnessReduce = 0.098f;

    [Header("Effects")]
    public ParticleSystem pickupEffect;
    public AudioClip pickupSound;

    private bool canInteract = true;

    void Start()
    {
        if (relicID == -1)
        {
            Debug.LogWarning("StolenRelic ID yok! Ýsimden tahmin...");
            if (gameObject.name.Contains("0")) relicID = 0;
            else if (gameObject.name.Contains("1")) relicID = 1;
            else if (gameObject.name.Contains("2")) relicID = 2;
            else if (gameObject.name.Contains("3")) relicID = 3;
            else if (gameObject.name.Contains("4")) relicID = 4;
        }

        Debug.Log($"StolenRelic ID: {relicID} hazýr!");
    }

    public string GetInteractPrompt()
    {
        return "Take Back Stolen Relic";
    }

    public void OnInteract()
    {
        if (!canInteract || relicID == -1) return;
        RecoverRelic();
    }

    public bool CanInteract()
    {
        return canInteract;
    }

    void RecoverRelic()
    {
        CameraFOVController fovController = FindObjectOfType<CameraFOVController>();
        if (fovController != null)
        {
            fovController.IncreaseFOV(fovIncrease);
            Debug.Log($"FOV +{fovIncrease} arttý");
        }

        VisionEffectController visionController = FindObjectOfType<VisionEffectController>();
        if (visionController != null)
        {
            visionController.ReduceMadness(madnessReduce);
            Debug.Log($"Madness -{madnessReduce} azaldý");
        }

        if (RelicManager.Instance != null)
        {
            RelicManager.Instance.RelicRecovered(relicID);
            Debug.Log($"Relic {relicID} geri alýndý!");
        }

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, 0.7f);

        PlayerInteraction playerInteraction = FindObjectOfType<PlayerInteraction>();
        if (playerInteraction != null)
        {
            playerInteraction.ShowMessage($"Relic {relicID} Recovered!");
        }

        canInteract = false;
        Destroy(gameObject, 0.1f);
    }

    void OnDrawGizmos()
    {
        if (relicID >= 0)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}