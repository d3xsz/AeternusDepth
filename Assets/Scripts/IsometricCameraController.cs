using UnityEngine;

public class IsometricCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 12, -10);
    public float followSmooth = 5f;
    public float isoAngleX = 30f; // 30 derece izometrik
    public float isoAngleY = 45f; // 45 derece izometrik

    private Vector3 cameraVelocity = Vector3.zero;

    private void Start()
    {
        // Eðer target atanmamýþsa, karakteri otomatik bul
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }

        if (target != null)
        {
            SetupIsometricCamera();
        }
        else
        {
            Debug.LogError("Kamera için target bulunamadý! Lütfen karakteri 'Player' tag'i ile iþaretleyin veya manual olarak atayýn.");
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Basit ve etkili izometrik kamera takibi
        Quaternion isoRotation = Quaternion.Euler(isoAngleX, isoAngleY, 0f);
        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref cameraVelocity, followSmooth * Time.deltaTime);
        transform.rotation = isoRotation;
    }

    private void SetupIsometricCamera()
    {
        Quaternion isoRotation = Quaternion.Euler(isoAngleX, isoAngleY, 0f);
        transform.position = target.position + offset;
        transform.rotation = isoRotation;
    }
}