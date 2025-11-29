using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ybotController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float backwardSpeed = 1.5f;
    public float rotationSmooth = 10f;
    public float acceleration = 5f;
    public float jumpForce = 5f;

    [Header("Ground Check Settings")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer = 1;
    public string[] groundTags = { "Ground", "NoSpawnGround", "Platform" };

    [Header("Debug")]
    public bool showGroundDebug = false;

    private Animator ybotAnim;
    private Rigidbody rb;
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private bool isGrounded = true;
    private bool isJumping = false;
    private bool jumpRequested = false;

    // === SAHNE GEÇİŞİ İÇİN EKLENEN KOD ===
    private static ybotController instance;

    // === REWARD SİSTEM İÇİN EKLENEN KOD ===
    private float baseWalkSpeed;
    private float baseRunSpeed;
    private float baseBackwardSpeed;
    // === EKLEME BURADA BİTTİ ===



    private void Start()
    {
        ybotAnim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // === REWARD SİSTEM İÇİN EKLENEN KOD ===
        baseWalkSpeed = walkSpeed;
        baseRunSpeed = runSpeed;
        baseBackwardSpeed = backwardSpeed;
        // === EKLEME BURADA BİTTİ ===

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        gameObject.tag = "Player";

        InitializeSceneSettings();

        if (showGroundDebug)
            Debug.Log("ybotController: Ground detection aktif!");
    }

    private void InitializeSceneSettings()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            // MainMenu'de fizik ve hareketi durdur
            if (rb != null) rb.isKinematic = true;
            if (ybotAnim != null) ybotAnim.enabled = false;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            // Oyun sahnelerinde fizik ve hareketi aktif et
            if (rb != null) rb.isKinematic = false;
            if (ybotAnim != null) ybotAnim.enabled = true;
        }
    }

    private void Update()
    {
        // Sadece SampleScene'de movement aktif (MainMenu değilse)
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            // === REWARD SİSTEM İÇİN EKLENEN KOD ===
            UpdateMovementSpeeds();
            // === EKLEME BURADA BİTTİ ===

            HandleMovement();

            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isJumping)
            {
                jumpRequested = true;
            }

            if (ybotAnim != null)
            {
                ybotAnim.SetBool("isGrounded", isGrounded);
                ybotAnim.SetFloat("hiz", Mathf.InverseLerp(0, runSpeed, currentSpeed));
                ybotAnim.SetBool("isJumping", isJumping);
            }
        }

        
       
    }

    // === REWARD SİSTEM İÇİN EKLENEN METOD ===
    private void UpdateMovementSpeeds()
    {
        if (PlayerStats.Instance != null)
        {
            float speedMultiplier = PlayerStats.Instance.GetMovementSpeedMultiplier();
            walkSpeed = baseWalkSpeed * speedMultiplier;
            runSpeed = baseRunSpeed * speedMultiplier;
            backwardSpeed = baseBackwardSpeed * speedMultiplier;
        }
    }
    // === EKLEME BURADA BİTTİ ===

    private void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            CheckGrounded();

            if (jumpRequested && isGrounded && !isJumping)
            {
                Jump();
                jumpRequested = false;
            }
        }
    }

    private void ToggleCursor()
    {
        if (Cursor.visible)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            if (IsValidGroundTag(hit.collider.tag))
            {
                isGrounded = true;
            }
        }

        if (!isGrounded)
        {
            if (Physics.SphereCast(rayStart, 0.2f, Vector3.down, out hit, groundCheckDistance, groundLayer))
            {
                if (IsValidGroundTag(hit.collider.tag))
                {
                    isGrounded = true;
                }
            }
        }

        if (!isGrounded)
        {
            CheckGroundedByCollision();
        }

        if (isGrounded && !wasGrounded)
        {
            isJumping = false;
            if (showGroundDebug) Debug.Log("ybot: Grounded!");
        }
        else if (!isGrounded && wasGrounded)
        {
            if (showGroundDebug) Debug.Log("ybot: In Air!");
        }
    }

    private bool IsValidGroundTag(string tag)
    {
        foreach (string validTag in groundTags)
        {
            if (tag == validTag)
                return true;
        }
        return false;
    }

    private void CheckGroundedByCollision()
    {
        Collider[] colliders = Physics.OverlapBox(
            transform.position + Vector3.down * (groundCheckDistance * 0.5f),
            new Vector3(0.3f, groundCheckDistance * 0.5f, 0.3f),
            Quaternion.identity,
            groundLayer
        );

        foreach (Collider col in colliders)
        {
            if (IsValidGroundTag(col.tag))
            {
                RaycastHit normalHit;
                if (Physics.Raycast(transform.position, Vector3.down, out normalHit, groundCheckDistance * 2f, groundLayer))
                {
                    if (Vector3.Dot(normalHit.normal, Vector3.up) > 0.7f)
                    {
                        isGrounded = true;
                        break;
                    }
                }
            }
        }
    }

    private void HandleMovement()
    {
        bool forward = Keyboard.current.wKey.isPressed;
        bool backward = Keyboard.current.sKey.isPressed;
        bool left = Keyboard.current.aKey.isPressed;
        bool right = Keyboard.current.dKey.isPressed;
        bool run = Keyboard.current.leftShiftKey.isPressed;

        Vector3 direction = Vector3.zero;
        if (forward) direction += Vector3.forward;
        if (backward) direction += Vector3.back;
        if (left) direction += Vector3.left;
        if (right) direction += Vector3.right;
        direction.Normalize();

        if (direction.magnitude > 0)
        {
            if (run)
                targetSpeed = runSpeed;
            else if (backward)
                targetSpeed = backwardSpeed;
            else
                targetSpeed = walkSpeed;
        }
        else
            targetSpeed = 0f;

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmooth);
        }

        Vector3 moveDirection = transform.forward * currentSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + moveDirection);
    }

    private void Jump()
    {
        isJumping = true;
        isGrounded = false;
        if (ybotAnim != null)
            ybotAnim.SetBool("isJumping", true);

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        if (showGroundDebug) Debug.Log("ybot: Jump!");
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGroundDebug) return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawRay(rayStart, Vector3.down * groundCheckDistance);

        Gizmos.color = isGrounded ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(rayStart + Vector3.down * groundCheckDistance, 0.2f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(
            transform.position + Vector3.down * (groundCheckDistance * 0.5f),
            new Vector3(0.6f, groundCheckDistance, 0.6f)
        );
    }
}