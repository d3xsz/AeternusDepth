using UnityEngine;
using UnityEngine.UI;

public class TeleportDoor : MonoBehaviour, IInteractable
{
    [Header("Teleport Settings")]
    [SerializeField] private Transform teleportDestination; // Iþýnlanma noktasý
    [SerializeField] private string interactPrompt = "Open Door";
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private ParticleSystem teleportEffect;

    [Header("Fade Settings")]
    [SerializeField] private bool useFadeEffect = true;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Color fadeColor = Color.black;

    [Header("Player Rotation")]
    [SerializeField] private bool matchDoorRotation = true; // Kapýnýn yönüne dönsün mü?

    private bool canInteract = true;
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public string GetInteractPrompt()
    {
        return interactPrompt;
    }

    public void OnInteract()
    {
        if (!canInteract || player == null || teleportDestination == null) return;

        StartTeleport();
    }

    public bool CanInteract()
    {
        return canInteract;
    }

    void StartTeleport()
    {
        canInteract = false;

        // Ses efekti
        if (teleportSound != null)
        {
            AudioSource.PlayClipAtPoint(teleportSound, transform.position, 0.7f);
        }

        // Partikül efekti
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, player.transform.position, Quaternion.identity);
        }

        // Fade efekti kullanýlýyorsa
        if (useFadeEffect)
        {
            StartCoroutine(TeleportWithFade());
        }
        else
        {
            // Direkt ýþýnlanma
            TeleportPlayer();
            canInteract = true;
        }
    }

    System.Collections.IEnumerator TeleportWithFade()
    {
        // Fade-out (ekran kararsýn)
        yield return StartCoroutine(FadeScreen(0f, 1f, fadeDuration / 2f));

        // Oyuncuyu ýþýnla
        TeleportPlayer();

        // Fade-in (ekran aydýnlansýn)
        yield return StartCoroutine(FadeScreen(1f, 0f, fadeDuration / 2f));

        canInteract = true;
    }

    void TeleportPlayer()
    {
        // Oyuncuyu ýþýnla
        player.transform.position = teleportDestination.position;

        // Rotasyonu ayarla
        if (matchDoorRotation && teleportDestination != null)
        {
            player.transform.rotation = teleportDestination.rotation;

            // Eðer First Person Controller kullanýyorsan, camera rotation'ý da ayarla
            SetCameraRotation(teleportDestination.rotation);
        }

        // Hedef noktada da efekt oluþtur
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, teleportDestination.position, Quaternion.identity);
        }
    }

    void SetCameraRotation(Quaternion rotation)
    {
        // First Person Controller için camera rotation ayarla
        MonoBehaviour[] playerScripts = player.GetComponents<MonoBehaviour>();
        foreach (var script in playerScripts)
        {
            string scriptName = script.GetType().Name;

            // Eðer FirstPersonController veya benzeri script varsa
            if (scriptName.Contains("FirstPersonController") ||
                scriptName.Contains("PlayerLook"))
            {
                // Y eksenini (yaw) kapýnýn yönüne çevir
                Vector3 eulerRotation = rotation.eulerAngles;
                script.GetType().GetField("yaw")?.SetValue(script, eulerRotation.y);
                script.GetType().GetField("pitch")?.SetValue(script, 0f);

                // Camera'ý güncelle
                Transform cameraTransform = Camera.main?.transform;
                if (cameraTransform != null)
                {
                    cameraTransform.localEulerAngles = new Vector3(0f, eulerRotation.y, 0f);
                }
            }
        }
    }

    System.Collections.IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        // Ekran karartma için Canvas'a Fade Image eklememiz gerek
        // Önce var mý kontrol et, yoksa oluþtur

        GameObject fadeObject = GameObject.Find("FadeOverlay");
        if (fadeObject == null)
        {
            fadeObject = CreateFadeOverlay();
        }

        Image fadeImage = fadeObject.GetComponent<Image>();
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        Color color = fadeColor;
        color.a = startAlpha;
        fadeImage.color = color;

        fadeObject.SetActive(true);

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            color.a = alpha;
            fadeImage.color = color;

            elapsed += Time.deltaTime;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;

        // Fade-in bittiyse gizle
        if (endAlpha <= 0f)
        {
            fadeObject.SetActive(false);
        }
    }

    GameObject CreateFadeOverlay()
    {
        // Canvas'ý bul veya oluþtur
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // En üstte
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Fade Image oluþtur
        GameObject fadeObject = new GameObject("FadeOverlay");
        fadeObject.transform.SetParent(canvas.transform);

        RectTransform rt = fadeObject.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image image = fadeObject.AddComponent<Image>();
        image.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        fadeObject.SetActive(false);

        return fadeObject;
    }

    void OnDrawGizmos()
    {
        if (teleportDestination != null)
        {
            // Kapýdan hedefe çizgi çiz
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, teleportDestination.position);

            // Hedef noktayý göster
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(teleportDestination.position, 0.5f);
            Gizmos.DrawRay(teleportDestination.position, teleportDestination.forward * 1f);
        }
    }
}