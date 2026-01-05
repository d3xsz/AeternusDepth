using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Respawn Ayarları")]
    public Vector3 spawnPosition = new Vector3(0, 2, 0);

    [Header("Hayalet Efekt Ayarları")]
    public float ghostSpeed = 3f;
    public Color ghostColor = new Color(0.5f, 0.8f, 1f, 0.6f);
    public ParticleSystem ghostParticles;
    public AudioClip ghostSound;
    public AudioClip respawnSound;

    [Header("Ekran Efekti")]
    public Image screenOverlay;
    public Color screenGhostColor = new Color(0.3f, 0.5f, 0.8f, 0.4f);
    public float screenFadeInTime = 0.5f;
    public float screenFadeOutTime = 0.8f;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerSwimController swimController;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private Canvas canvas;
    private bool hasRespawned = true;
    private bool isRespawning = false;

    // Event'ler için delegate'ler
    public System.Action OnRespawnComplete;
    public System.Action OnDeath;
    public System.Action OnRespawnStart;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        swimController = GetComponent<PlayerSwimController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();

        CreateScreenOverlay();
        hasRespawned = true;
        isRespawning = false;
    }

    void CreateScreenOverlay()
    {
        if (screenOverlay == null)
        {
            GameObject canvasObj = new GameObject("GhostCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            GameObject imageObj = new GameObject("GhostOverlay");
            imageObj.transform.SetParent(canvas.transform);

            screenOverlay = imageObj.AddComponent<Image>();
            screenOverlay.color = Color.clear;

            RectTransform rt = screenOverlay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }

    public void StartGhostRespawn(Vector3 deathPosition)
    {
        if (isRespawning) return;

        // Respawn başladı event'ini tetikle
        Debug.Log("📢 Respawn başladı event'i tetikleniyor...");
        OnRespawnStart?.Invoke();

        // Ölüm event'ini tetikle
        Debug.Log("📢 Ölüm event'i tetikleniyor...");
        OnDeath?.Invoke();

        StartCoroutine(GhostRespawnRoutine(deathPosition));
    }

    IEnumerator GhostRespawnRoutine(Vector3 deathPosition)
    {
        Debug.Log("👻 HAYALET FORMA GEÇİLİYOR...");
        isRespawning = true;
        hasRespawned = false;

        yield return StartCoroutine(FadeScreenEffect(true, screenFadeInTime));

        transform.position = deathPosition;
        SetGhostMode(true);

        if (ghostSound != null)
            AudioSource.PlayClipAtPoint(ghostSound, transform.position);

        Vector3 startPos = deathPosition;
        Vector3 endPos = spawnPosition;
        float journeyLength = Vector3.Distance(startPos, endPos);
        float startTime = Time.time;

        while (Vector3.Distance(transform.position, endPos) > 0.1f)
        {
            float distCovered = (Time.time - startTime) * ghostSpeed;
            float t = distCovered / journeyLength;

            Vector3 curvedPos = CalculateGhostPath(startPos, endPos, t, 2f);
            transform.position = curvedPos;
            transform.position += (Vector3)Random.insideUnitCircle * 0.03f;

            yield return null;
        }

        SetGhostMode(false);
        yield return StartCoroutine(FadeScreenEffect(false, screenFadeOutTime));
        CompleteRespawn();

        if (respawnSound != null)
            AudioSource.PlayClipAtPoint(respawnSound, transform.position, 1f);

        NotifyCthulhuPlayerReady();
        hasRespawned = true;
        isRespawning = false;

        // Respawn tamamlandı event'ini tetikle
        Debug.Log("✅ HAYALET RESPAWN TAMAMLANDI! Event tetikleniyor...");
        OnRespawnComplete?.Invoke();
    }

    IEnumerator FadeScreenEffect(bool fadeIn, float duration)
    {
        if (screenOverlay == null) yield break;

        Color startColor = fadeIn ? Color.clear : screenGhostColor;
        Color endColor = fadeIn ? screenGhostColor : Color.clear;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            screenOverlay.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        screenOverlay.color = endColor;
    }

    Vector3 CalculateGhostPath(Vector3 start, Vector3 end, float t, float curveHeight)
    {
        Vector3 controlPoint = (start + end) / 2;
        controlPoint.y += curveHeight;
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        return (uu * start) + (2 * u * t * controlPoint) + (tt * end);
    }

    void SetGhostMode(bool ghost)
    {
        if (playerCollider != null) playerCollider.enabled = !ghost;
        if (spriteRenderer != null) spriteRenderer.color = ghost ? ghostColor : Color.white;
        if (ghostParticles != null)
        {
            if (ghost) ghostParticles.Play();
            else ghostParticles.Stop();
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = !ghost;
        }
        if (swimController != null)
        {
            if (ghost) swimController.StopSwimming();
            swimController.enabled = !ghost;
        }
        if (animator != null) animator.enabled = !ghost;

        if (ghost) StopAnchorSpawning();
        else StartAnchorSpawning();
    }

    void CompleteRespawn()
    {
        transform.position = spawnPosition;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsSwimming", true);
            animator.Play("Idle", 0, 0f);
        }
        if (swimController != null) swimController.StartSwimming();
    }

    void NotifyCthulhuPlayerReady()
    {
        CthulhuChase[] allCthulhus = FindObjectsOfType<CthulhuChase>();
        foreach (CthulhuChase cthulhu in allCthulhus)
        {
            if (cthulhu != null)
                cthulhu.OnPlayerRespawnComplete();
        }
    }

    void StartAnchorSpawning()
    {
        AnchorSpawner[] allSpawners = FindObjectsOfType<AnchorSpawner>();
        foreach (AnchorSpawner spawner in allSpawners)
            if (spawner != null) spawner.StartSpawning();
    }

    void StopAnchorSpawning()
    {
        AnchorSpawner[] allSpawners = FindObjectsOfType<AnchorSpawner>();
        foreach (AnchorSpawner spawner in allSpawners)
            if (spawner != null) spawner.StopSpawning();
    }

    public bool HasPlayerRespawned() => hasRespawned;

    public void RespawnPlayer()
    {
        Debug.Log("Direct RespawnPlayer çağrıldı");

        // Respawn başladı event'ini tetikle
        OnRespawnStart?.Invoke();

        if (swimController != null) swimController.ResetAllEffects();
        transform.position = spawnPosition;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsSwimming", true);
            animator.Play("Idle", 0, 0f);
        }

        if (swimController != null) swimController.StartSwimming();

        hasRespawned = true;
        isRespawning = false;

        NotifyCthulhuPlayerReady();
        StartAnchorSpawning();

        // Event'i tetikle
        OnRespawnComplete?.Invoke();
    }

    public void HandleDeath() => StartGhostRespawn(transform.position);

    void OnDestroy()
    {
        if (canvas != null) Destroy(canvas.gameObject);
    }
}