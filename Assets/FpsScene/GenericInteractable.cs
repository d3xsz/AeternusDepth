using UnityEngine;

public class GenericInteractable : MonoBehaviour, IInteractable
{
    [Header("Wrong Interaction")]
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private string wrongMessage = "It's just junk.";
    [SerializeField] private ParticleSystem failEffect;
    [SerializeField] private float cooldownTime = 0.5f;

    private bool canInteract = true;

    public string GetInteractPrompt()
    {
        return "Examine";
    }

    public void OnInteract()
    {
        if (!canInteract) return;

        ShowFailFeedback();
    }

    public bool CanInteract()
    {
        return canInteract;
    }

    void ShowFailFeedback()
    {
        if (wrongSound != null)
            AudioSource.PlayClipAtPoint(wrongSound, transform.position, 0.3f);

        if (failEffect != null)
            Instantiate(failEffect, transform.position, Quaternion.identity);

        PlayerInteraction playerInteraction = FindObjectOfType<PlayerInteraction>();
        if (playerInteraction != null)
        {
            playerInteraction.ShowWarning(wrongMessage);
        }

        StartCoroutine(ShakeEffect());
        StartCoroutine(InteractionCooldown());
    }

    System.Collections.IEnumerator ShakeEffect()
    {
        Vector3 originalPos = transform.position;
        float duration = 0.3f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            transform.position = originalPos + Random.insideUnitSphere * 0.03f;
            yield return null;
        }

        transform.position = originalPos;
    }

    System.Collections.IEnumerator InteractionCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(cooldownTime);
        canInteract = true;
    }
}