
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ControlDistorsionMental : MonoBehaviour
{
    public static ControlDistorsionMental Instance { get; private set; }

    [Header("Renderer de Unity")]
    public ScriptableRendererData rendererData;

    [Header("Nombre de la Renderer Feature")]
    public string nombreEfecto = "Distorsion";

    private ScriptableRendererFeature efecto;
    private bool estadoOriginal;

    private bool paranoiaAlta;
    private bool combateActivo;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        if (rendererData == null)
        {
            Debug.LogError("[DistorsionMental] Falta PC_Renderer.");
            return;
        }

        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature != null &&
                feature.name.IndexOf(
                    nombreEfecto,
                    System.StringComparison.OrdinalIgnoreCase
                ) >= 0)
            {
                efecto = feature;
                break;
            }
        }

        if (efecto == null)
        {
            Debug.LogError(
                "[DistorsionMental] No se encontró la Renderer Feature."
            );
            return;
        }

        estadoOriginal = efecto.isActive;
        ActualizarEfecto();
    }

    public void EstablecerParanoiaAlta(bool activa)
    {
        paranoiaAlta = activa;
        ActualizarEfecto();
    }

    public void EstablecerCombate(bool activo)
    {
        combateActivo = activo;
        ActualizarEfecto();
    }

    private void ActualizarEfecto()
    {
        if (efecto == null)
            return;

        bool activar = paranoiaAlta || combateActivo;

        if (efecto.isActive != activar)
            efecto.SetActive(activar);
    }

    private void OnDestroy()
    {
        if (efecto != null)
            efecto.SetActive(estadoOriginal);

        if (Instance == this)
            Instance = null;
    }
}
