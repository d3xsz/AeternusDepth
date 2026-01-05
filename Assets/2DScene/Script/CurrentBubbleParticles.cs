using UnityEngine;

public class CurrentBubbleParticles : MonoBehaviour
{
    [Header("Particle Sistemleri")]
    public ParticleSystem bubbleParticles;

    [Header("Baloncuk Ayarlarý")]
    public int bubbleCount = 30;
    public float bubbleSpeed = 1.5f;
    public float bubbleSizeMin = 0.3f;
    public float bubbleSizeMax = 0.8f;

    [Header("Akýntý Yönü")]
    public Vector2 currentDirection = new Vector2(-1f, 0f);
    public float currentStrength = 2f;

    [Header("Render Ayarlarý - EN ÖNEMLÝ KISIM")]
    public string sortingLayer = "ParticlesBack";
    public int sortingOrder = -1;

    void Start()
    {
        // Eðer particle sistemi yoksa oluþtur
        if (bubbleParticles == null)
        {
            CreateBubbleParticles();
        }

        SetupParticles();
        bubbleParticles.Play();

        Debug.Log("Baloncuklar baþlatýldý. Sorting: " + sortingLayer + " Order: " + sortingOrder);
    }

    void CreateBubbleParticles()
    {
        GameObject particleObj = new GameObject("BubbleParticles");
        particleObj.transform.parent = transform;
        particleObj.transform.localPosition = Vector3.zero;

        bubbleParticles = particleObj.AddComponent<ParticleSystem>();
    }

    void SetupParticles()
    {
        if (bubbleParticles == null) return;

        // RENDERER AYARLARI - EN KRÝTÝK KISIM
        ParticleSystemRenderer renderer = bubbleParticles.GetComponent<ParticleSystemRenderer>();

        // 1. SORTING LAYER VE ORDER
        if (!string.IsNullOrEmpty(sortingLayer))
        {
            renderer.sortingLayerName = sortingLayer;
        }
        renderer.sortingOrder = sortingOrder;

        // 2. MATERIAL - Basit baloncuk görünümü
        Material bubbleMat = new Material(Shader.Find("Sprites/Default"));
        bubbleMat.color = new Color(0.6f, 0.8f, 1f, 0.6f);
        renderer.material = bubbleMat;

        // 3. PARTICLE AYARLARI
        var main = bubbleParticles.main;
        main.startSpeed = bubbleSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(bubbleSizeMin, bubbleSizeMax);
        main.startLifetime = 3f;
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.02f; // Hafif yukarý

        // 4. EMISSION (Yayýlma)
        var emission = bubbleParticles.emission;
        emission.rateOverTime = bubbleCount;

        // 5. SHAPE (Þekil)
        var shape = bubbleParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;

        // Collider boyutunu al veya varsayýlan
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            shape.scale = new Vector3(collider.size.x, collider.size.y, 1f);
        }
        else
        {
            shape.scale = new Vector3(5f, 2f, 1f);
        }

        // 6. VELOCITY (Hýz - Akýntý yönü)
        var velocity = bubbleParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = currentDirection.normalized.x * currentStrength;
        velocity.y = currentDirection.normalized.y * currentStrength;

        // 7. COLOR OVER LIFETIME (Rengin zamanla deðiþimi)
        var colorOverLifetime = bubbleParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.7f, 0.9f, 1f), 0f),
                new GradientColorKey(new Color(0.4f, 0.7f, 1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.7f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
    }

    void OnValidate()
    {
        // Inspector'da deðiþiklik yapýldýðýnda güncelle
        if (Application.isPlaying && bubbleParticles != null)
        {
            SetupParticles();
        }
    }

    void OnDrawGizmos()
    {
        // Akýntý yönünü göster
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.5f);
        Vector3 dirEnd = transform.position + (Vector3)currentDirection.normalized * 2f;
        Gizmos.DrawLine(transform.position, dirEnd);
        Gizmos.DrawWireSphere(dirEnd, 0.1f);
    }
}