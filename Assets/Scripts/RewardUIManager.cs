using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RewardUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject rewardPanel;
    public Image leftIcon;
    public Image rightIcon;
    public TextMeshProUGUI leftText;
    public TextMeshProUGUI leftDescription;
    public TextMeshProUGUI rightText;
    public TextMeshProUGUI rightDescription;
    public Button leftButton;
    public Button rightButton;

    [Header("Reward Settings")]
    public List<PlayerStats.RewardItem> rewardPool = new List<PlayerStats.RewardItem>();

    [Header("Audio Settings")]
    public AudioClip rewardSelectSound; // Reward seçildiğinde çalacak ses
    public AudioClip rewardPanelOpenSound; // Panel açıldığında çalacak ses
    [Range(0f, 1f)] public float soundVolume = 1f;

    private AudioSource audioSource; // Ses çalmak için AudioSource
    private PlayerStats.RewardItem leftReward;
    private PlayerStats.RewardItem rightReward;
    private System.Action<PlayerStats.RewardItem> onRewardSelected;
    private bool isRewardPanelOpen = false;

    // ESC menu control
    private ESCMenu escMenu;
    private bool wasCursorVisible;
    private CursorLockMode previousCursorLockState;
    private float previousTimeScale;

    void Start()
    {
        if (rewardPanel == null) Debug.LogError("RewardPanel reference is not connected!");
        if (leftIcon == null) Debug.LogError("LeftIcon reference is not connected!");
        if (rightIcon == null) Debug.LogError("RightIcon reference is not connected!");
        if (leftText == null) Debug.LogError("LeftText reference is not connected!");
        if (rightText == null) Debug.LogError("RightText reference is not connected!");
        if (leftButton == null) Debug.LogError("LeftButton reference is not connected!");
        if (rightButton == null) Debug.LogError("RightButton reference is not connected!");

        // AudioSource component'i ekle veya bul
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = soundVolume;
        }

        escMenu = FindObjectOfType<ESCMenu>();
        if (escMenu == null) Debug.LogWarning("ESCMenu not found!");

        rewardPanel.SetActive(false);
        leftButton.onClick.AddListener(() => SelectReward(leftReward, true));
        rightButton.onClick.AddListener(() => SelectReward(rightReward, false));
    }

    void Update()
    {
        if (isRewardPanelOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC: Reward panel is open, ESC menu blocked");
            return;
        }
    }

    public void ShowRewardSelection(System.Action<PlayerStats.RewardItem> callback)
    {
        if (rewardPool.Count == 0)
        {
            Debug.LogError("RewardPool is empty! Add at least 1 reward.");
            return;
        }

        // PAUSE TIMER
        if (GameTimer.Instance != null)
            GameTimer.Instance.PauseTimer();

        if (escMenu != null && escMenu.isMenuOpen)
        {
            escMenu.ToggleMenu();
        }

        GetTwoRandomRewards();
        onRewardSelected = callback;
        UpdateRewardUI();

        wasCursorVisible = Cursor.visible;
        previousCursorLockState = Cursor.lockState;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        rewardPanel.SetActive(true);
        isRewardPanelOpen = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Panel açılma sesini çal
        if (rewardPanelOpenSound != null)
        {
            PlaySound(rewardPanelOpenSound);
        }

        Debug.Log("🎁 Reward panel opened - Time stopped");
    }

    void GetTwoRandomRewards()
    {
        if (rewardPool.Count < 2)
        {
            Debug.LogWarning("Only 1 reward available, same reward will be shown twice.");
            leftReward = rewardPool[0];
            rightReward = rewardPool[0];
            return;
        }

        int randomIndex1 = Random.Range(0, rewardPool.Count);
        leftReward = rewardPool[randomIndex1];

        int randomIndex2;
        do
        {
            randomIndex2 = Random.Range(0, rewardPool.Count);
        } while (randomIndex2 == randomIndex1 && rewardPool.Count > 1);

        rightReward = rewardPool[randomIndex2];
    }

    void UpdateRewardUI()
    {
        if (leftReward == null || rightReward == null)
        {
            Debug.LogError("Rewards are null! Left: " + leftReward + " Right: " + rightReward);
            return;
        }

        if (leftIcon != null) leftIcon.sprite = leftReward.icon;
        if (leftText != null) leftText.text = leftReward.rewardName;
        if (leftDescription != null) leftDescription.text = GetRewardDescription(leftReward);

        if (rightIcon != null) rightIcon.sprite = rightReward.icon;
        if (rightText != null) rightText.text = rightReward.rewardName;
        if (rightDescription != null) rightDescription.text = GetRewardDescription(rightReward);

        Debug.Log("UI updated: " + leftReward.rewardName + " - " + rightReward.rewardName);
    }

    string GetRewardDescription(PlayerStats.RewardItem reward)
    {
        if (reward == null) return "NULL REWARD";

        switch (reward.type)
        {
            case PlayerStats.RewardType.Health:
                return $"+{reward.value} Maximum Oxygen";
            case PlayerStats.RewardType.Mana:
                return $"+{reward.value} Maximum Mana";
            case PlayerStats.RewardType.Gold:
                return $"+{reward.value} Gold";
            case PlayerStats.RewardType.Experience:
                return $"+{reward.value} EXP";
            case PlayerStats.RewardType.MovementSpeed:
                return $"+%{reward.floatValue} Movement Speed";
            case PlayerStats.RewardType.AttackSpeed:
                return $"+%{reward.floatValue} Attack Speed";
            case PlayerStats.RewardType.BulletSpeed:
                return $"+%{reward.floatValue} Bullet Speed";
            case PlayerStats.RewardType.Damage:
                return $"+%{reward.floatValue} Damage";
            case PlayerStats.RewardType.SpecialItem:
                return reward.description;
            default:
                return reward.description;
        }
    }

    void SelectReward(PlayerStats.RewardItem selectedReward, bool isLeftButton)
    {
        if (selectedReward == null)
        {
            Debug.LogError("Selected reward is null!");
            return;
        }

        // Reward seçim sesini çal
        if (rewardSelectSound != null)
        {
            PlaySound(rewardSelectSound);
        }

        // İsteğe bağlı: Butona tıklama animasyonu veya feedback
        if (isLeftButton)
        {
            ButtonFeedback(leftButton);
        }
        else
        {
            ButtonFeedback(rightButton);
        }

        onRewardSelected?.Invoke(selectedReward);
        CloseRewardPanel();
        Debug.Log($"🎁 Reward selected: {selectedReward.rewardName}");
    }

    void CloseRewardPanel()
    {
        rewardPanel.SetActive(false);
        isRewardPanelOpen = false;

        // RESUME TIMER
        if (GameTimer.Instance != null)
            GameTimer.Instance.ResumeTimer();

        Time.timeScale = previousTimeScale;

        Cursor.visible = wasCursorVisible;
        Cursor.lockState = previousCursorLockState;

        Debug.Log("🎁 Reward panel closed - Time returned to normal");
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, soundVolume);
        }
    }

    void ButtonFeedback(Button button)
    {
        // İsteğe bağlı: Butona tıklandığında görsel feedback
        // Örneğin: butonun rengini değiştir veya küçük bir animasyon
        if (button != null)
        {
            // Basit bir scale animasyonu
            StartCoroutine(ButtonClickAnimation(button.transform));
        }
    }

    System.Collections.IEnumerator ButtonClickAnimation(Transform buttonTransform)
    {
        if (buttonTransform == null) yield break;

        Vector3 originalScale = buttonTransform.localScale;
        Vector3 targetScale = originalScale * 0.9f;

        buttonTransform.localScale = targetScale;
        yield return new WaitForSecondsRealtime(0.1f); // Realtime kullanıyoruz çünkü timeScale = 0
        buttonTransform.localScale = originalScale;
    }

    public void HideRewardPanel()
    {
        CloseRewardPanel();
    }

    public bool IsRewardPanelOpen()
    {
        return isRewardPanelOpen;
    }

    // Inspector'dan sesleri test etmek için
    [ContextMenu("Test Reward Select Sound")]
    public void TestRewardSelectSound()
    {
        if (rewardSelectSound != null)
        {
            PlaySound(rewardSelectSound);
        }
    }

    [ContextMenu("Test Panel Open Sound")]
    public void TestPanelOpenSound()
    {
        if (rewardPanelOpenSound != null)
        {
            PlaySound(rewardPanelOpenSound);
        }
    }
}