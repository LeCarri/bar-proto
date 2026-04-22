using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HomeBackgroundAmbient : MonoBehaviour
{
    [Header("Background Motion")]
    [SerializeField] private Vector2 movementAmplitude = new Vector2(18f, 12f);
    [SerializeField] private Vector2 movementSpeed = new Vector2(0.09f, 0.07f);
    [SerializeField] private float scaleAmplitude = 0.015f;
    [SerializeField] private float scaleSpeed = 0.05f;
    [SerializeField] private Vector2 backgroundOverscan = new Vector2(140f, 100f);

    [Header("Particles")]
    [SerializeField] private int particleCount = 24;
    [SerializeField] private Vector2 particleSizeRange = new Vector2(12f, 28f);
    [SerializeField] private Vector2 particleSpeedRange = new Vector2(4f, 14f);
    [SerializeField] private Vector2 particleDrift = new Vector2(3f, 1.5f);
    [SerializeField] private Vector2 particleAlphaRange = new Vector2(0.08f, 0.22f);

    private RectTransform backgroundRect;
    private RectTransform canvasRect;
    private RectTransform particlesRoot;
    private Vector2 initialAnchoredPosition;
    private Vector2 initialSizeDelta;
    private Vector3 initialScale;
    private Texture2D particleTexture;
    private Sprite particleSprite;
    private ParticleData[] particles;

    private struct ParticleData
    {
        public RectTransform Rect;
        public Image Image;
        public Vector2 Position;
        public Vector2 Velocity;
        public float BaseAlpha;
        public float AlphaPhase;
        public float AlphaSpeed;
        public float Size;
    }

    private void Awake()
    {
        backgroundRect = GetComponent<RectTransform>();

        if (backgroundRect == null)
        {
            enabled = false;
            return;
        }

        canvasRect = backgroundRect.parent as RectTransform;

        if (canvasRect == null)
        {
            enabled = false;
            return;
        }

        initialAnchoredPosition = backgroundRect.anchoredPosition;
        initialSizeDelta = backgroundRect.sizeDelta;
        initialScale = backgroundRect.localScale;

        EnsureParticlesRoot();
        EnsureParticleSprite();
        CreateParticles();
        ApplyOverscan();
    }

    private void OnEnable()
    {
        if (backgroundRect != null)
        {
            ApplyOverscan();
        }
    }

    private void Update()
    {
        if (backgroundRect == null || canvasRect == null)
        {
            return;
        }

        AnimateBackground();
        AnimateParticles();
    }

    private void OnDestroy()
    {
        if (particleSprite != null)
        {
            Destroy(particleSprite);
        }

        if (particleTexture != null)
        {
            Destroy(particleTexture);
        }
    }

    private void OnDisable()
    {
        if (backgroundRect == null)
        {
            return;
        }

        backgroundRect.anchoredPosition = initialAnchoredPosition;
        backgroundRect.sizeDelta = initialSizeDelta;
        backgroundRect.localScale = initialScale;
    }

    private void EnsureParticlesRoot()
    {
        Transform existing = canvasRect.Find("AmbientParticles");
        if (existing != null)
        {
            particlesRoot = existing as RectTransform;
        }
        else
        {
            GameObject root = new GameObject("AmbientParticles", typeof(RectTransform));
            particlesRoot = root.GetComponent<RectTransform>();
            particlesRoot.SetParent(canvasRect, false);
            particlesRoot.anchorMin = Vector2.zero;
            particlesRoot.anchorMax = Vector2.one;
            particlesRoot.offsetMin = Vector2.zero;
            particlesRoot.offsetMax = Vector2.zero;
            particlesRoot.pivot = new Vector2(0.5f, 0.5f);
        }

        particlesRoot.SetSiblingIndex(backgroundRect.GetSiblingIndex() + 1);
    }

    private void EnsureParticleSprite()
    {
        if (particleSprite != null)
        {
            return;
        }

        const int size = 64;
        particleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            name = "AmbientParticleTexture"
        };

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / radius;
                float alpha = Mathf.Clamp01(1f - distance);
                alpha *= alpha;
                alpha *= 0.85f;
                particleTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        particleTexture.Apply();
        particleSprite = Sprite.Create(particleTexture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        particleSprite.name = "AmbientParticleSprite";
    }

    private void CreateParticles()
    {
        ClearParticles();
        particles = new ParticleData[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            GameObject particleObject = new GameObject($"Particle_{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform particleRect = particleObject.GetComponent<RectTransform>();
            Image particleImage = particleObject.GetComponent<Image>();

            particleRect.SetParent(particlesRoot, false);
            particleRect.anchorMin = new Vector2(0.5f, 0.5f);
            particleRect.anchorMax = new Vector2(0.5f, 0.5f);
            particleRect.pivot = new Vector2(0.5f, 0.5f);

            float size = Random.Range(particleSizeRange.x, particleSizeRange.y);
            float speed = Random.Range(particleSpeedRange.x, particleSpeedRange.y);
            float alpha = Random.Range(particleAlphaRange.x, particleAlphaRange.y);
            Vector2 direction = new Vector2(
                Random.Range(-particleDrift.x, particleDrift.x),
                Random.Range(0.6f, 1f) * speed + Random.Range(-particleDrift.y, particleDrift.y));

            ParticleData particle = new ParticleData
            {
                Rect = particleRect,
                Image = particleImage,
                Position = RandomCanvasPosition(size),
                Velocity = direction,
                BaseAlpha = alpha,
                AlphaPhase = Random.Range(0f, Mathf.PI * 2f),
                AlphaSpeed = Random.Range(0.4f, 0.9f),
                Size = size
            };

            particleRect.sizeDelta = Vector2.one * size;
            particleRect.anchoredPosition = particle.Position;
            particleImage.sprite = particleSprite;
            particleImage.color = new Color(1f, 1f, 1f, alpha);
            particleImage.raycastTarget = false;

            particles[i] = particle;
        }
    }

    private void ClearParticles()
    {
        if (particlesRoot == null)
        {
            return;
        }

        for (int i = particlesRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(particlesRoot.GetChild(i).gameObject);
        }
    }

    private void ApplyOverscan()
    {
        backgroundRect.sizeDelta = initialSizeDelta + backgroundOverscan;
    }

    private void AnimateBackground()
    {
        float time = Time.unscaledTime;
        backgroundRect.anchoredPosition = initialAnchoredPosition + new Vector2(
            Mathf.Sin(time * movementSpeed.x * Mathf.PI * 2f) * movementAmplitude.x,
            Mathf.Cos(time * movementSpeed.y * Mathf.PI * 2f) * movementAmplitude.y);

        float scaleOffset = 1f + Mathf.Sin(time * scaleSpeed * Mathf.PI * 2f) * scaleAmplitude;
        backgroundRect.localScale = initialScale * scaleOffset;
    }

    private void AnimateParticles()
    {
        if (particles == null || particles.Length == 0)
        {
            return;
        }

        float deltaTime = Time.unscaledDeltaTime;
        float width = canvasRect.rect.width;
        float height = canvasRect.rect.height;
        float time = Time.unscaledTime;

        for (int i = 0; i < particles.Length; i++)
        {
            ParticleData particle = particles[i];
            particle.Position += particle.Velocity * deltaTime;

            float horizontalLimit = (width * 0.5f) + particle.Size;
            float verticalLimit = (height * 0.5f) + particle.Size;

            if (particle.Position.y > verticalLimit)
            {
                particle.Position.y = -verticalLimit;
                particle.Position.x = Random.Range(-horizontalLimit, horizontalLimit);
            }

            if (particle.Position.x > horizontalLimit)
            {
                particle.Position.x = -horizontalLimit;
            }
            else if (particle.Position.x < -horizontalLimit)
            {
                particle.Position.x = horizontalLimit;
            }

            float alphaPulse = 0.75f + ((Mathf.Sin(time * particle.AlphaSpeed + particle.AlphaPhase) + 1f) * 0.125f);
            particle.Image.color = new Color(1f, 1f, 1f, particle.BaseAlpha * alphaPulse);
            particle.Rect.anchoredPosition = particle.Position;
            particles[i] = particle;
        }
    }

    private Vector2 RandomCanvasPosition(float size)
    {
        float width = (canvasRect.rect.width * 0.5f) + size;
        float height = (canvasRect.rect.height * 0.5f) + size;
        return new Vector2(Random.Range(-width, width), Random.Range(-height, height));
    }
}
