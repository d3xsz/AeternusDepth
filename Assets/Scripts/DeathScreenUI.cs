using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DeathScreenUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject deathPanel;
    public TextMeshProUGUI deathReasonText;
    public TextMeshProUGUI statsSummaryText;
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Death Messages")]
    public string defaultDeathMessage = "OXYGEN DEPLETED";
    public string enemyDeathMessage = "ELIMINATED BY ENEMY";

    private PlayerHealth playerHealth;

    void Start()
    {
        // Hide death panel at start
        if (deathPanel != null)
            deathPanel.SetActive(false);

        // Find PlayerHealth
        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            // Subscribe to death event
            playerHealth.OnDeath.AddListener(OnPlayerDeath);
        }

        // MANUALLY assign button events
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        Debug.Log("💀 DeathScreenUI initialized - Button events assigned");
    }

    void OnPlayerDeath()
    {
        ShowDeathScreen();
    }

    public void ShowDeathScreen(string deathReason = "")
    {
        Debug.Log("💀 Showing death screen");

        // Pause time
        Time.timeScale = 0f;

        // Show cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Show death panel
        if (deathPanel != null)
            deathPanel.SetActive(true);

        // Set death reason
        if (deathReasonText != null)
        {
            string message = GetDeathMessage(deathReason);
            deathReasonText.text = message;
        }

        // Show statistics
        if (statsSummaryText != null)
        {
            statsSummaryText.text = GetStatsSummary();
        }

        // Close ESC menu (if open)
        ESCMenu escMenu = FindObjectOfType<ESCMenu>();
        if (escMenu != null && escMenu.isMenuOpen)
        {
            escMenu.ToggleMenu();
        }
    }

    string GetDeathMessage(string reason)
    {
        switch (reason.ToLower())
        {
            case "enemy":
                return enemyDeathMessage;
            default:
                return defaultDeathMessage;
        }
    }

    string GetStatsSummary()
    {
        string summary = "YOUR FINAL STATS\n\n";

        if (PlayerStats.Instance != null)
        {
            // Remove "TOTAL STATISTICS" header, just get the stats
            string stats = PlayerStats.Instance.GetTotalStatsSummary();
            // Remove "TOTAL STATISTICS" text
            stats = stats.Replace("TOTAL STATISTICS\n\n", "");
            summary += stats;
        }
        else
        {
            summary += "Failed to load statistics";
        }

        // Player health information
        if (playerHealth != null)
        {
            summary += $"\n\n💧 Final Oxygen: {playerHealth.currentHealth}/{playerHealth.maxHealth}";
        }

        return summary;
    }

    public void RestartGame()
    {
        Debug.Log("🔁 Restarting game...");

        // Restore normal time
        Time.timeScale = 1f;

        // Reload current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void GoToMainMenu()
    {
        Debug.Log("🏠 Returning to main menu...");

        // Restore normal time
        Time.timeScale = 1f;

        // Go to main menu
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("🔴 Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void OnDestroy()
    {
        // Clean up event connections
        if (playerHealth != null)
        {
            playerHealth.OnDeath.RemoveListener(OnPlayerDeath);
        }

        // Clean up button events
        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(GoToMainMenu);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(QuitGame);
    }
}