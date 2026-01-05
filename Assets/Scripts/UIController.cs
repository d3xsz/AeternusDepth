using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HoverEnergyUI : MonoBehaviour
{
    [Header("Hover Bar")]
    public Image hoverFillImage;  // TEK GEREKLİ REFERANS
    public ybotController playerController;

    [Header("Colors")]
    public Color fullColor = Color.green;
    public Color mediumColor = Color.yellow;
    public Color lowColor = Color.red;

    private CanvasGroup canvasGroup;
    private float currentFill = 1f;

    void Start()
    {
        // CanvasGroup ekle
        if (TryGetComponent<CanvasGroup>(out canvasGroup) == false)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // PlayerController'ı bul
        FindPlayer();
    }

    void Update()
    {
        if (playerController == null) return;
        if (hoverFillImage == null) return;

        // Enerji yüzdesini al
        float energyPercent = playerController.GetHoverEnergyPercentage();

        // Fill miktarını güncelle (smooth)
        currentFill = Mathf.Lerp(currentFill, energyPercent, Time.deltaTime * 5f);
        hoverFillImage.fillAmount = currentFill;

        // Renk güncelle
        UpdateColor(energyPercent);

        // Görünürlük kontrolü
        UpdateVisibility();
    }

    void UpdateColor(float percent)
    {
        if (percent > 0.5f)
            hoverFillImage.color = fullColor;
        else if (percent > 0.2f)
            hoverFillImage.color = mediumColor;
        else
            hoverFillImage.color = lowColor;

        // Hover modunda değilken şeffaflık
        float alpha = playerController.IsHovering() ? 1f : 0.7f;
        hoverFillImage.color = new Color(
            hoverFillImage.color.r,
            hoverFillImage.color.g,
            hoverFillImage.color.b,
            alpha
        );
    }

    void UpdateVisibility()
    {
        if (canvasGroup == null) return;

        bool shouldHide = false;

        // ESC menü
        ESCMenu escMenu = FindObjectOfType<ESCMenu>();
        if (escMenu != null && escMenu.isMenuOpen)
            shouldHide = true;

        // Diyalog
        SeamanDialogue dialogue = FindObjectOfType<SeamanDialogue>();
        if (dialogue != null && dialogue.isDialogueActive)
            shouldHide = true;

        // Ölüm ekranı
        DeathScreenUI deathScreen = FindObjectOfType<DeathScreenUI>();
        if (deathScreen != null && deathScreen.deathPanel != null && deathScreen.deathPanel.activeInHierarchy)
            shouldHide = true;

        // Puzzle
        Door6PuzzleController puzzle = FindObjectOfType<Door6PuzzleController>();
        if (puzzle != null && puzzle.IsPuzzleUIOpen)
            shouldHide = true;

        // Görünürlüğü ayarla
        canvasGroup.alpha = shouldHide ? 0f : 1f;
    }

    void FindPlayer()
    {
        playerController = FindObjectOfType<ybotController>();
        if (playerController == null)
        {
            Invoke("FindPlayer", 1f);
        }
    }

    // Scene değişikliği
    private static HoverEnergyUI instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayer();
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }
}