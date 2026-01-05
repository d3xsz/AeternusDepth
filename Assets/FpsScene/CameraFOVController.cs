using UnityEngine;

public class CameraFOVController : MonoBehaviour
{
    [Header("FOV Settings")]
    [SerializeField] private float startFOV = 30f;
    [SerializeField] private float minFOV = 30f;
    [SerializeField] private float maxFOV = 65f;

    [Header("Smooth Settings")]
    [SerializeField] private float lerpSpeed = 2f;

    private Camera cam;
    private float targetFOV;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.fieldOfView = startFOV;
        targetFOV = startFOV;
    }

    void Update()
    {
        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            targetFOV,
            Time.deltaTime * lerpSpeed
        );
    }

    public void IncreaseFOV(float amount)
    {
        targetFOV += amount;
        targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
    }

    // YENÝ: FOV azaltma (yengeç çalýnca)
    public void DecreaseFOV(float amount)
    {
        targetFOV -= amount;
        targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
    }
}