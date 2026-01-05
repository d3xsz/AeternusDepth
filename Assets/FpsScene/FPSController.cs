using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float backwardSpeed = 3f;
    public float acceleration = 10f;
    public float jumpForce = 8f;
    public float airControlMultiplier = 0.5f;
    public float gravity = -20f;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public float verticalLookLimit = 80f;
    public Transform cameraTransform; // FPS kameran�z� buraya atay�n
    public bool invertY = false;

    [Header("Hover Mode Settings")]
    public float hoverHeight = 0.2f;
    public float hoverForce = 10f;
    public float hoverSpeedMultiplier = 1.5f;
    public float hoverBobSpeed = 3f;
    public float hoverBobAmount = 0.05f;
    public float hoverEnergyDrainRate = 5f;
    public float hoverEnergyRegenRate = 3f;
    public float maxHoverEnergy = 100f;

    [Header("Effects")]
    public GameObject hoverBubblePrefab;
    public Transform bubbleSpawnPoint;

    [Header("Ground Check Settings")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer = 1;
    public string[] groundTags = { "Ground", "NoSpawnGround", "Platform" };

    // Components
    private Animator animator;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    // Movement variables
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private bool isGrounded = true;
    private bool isJumping = false;
    private bool jumpRequested = false;
    private Vector3 moveDirection;
    private float verticalVelocity;
    private Vector2 moveInput;

    // Mouse look variables
    private float rotationX = 0f;
    private float rotationY = 0f;
    private Vector2 mouseLookInput;

    // Hover Mode variables
    private bool isHovering = false;
    private bool hoverRequested = false;
    private float currentHoverEnergy;
    private Vector3 hoverStartPosition;
    private bool canHover = true;
    private float hoverBobTimer = 0f;
    private GameObject currentBubbleInstance;

    // Reward system i�in
    private float baseWalkSpeed;
    private float baseRunSpeed;
    private float baseBackwardSpeed;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        // Rigidbody ayarlar�
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.useGravity = false; // Kendi gravity sistemimizi kullanaca��z
        }

        // Reward system ba�lang�� de�erleri
        baseWalkSpeed = walkSpeed;
        baseRunSpeed = runSpeed;
        baseBackwardSpeed = backwardSpeed;

        // Kamera ayarlar�
        if (cameraTransform == null)
        {
            cameraTransform = GetComponentInChildren<Camera>()?.transform;
            if (cameraTransform == null)
            {
                Debug.LogError("FPS kamera bulunamad�! L�tfen cameraTransform'u manuel olarak atay�n.");
            }
        }

        gameObject.tag = "Player";

        // Hover enerjisini ba�lat
        currentHoverEnergy = maxHoverEnergy;

        InitializeSceneSettings();
        LockCursor();
    }

    private void InitializeSceneSettings()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (animator != null) animator.enabled = false;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            if (animator != null) animator.enabled = true;
        }
    }

    private void LockCursor()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            HandleMouseLook();
            HandleMovementInput();
            HandleHoverInput();
            HandleJumpInput();
            UpdateAnimations();

            // Escape tu�u ile cursor kilidini a�/kapat
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ToggleCursorLock();
            }
        }
    }

    private void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        // Mouse input'unu al
        if (Mouse.current != null)
        {
            mouseLookInput = Mouse.current.delta.ReadValue() * mouseSensitivity * 0.1f;
        }

        // Yatay d�n�� (karakter ve kamera)
        rotationY += mouseLookInput.x;
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

        // Dikey d�n�� (sadece kamera)
        rotationX += invertY ? mouseLookInput.y : -mouseLookInput.y;
        rotationX = Mathf.Clamp(rotationX, -verticalLookLimit, verticalLookLimit);

        // Kamera rotasyonunu uygula
        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        }
    }

    private void HandleMovementInput()
    {
        // Input'lar� al
        moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1;

        // Normalize et
        if (moveInput.magnitude > 1f)
            moveInput.Normalize();

        // Kameran�n bak�� y�n�ne g�re hareket vekt�r� olu�tur
        Vector3 forward = cameraTransform != null ? cameraTransform.forward : transform.forward;
        Vector3 right = cameraTransform != null ? cameraTransform.right : transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 direction = (forward * moveInput.y + right * moveInput.x).normalized;

        // H�z hesapla
        if (direction.magnitude > 0)
        {
            if (isHovering)
            {
                targetSpeed = walkSpeed * hoverSpeedMultiplier;
            }
            else if (moveInput.y < 0 && moveInput.y >= 0) // Geriye do�ru
            {
                targetSpeed = backwardSpeed;
            }
            else if (Keyboard.current.leftCtrlKey.isPressed && !isHovering) // Ko�ma
            {
                targetSpeed = runSpeed;
            }
            else
            {
                targetSpeed = walkSpeed;
            }
        }
        else
        {
            targetSpeed = 0f;
        }

        // H�z� yumu�ak ge�i�le ayarla
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);

        // Havada kontrol azalmas�
        float controlMultiplier = (!isGrounded && !isHovering) ? airControlMultiplier : 1f;
        moveDirection = direction * currentSpeed * controlMultiplier;
    }

    private void HandleHoverInput()
    {
        // Hover i�in Shift'e bas�l� tut
        if (Keyboard.current.leftShiftKey.isPressed && canHover && currentHoverEnergy > 10f && isGrounded && !isHovering)
        {
            hoverRequested = true;
        }

        if (Keyboard.current.leftShiftKey.wasReleasedThisFrame && isHovering)
        {
            StopHover();
        }

        // Hover enerjisi yenileme
        if (!isHovering && currentHoverEnergy < maxHoverEnergy)
        {
            currentHoverEnergy += hoverEnergyRegenRate * Time.deltaTime;
            currentHoverEnergy = Mathf.Clamp(currentHoverEnergy, 0, maxHoverEnergy);
        }

        // Hover bob timer
        if (isHovering)
        {
            hoverBobTimer += Time.deltaTime * hoverBobSpeed;

            // Particle instance'� spawn point'te tut
            if (currentBubbleInstance != null && bubbleSpawnPoint != null)
            {
                currentBubbleInstance.transform.position = bubbleSpawnPoint.position;
                currentBubbleInstance.transform.rotation = bubbleSpawnPoint.rotation;
            }
        }
    }

    private void HandleJumpInput()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isJumping && !isHovering)
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            CheckGrounded();

            if (hoverRequested && isGrounded && canHover && currentHoverEnergy > 10f)
            {
                StartHover();
                hoverRequested = false;
            }

            if (isHovering)
            {
                HandleHover();
                currentHoverEnergy -= hoverEnergyDrainRate * Time.deltaTime;
                if (currentHoverEnergy <= 0)
                {
                    currentHoverEnergy = 0;
                    StopHover();
                }
            }
            else
            {
                HandleMovement();
            }

            if (jumpRequested && isGrounded && !isJumping && !isHovering)
            {
                Jump();
                jumpRequested = false;
            }
        }
    }

    private void CheckGrounded()
    {
        if (isHovering)
        {
            isGrounded = true;
            isJumping = false;
            return;
        }

        bool wasGrounded = isGrounded;
        isGrounded = false;

        // Capsule collider i�in ground check
        if (capsuleCollider != null)
        {
            float radius = capsuleCollider.radius * 0.9f;
            Vector3 point1 = transform.position + Vector3.up * (capsuleCollider.height / 2 - radius);
            Vector3 point2 = transform.position - Vector3.up * (capsuleCollider.height / 2 - radius);

            if (Physics.CapsuleCast(point1, point2, radius, Vector3.down,
                out RaycastHit hit, groundCheckDistance + radius, groundLayer))
            {
                if (IsValidGroundTag(hit.collider.tag))
                {
                    isGrounded = true;
                }
            }
        }
        else
        {
            // Basit raycast
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down,
                out RaycastHit hit, groundCheckDistance + 0.2f, groundLayer))
            {
                if (IsValidGroundTag(hit.collider.tag))
                {
                    isGrounded = true;
                }
            }
        }

        // Yer�ekimi
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else if (!isHovering)
        {
            verticalVelocity += gravity * Time.fixedDeltaTime;
        }

        if (isGrounded && !wasGrounded)
        {
            isJumping = false;
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

    private void HandleMovement()
    {
        if (rb == null) return;

        // Hareket vekt�r�n� olu�tur
        Vector3 velocity = moveDirection;
        velocity.y = verticalVelocity;

        // Rigidbody velocity'yi ayarla
        rb.linearVelocity = velocity;
    }

    private void StartHover()
    {
        isHovering = true;
        isJumping = false;
        verticalVelocity = 0f;
        hoverStartPosition = transform.position;

        // Yer�ekimini devre d��� b�rak
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }

        // Particle olu�tur
        if (hoverBubblePrefab != null && bubbleSpawnPoint != null)
        {
            if (currentBubbleInstance != null)
                Destroy(currentBubbleInstance);

            currentBubbleInstance = Instantiate(hoverBubblePrefab, bubbleSpawnPoint.position, bubbleSpawnPoint.rotation);
            ParticleSystem ps = currentBubbleInstance.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }
    }

    private void HandleHover()
    {
        if (rb == null) return;

        // Hover bob efekti
        float bobOffset = Mathf.Sin(hoverBobTimer) * hoverBobAmount;
        float targetHeight = hoverStartPosition.y + hoverHeight + bobOffset;

        // Y�ksekli�i koru
        Vector3 currentPos = transform.position;
        float heightDifference = targetHeight - currentPos.y;

        if (Mathf.Abs(heightDifference) > 0.01f)
        {
            float liftForce = heightDifference * hoverForce;
            rb.AddForce(Vector3.up * liftForce, ForceMode.Acceleration);
        }

        // Hareketi uygula
        Vector3 velocity = moveDirection;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;

        // Yatay h�z limiti
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (horizontalVel.magnitude > walkSpeed * hoverSpeedMultiplier)
        {
            horizontalVel = horizontalVel.normalized * walkSpeed * hoverSpeedMultiplier;
            rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);
        }
    }

    private void StopHover()
    {
        isHovering = false;

        if (rb != null)
        {
            rb.useGravity = true;
        }

        // Particle'� temizle
        if (currentBubbleInstance != null)
        {
            ParticleSystem ps = currentBubbleInstance.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Destroy(currentBubbleInstance, 2f);
            currentBubbleInstance = null;
        }
    }

    private void Jump()
    {
        isJumping = true;
        isGrounded = false;
        verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
    }

    private void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("isGrounded", isGrounded);

            if (!isHovering)
            {
                animator.SetFloat("hiz", Mathf.InverseLerp(0, runSpeed, currentSpeed));
            }
            else
            {
                animator.SetFloat("hiz", 0f);
            }

            animator.SetBool("isJumping", isJumping);
            animator.SetBool("isHovering", isHovering);
        }
    }

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

    public float GetHoverEnergyPercentage()
    {
        return currentHoverEnergy / maxHoverEnergy;
    }

    public bool IsHovering()
    {
        return isHovering;
    }

    public void AddHoverEnergy(float amount)
    {
        currentHoverEnergy = Mathf.Clamp(currentHoverEnergy + amount, 0, maxHoverEnergy);
    }

    public void SetHoverEnabled(bool enabled)
    {
        canHover = enabled;
        if (!enabled && isHovering)
        {
            StopHover();
        }
    }
}