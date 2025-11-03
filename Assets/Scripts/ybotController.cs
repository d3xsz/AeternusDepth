using UnityEngine;
using UnityEngine.InputSystem;

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

    private Animator ybotAnim;
    private Rigidbody rb;
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private bool isGrounded = true;
    private bool isJumping = false;
    private bool jumpRequested = false;

    private void Start()
    {
        ybotAnim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // Rigidbody ayarları
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Karakteri "Player" tag'i ile işaretle
        gameObject.tag = "Player";
    }

    private void Update()
    {
        HandleMovement();

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isJumping)
        {
            jumpRequested = true;
        }

        ybotAnim.SetBool("isGrounded", isGrounded);
        ybotAnim.SetFloat("hiz", Mathf.InverseLerp(0, runSpeed, currentSpeed));
        ybotAnim.SetBool("isJumping", isJumping);
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        if (jumpRequested && isGrounded && !isJumping)
        {
            Jump();
            jumpRequested = false;
        }
    }

    private void CheckGrounded()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        bool wasGrounded = isGrounded;

        isGrounded = Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            isJumping = false;
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
        ybotAnim.SetBool("isJumping", true);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}