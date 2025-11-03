using UnityEngine;

public class ChestController : MonoBehaviour
{
    [Header("Chest Settings")]
    public float interactionRange = 2f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Debug")]
    public bool showDebug = true;
    public bool alwaysShowGizmos = true;

    private Transform player;
    private Animation chestAnimation;
    private bool canInteract = false;
    private bool isOpened = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        chestAnimation = GetComponent<Animation>();

        if (showDebug && player == null)
            Debug.LogError("Chest: Player bulunamadý! Player'ýn 'Player' tag'i olduðundan emin olun.");

        if (showDebug && chestAnimation == null)
            Debug.LogError("Chest: Animation component'i bulunamadý!");
        else if (showDebug)
        {
            foreach (AnimationState state in chestAnimation)
            {
                Debug.Log($"Mevcut animasyon: {state.name}");
            }
        }
    }

    void Update()
    {
        if (player == null || isOpened) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        canInteract = distanceToPlayer <= interactionRange;

        if (canInteract && Input.GetKeyDown(interactKey))
        {
            OpenChest();
        }
    }

    void OpenChest()
    {
        if (isOpened) return;

        if (chestAnimation != null)
        {
            // Tüm animasyonlarý durdur ve hemen open animasyonunu son karesine al
            chestAnimation.Stop();

            // "open" animasyonunu bul ve hemen son karesine al
            foreach (AnimationState state in chestAnimation)
            {
                if (state.name.ToLower().Contains("open"))
                {
                    // Animasyonu son karesine al ve oynat
                    state.time = state.length; // Son kareye git
                    state.speed = 0; // Oynatma hýzýný sýfýrla (duraðan kalsýn)
                    chestAnimation.Play(state.name);

                    isOpened = true;
                    if (showDebug) Debug.Log("Chest anýnda açýldý: " + state.name);
                    OnChestOpened();
                    return;
                }
            }

            // Eðer "open" bulunamazsa, element 1'i kullan
            int index = 0;
            foreach (AnimationState state in chestAnimation)
            {
                if (index == 1) // Element 1 (open)
                {
                    state.time = state.length;
                    state.speed = 0;
                    chestAnimation.Play(state.name);

                    isOpened = true;
                    if (showDebug) Debug.Log("Chest anýnda açýldý (element 1): " + state.name);
                    OnChestOpened();
                    return;
                }
                index++;
            }

            Debug.LogError("Open animasyonu bulunamadý!");
        }
    }

    void OnChestOpened()
    {
        // Chest açýldýðýnda yapýlacak iþlemler
        Debug.Log("Chest anýnda açýldý! Ödül verilebilir.");
    }

    // Gizmos ile etkileþim alanýný göster
    void OnDrawGizmos()
    {
        if (!alwaysShowGizmos && !showDebug) return;

        // Etkileþim alaný
        Gizmos.color = canInteract ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Player'a olan baðlantýyý göster
        if (canInteract && player != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, player.position);
        }

        // Chest durumunu göster
        Gizmos.color = isOpened ? Color.red : Color.white;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 1.2f);
    }
}