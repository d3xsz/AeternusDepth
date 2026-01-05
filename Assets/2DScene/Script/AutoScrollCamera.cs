using UnityEngine;

public class AutoScrollCamera : MonoBehaviour
{
    [Header("Kamera Takip Ayarları")]
    public float followSpeed = 5f;
    public Vector2 cameraOffset = new Vector2(0f, 2f);
    public float lookAheadDistance = 3f;
    public float lookAheadSpeed = 2f;

    [Header("Kamera Sınırları (OPSİYONEL)")]
    public bool useBounds = false;
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -5f;
    public float maxY = 20f;

    [Header("Referanslar")]
    public Transform player;
    public Camera gameCamera;

    private PlayerSwimController playerSwimController;
    private Vector3 targetPosition;
    private Vector2 lookAheadPos;
    private Vector2 currentLookAhead;
    private bool isFollowing = true;
    private Vector3 cameraStartPosition;

    void Start()
    {
        if (gameCamera == null)
            gameCamera = GetComponent<Camera>();

        cameraStartPosition = transform.position;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (player != null)
        {
            playerSwimController = player.GetComponent<PlayerSwimController>();
        }

        if (player != null)
        {
            targetPosition = player.position + (Vector3)cameraOffset;
            transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
        }

        Debug.Log("Kamera Player Takip Sistemine geçti!");
    }

    void LateUpdate()
    {
        if (!isFollowing || player == null) return;

        UpdateCameraPosition();
        ApplyCameraBounds();
    }

    void UpdateCameraPosition()
    {
        Vector2 moveDirection = Vector2.zero;
        if (playerSwimController != null)
        {
            moveDirection = playerSwimController.GetInputDirection();
        }

        if (moveDirection.magnitude > 0.1f)
        {
            lookAheadPos = Vector2.Lerp(lookAheadPos, moveDirection.normalized * lookAheadDistance, lookAheadSpeed * Time.deltaTime);
        }
        else
        {
            lookAheadPos = Vector2.Lerp(lookAheadPos, Vector2.zero, lookAheadSpeed * Time.deltaTime);
        }

        currentLookAhead = Vector2.Lerp(currentLookAhead, lookAheadPos, Time.deltaTime * lookAheadSpeed);

        targetPosition = player.position + (Vector3)cameraOffset + (Vector3)currentLookAhead;

        Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        newPosition.z = transform.position.z;

        transform.position = newPosition;
    }

    void ApplyCameraBounds()
    {
        if (!useBounds) return;

        Vector3 clampedPosition = transform.position;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);

        transform.position = clampedPosition;
    }

    // YENİ METOD: ANINDA RESET
    public void InstantResetToPlayer()
    {
        if (player != null)
        {
            // Kamerayı player'ın üstüne ANINDA ışınla
            Vector3 newPos = player.position + (Vector3)cameraOffset;
            newPos.z = transform.position.z;
            transform.position = newPos;

            // Look-ahead'ı sıfırla
            lookAheadPos = Vector2.zero;
            currentLookAhead = Vector2.zero;

            Debug.Log("Kamera ANINDA resetlendi: " + newPos);
        }
    }

    public void AdjustFollowSpeedBasedOnPlayerSpeed(float playerSpeedMultiplier)
    {
        followSpeed = 5f * playerSpeedMultiplier;
        lookAheadDistance = 3f * playerSpeedMultiplier;
    }

    public void SetCameraBounds(float newMinX, float newMaxX, float newMinY, float newMaxY)
    {
        minX = newMinX;
        maxX = newMaxX;
        minY = newMinY;
        maxY = newMaxY;
        useBounds = true;
    }

    public void ResetPositions()
    {
        Debug.Log("Kamera pozisyonu resetlendi");

        if (player != null)
        {
            Vector3 resetPosition = player.position + (Vector3)cameraOffset;
            resetPosition.z = transform.position.z;
            transform.position = resetPosition;
        }
        else
        {
            transform.position = cameraStartPosition;
        }

        lookAheadPos = Vector2.zero;
        currentLookAhead = Vector2.zero;

        Debug.Log($"Kamera resetlendi. Yeni pozisyon: {transform.position}");
    }

    void KeepPlayerInScreen()
    {
        // BU METOD ARTIK ÇALIŞMAYACAK
    }

    void CheckPlayerOutOfBounds()
    {
        // BU METOD ARTIK ÇALIŞMAYACAK
    }

    void HandlePlayerDeath()
    {
        // BU METOD ARTIK ÇALIŞMAYACAK
    }

    public void StartCameraMovement() => isFollowing = true;
    public void StopCameraMovement() => isFollowing = false;
    public void SetScrollSpeed(float newSpeed) => followSpeed = newSpeed;
    public void SetGameRunning(bool running) => isFollowing = running;
    public float GetCurrentSpeed() => followSpeed;
    public bool IsCameraMoving() => isFollowing;
    public int GetSpeedLevel() => 0;

    void OnDrawGizmosSelected()
    {
        if (player != null && gameCamera != null)
        {
            Gizmos.color = Color.cyan;
            float verticalHeight = gameCamera.orthographicSize;
            float horizontalWidth = verticalHeight * gameCamera.aspect;

            Vector3 cameraCenter = transform.position;
            cameraCenter.z = 0;

            Gizmos.DrawWireCube(cameraCenter, new Vector3(horizontalWidth * 2, verticalHeight * 2, 0.1f));

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(player.position, cameraCenter);

            if (playerSwimController != null)
            {
                Vector2 moveDir = playerSwimController.GetInputDirection();
                if (moveDir.magnitude > 0.1f)
                {
                    Vector3 lookAheadEnd = player.position + (Vector3)moveDir.normalized * lookAheadDistance;
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(player.position, lookAheadEnd);
                    Gizmos.DrawWireSphere(lookAheadEnd, 0.3f);
                }
            }
        }
    }
}