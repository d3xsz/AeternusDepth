using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [System.Serializable]
    public class InteractableSettings
    {
        public string objectNameContains = "";
        public string customPrompt = "";
        public float promptSize = 22f;
        public Color promptColor = Color.white;
        public float keySize = 18f;
    }

    [Header("UI References")]
    [SerializeField] private GameObject interactPanel;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private TextMeshProUGUI successText;

    [Header("Settings")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float successDisplayTime = 2f;
    [SerializeField] private string successMessage = "Relic Acquired!";

    [Header("Default Text Sizes")]
    [SerializeField] private float defaultPromptSize = 22f;
    [SerializeField] private float defaultKeySize = 18f;
    [SerializeField] private Color defaultPromptColor = Color.white;

    [Header("CUSTOM INTERACTABLE SETTINGS - BURADAN AYARLA")]
    [SerializeField] private List<InteractableSettings> customSettings = new List<InteractableSettings>();

    [Header("Preset Examples (Silip kendin ekleyebilirsin)")]
    [SerializeField]
    private InteractableSettings mermaidSettings = new InteractableSettings()
    {
        objectNameContains = "Mermaid",
        customPrompt = "<size=32>TALK TO MERMAID</size>",
        promptSize = 32f,
        promptColor = new Color(1f, 0.84f, 0f, 1f),
        keySize = 22f
    };

    [SerializeField]
    private InteractableSettings doorSettings = new InteractableSettings()
    {
        objectNameContains = "Door",
        customPrompt = "OPEN DOOR",
        promptSize = 26f,
        promptColor = Color.cyan,
        keySize = 20f
    };

    [SerializeField]
    private InteractableSettings relicSettings = new InteractableSettings()
    {
        objectNameContains = "Relic",
        customPrompt = "Examine",
        promptSize = 24f,
        promptColor = Color.white,
        keySize = 18f
    };

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    private IInteractable currentInteractable;
    private float warningTimer;
    private float successTimer;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        HideInteractUI();

        if (successText != null)
            successText.gameObject.SetActive(false);

        if (!customSettings.Contains(mermaidSettings))
            customSettings.Add(mermaidSettings);
        if (!customSettings.Contains(doorSettings))
            customSettings.Add(doorSettings);
        if (!customSettings.Contains(relicSettings))
            customSettings.Add(relicSettings);
    }

    void Update()
    {
        FindInteractable();

        if (Input.GetKeyDown(interactKey) && currentInteractable != null)
        {
            currentInteractable.OnInteract();
        }

        if (warningTimer > 0)
        {
            warningTimer -= Time.deltaTime;
            if (warningTimer <= 0)
            {
                warningText.gameObject.SetActive(false);
            }
        }

        if (successTimer > 0)
        {
            successTimer -= Time.deltaTime;
            if (successTimer <= 0)
            {
                successText.gameObject.SetActive(false);
            }
        }
    }

    void FindInteractable()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null && interactable.CanInteract())
            {
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;

                    string prompt = interactable.GetInteractPrompt();
                    GameObject obj = hit.collider.gameObject;

                    ShowInteractUI(prompt, obj);
                }
                return;
            }
        }

        if (currentInteractable != null)
        {
            currentInteractable = null;
            HideInteractUI();
        }
    }

    void ShowInteractUI(string defaultPrompt, GameObject targetObject)
    {
        if (interactPanel != null)
            interactPanel.SetActive(true);

        if (interactText != null)
        {
            string finalPrompt = defaultPrompt;
            float promptSize = defaultPromptSize;
            float keySize = defaultKeySize;
            Color promptColor = defaultPromptColor;

            InteractableSettings customSetting = GetCustomSettings(targetObject);
            if (customSetting != null)
            {
                if (!string.IsNullOrEmpty(customSetting.customPrompt))
                    finalPrompt = customSetting.customPrompt;

                promptSize = customSetting.promptSize;
                keySize = customSetting.keySize;
                promptColor = customSetting.promptColor;
            }

            interactText.text = $"<size={promptSize}><color=#{ColorUtility.ToHtmlStringRGB(promptColor)}>{finalPrompt}</color></size>\n<size={keySize}>[{interactKey}]</size>";
        }
    }

    InteractableSettings GetCustomSettings(GameObject obj)
    {
        string objName = obj.name.ToUpper();

        if (obj.GetComponent<MermaidNPC>() != null)
        {
            foreach (var setting in customSettings)
            {
                if (!string.IsNullOrEmpty(setting.objectNameContains))
                {
                    string searchTerm = setting.objectNameContains.ToUpper();
                    if (searchTerm == "MERMAID")
                    {
                        return setting;
                    }
                }
            }
        }

        foreach (var setting in customSettings)
        {
            if (!string.IsNullOrEmpty(setting.objectNameContains))
            {
                string searchTerm = setting.objectNameContains.ToUpper();
                if (objName.Contains(searchTerm))
                {
                    return setting;
                }
            }
        }

        return null;
    }

    public void HideInteractUI()
    {
        if (interactPanel != null)
            interactPanel.SetActive(false);
    }

    public void ShowWarning(string message)
    {
        if (warningText != null)
        {
            warningText.text = message;
            warningText.gameObject.SetActive(true);
            warningTimer = 1.5f;
        }
    }

    public void ShowSuccess()
    {
        if (successText != null)
        {
            successText.text = successMessage;
            successText.gameObject.SetActive(true);
            successTimer = successDisplayTime;
        }
    }

    // YENÝ: Özel mesaj gösterme methodu
    public void ShowMessage(string message)
    {
        if (successText != null)
        {
            successText.text = message;
            successText.gameObject.SetActive(true);
            successTimer = successDisplayTime;
        }
    }

    [ContextMenu("Test: Add Example Settings")]
    void AddExampleSettings()
    {
        customSettings.Clear();

        customSettings.Add(new InteractableSettings()
        {
            objectNameContains = "Mermaid",
            customPrompt = "<size=32>TALK TO MERMAID</size>",
            promptSize = 32f,
            promptColor = new Color(1f, 0.84f, 0f, 1f),
            keySize = 22f
        });

        customSettings.Add(new InteractableSettings()
        {
            objectNameContains = "Door",
            customPrompt = "OPEN DOOR",
            promptSize = 26f,
            promptColor = Color.cyan,
            keySize = 20f
        });

        customSettings.Add(new InteractableSettings()
        {
            objectNameContains = "Relic",
            customPrompt = "Examine",
            promptSize = 24f,
            promptColor = Color.white,
            keySize = 18f
        });

        customSettings.Add(new InteractableSettings()
        {
            objectNameContains = "Chest",
            customPrompt = "OPEN CHEST",
            promptSize = 26f,
            promptColor = new Color(1f, 0.65f, 0f, 1f),
            keySize = 20f
        });

        Debug.Log("4 örnek ayar eklendi!");
    }
}