// BasitFPSCamera.cs
using UnityEngine;

public class BasitFPSCamera : MonoBehaviour
{
    [Header("Ayarlar")]
    public float fareHassasiyet = 2f;
    public bool fareKilitli = true;

    [Header("Sýnýrlar")]
    public float maxYukariBak = 80f;
    public float maxAsagiBak = -80f;

    private float donusX = 0f; // Yatay dönüþ
    private float donusY = 0f; // Dikey dönüþ

    void Start()
    {
        // Fareyi kilitle
        if (fareKilitli)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Debug.Log("FPS Kamera hazýr! Pozisyon: " + transform.localPosition);
    }

    void Update()
    {
        // Fare ile bakýþ
        FareBakisi();

        // ESC ile fare kilidini aç/kapa
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            FareKilidiDegistir();
        }
    }

    void FareBakisi()
    {
        // Fare girdisini al
        float fareX = Input.GetAxis("Mouse X") * fareHassasiyet;
        float fareY = Input.GetAxis("Mouse Y") * fareHassasiyet;

        // Dikey bakýþ (yukarý-aþaðý) - KAMERAYI döndür
        donusY -= fareY;
        donusY = Mathf.Clamp(donusY, maxAsagiBak, maxYukariBak);

        transform.localRotation = Quaternion.Euler(donusY, 0f, 0f);

        // Yatay dönüþ (sað-sol) - KARAKTERÝ döndür
        donusX += fareX;

        // Player'ý döndür (üst objeye bak)
        if (transform.parent != null)
        {
            transform.parent.rotation = Quaternion.Euler(0f, donusX, 0f);
        }
        else
        {
            // Eðer parent yoksa, kamerayý döndür
            transform.rotation = Quaternion.Euler(donusY, donusX, 0f);
        }
    }

    void FareKilidiDegistir()
    {
        fareKilitli = !fareKilitli;

        if (fareKilitli)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("Fare kilitlendi");
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Fare serbest");
        }
    }

    // Debug için: Kamera pozisyonunu göster
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1f);
    }
}