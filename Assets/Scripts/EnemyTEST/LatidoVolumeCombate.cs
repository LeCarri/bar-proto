using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LatidoVolumeCombate : MonoBehaviour
{
    [Header("Volume")]
    public Volume volumen;

    [Header("Ritmo")]
    [Tooltip("Velocidad del latido.")]
    public float latidosPorMinuto = 55f;

    [Range(0f, 1f)]
    public float fuerzaLatido = 0.7f;

    [Header("Lens Distortion")]
    public float distorsionBase = -0.08f;
    public float distorsionExtra = -0.10f;

    [Tooltip("Pequeño movimiento del centro de la distorsión.")]
    public Vector2 movimientoCentro = new Vector2(0.015f, 0.01f);

    [Header("Chromatic Aberration")]
    public float aberracionBase = 0.15f;
    public float aberracionExtra = 0.15f;

    [Header("Vignette")]
    public float vignetteBase = 0.25f;
    public float vignetteExtra = 0.10f;

    [Header("Forma del latido")]
    public AnimationCurve formaLatido = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.08f, 1f),
        new Keyframe(0.17f, 0f),
        new Keyframe(0.27f, 0.55f),
        new Keyframe(0.38f, 0f),
        new Keyframe(1f, 0f)
    );

    private LensDistortion lensDistortion;
    private ChromaticAberration chromaticAberration;
    private Vignette vignette;

    private void Start()
    {
        if (volumen == null)
            volumen = GetComponent<Volume>();

        if (volumen == null || volumen.profile == null)
        {
            Debug.LogError("[LatidoVolume] Falta Volume o Profile.");
            enabled = false;
            return;
        }

        volumen.profile.TryGet(out lensDistortion);
        volumen.profile.TryGet(out chromaticAberration);
        volumen.profile.TryGet(out vignette);

        if (lensDistortion != null)
        {
            lensDistortion.intensity.overrideState = true;
            lensDistortion.center.overrideState = true;
        }

        if (chromaticAberration != null)
            chromaticAberration.intensity.overrideState = true;

        if (vignette != null)
            vignette.intensity.overrideState = true;
    }

    private void Update()
    {
        // Si el Volume está apagado, no hacemos nada.
        if (volumen == null || volumen.weight <= 0.001f)
            return;

        float ciclosPorSegundo = latidosPorMinuto / 60f;

        float fase =
            Mathf.Repeat(Time.time * ciclosPorSegundo, 1f);

        float latido =
            formaLatido.Evaluate(fase) * fuerzaLatido;

        // --------------------------
        // LENS DISTORTION
        // --------------------------

        if (lensDistortion != null)
        {
            lensDistortion.intensity.value =
                distorsionBase +
                distorsionExtra * latido;

            float x =
                0.5f +
                Mathf.Sin(Time.time * 1.7f) *
                movimientoCentro.x *
                latido;

            float y =
                0.5f +
                Mathf.Cos(Time.time * 1.3f) *
                movimientoCentro.y *
                latido;

            lensDistortion.center.value =
                new Vector2(x, y);
        }

        // --------------------------
        // CHROMATIC ABERRATION
        // --------------------------

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value =
                Mathf.Clamp01(
                    aberracionBase +
                    aberracionExtra * latido
                );
        }

        // --------------------------
        // VIGNETTE
        // --------------------------

        if (vignette != null)
        {
            vignette.intensity.value =
                Mathf.Clamp01(
                    vignetteBase +
                    vignetteExtra * latido
                );
        }
    }
}