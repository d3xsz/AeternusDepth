using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public string weaponName = "Pistol";
    public float pickupRadius = 2f;
    public KeyCode pickupKey = KeyCode.E;

    [Header("Bone Settings")]
    public string handBoneName = "RightHand";
    public Vector3 bonePosition = new Vector3(0.05f, 0.02f, 0.1f);
    public Vector3 boneRotation = new Vector3(0f, 0f, 0f);

    [Header("Debug")]
    public bool showDebug = true;
    public bool adjustMode = false;
    public float adjustSpeed = 0.01f;

    // PUBLIC - CRITICAL!
    public bool isEquipped = false;

    private Transform player;
    private Animator playerAnimator;
    private bool canPickup = false;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform handBone;
    private AutoBulletShooter bulletShooter;

    void Start()
    {
        FindPlayer();
        originalParent = transform.parent;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        // AutoBulletShooter'ı bul
        bulletShooter = GetComponent<AutoBulletShooter>();
        if (bulletShooter != null)
        {
            bulletShooter.enabled = false;
            if (showDebug) Debug.Log("AutoBulletShooter bulundu ve devre dışı bırakıldı");
        }
        else
        {
            if (showDebug) Debug.LogError("AutoBulletShooter bulunamadı!");
        }
    }

    void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
            if (showDebug) Debug.Log("Player bulundu: " + player.name);
        }
        else
        {
            if (showDebug) Debug.LogError("Player bulunamadı! Player tag'ini kontrol et.");
        }
    }

    void Update()
    {
        if (isEquipped && adjustMode)
        {
            AdjustWeaponPosition();
            return;
        }

        if (isEquipped) return;

        // Player yoksa bulmaya çalış
        if (player == null)
        {
            FindPlayer();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        canPickup = distance <= pickupRadius;

        if (canPickup && Input.GetKeyDown(pickupKey))
        {
            PickUpWeapon();
        }
    }

    void PickUpWeapon()
    {
        if (player == null)
        {
            Debug.LogError("Player yok, silah alınamaz!");
            return;
        }

        handBone = FindHandBone();
        if (handBone == null)
        {
            Debug.LogError($"El kemiği bulunamadı: {handBoneName}");
            return;
        }

        // Silahı el kemiğine bağla
        transform.SetParent(handBone);
        transform.localPosition = bonePosition;
        transform.localEulerAngles = boneRotation;

        isEquipped = true;

        // AutoBulletShooter'ı etkinleştir
        if (bulletShooter != null)
        {
            bulletShooter.enabled = true;
            if (showDebug) Debug.Log("✅ AutoBulletShooter ETKİNLEŞTİRİLDİ!");
        }
        else
        {
            Debug.LogError("❌ AutoBulletShooter bulunamadı!");
        }

        // Fizik bileşenlerini devre dışı bırak
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

        // PlayerWeaponManager'a silahın alındığını bildir
        PlayerWeaponManager weaponManager = player.GetComponent<PlayerWeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.OnWeaponPickedUp(transform);
        }
        else
        {
            Debug.LogError("PlayerWeaponManager bulunamadı!");
        }

        if (showDebug)
        {
            Debug.Log($"✅ {weaponName} {handBone.name} kemiğine bağlandı!");
            Debug.Log($"🔫 Silah durumu: isEquipped = {isEquipped}");
        }
    }

    Transform FindHandBone()
    {
        if (player == null) return null;

        // 1. Önce isimle ara
        Transform bone = FindDeepChild(player, handBoneName);
        if (bone != null) return bone;

        // 2. Humanoid karakter için Animator'dan ara
        if (playerAnimator != null && playerAnimator.isHuman)
        {
            bone = playerAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            if (bone != null) return bone;
        }

        // 3. Alternatif isimlerle ara
        string[] alternativeNames = {
            "RightHand", "Hand_R", "mixamorig:RightHand",
            "hand_r", "R_Hand", "Right Hand", "RightArm"
        };

        foreach (string name in alternativeNames)
        {
            bone = FindDeepChild(player, name);
            if (bone != null)
            {
                if (showDebug) Debug.Log($"Alternatif kemik bulundu: {name}");
                return bone;
            }
        }

        return null;
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        if (parent == null) return null;
        if (parent.name == name) return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }

    void AdjustWeaponPosition()
    {
        if (handBone == null) return;

        // Pozisyon ayarı
        if (Input.GetKey(KeyCode.UpArrow)) bonePosition.y += adjustSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow)) bonePosition.y -= adjustSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftArrow)) bonePosition.x -= adjustSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow)) bonePosition.x += adjustSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.PageUp)) bonePosition.z += adjustSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.PageDown)) bonePosition.z -= adjustSpeed * Time.deltaTime;

        // Rotasyon ayarı
        if (Input.GetKey(KeyCode.Q)) boneRotation.z += adjustSpeed * 100 * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) boneRotation.z -= adjustSpeed * 100 * Time.deltaTime;
        if (Input.GetKey(KeyCode.R)) boneRotation.x += adjustSpeed * 100 * Time.deltaTime;
        if (Input.GetKey(KeyCode.F)) boneRotation.x -= adjustSpeed * 100 * Time.deltaTime;

        transform.localPosition = bonePosition;
        transform.localEulerAngles = boneRotation;

        Debug.Log($"Position: {bonePosition}, Rotation: {boneRotation}");
    }

    public void DropWeapon()
    {
        if (!isEquipped) return;

        transform.SetParent(originalParent);
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;

        // AutoBulletShooter'ı devre dışı bırak
        if (bulletShooter != null)
        {
            bulletShooter.enabled = false;
            if (showDebug) Debug.Log("❌ AutoBulletShooter devre dışı bırakıldı!");
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = true;

        isEquipped = false;
        adjustMode = false;
        handBone = null;

        if (showDebug) Debug.Log("🗑️ Silah bırakıldı");
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebug) return;

        Gizmos.color = canPickup ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}