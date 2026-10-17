using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(BoxCollider))]
public class TriggerPilarDemon : MonoBehaviour
{
    [Header("Personajes")]
    [SerializeField] private GameObject pilarNormal;
    [SerializeField] private GameObject pilarDemon;

    [Header("Sonidos del Jumpscare")]
    [SerializeField] private AudioSource sonido1;
    [SerializeField] private AudioSource sonido2;

    [Header("Sacudida de camara")]
    [SerializeField] private CameraShake sacudidaCamara;
    [SerializeField] private float duracionSacudida = 0.35f;
    [SerializeField] private float intensidadSacudida = 5f;

    [Header("Efectos Visuales")]
    [SerializeField] private Volume volumePilar;
    [SerializeField] private float velocidadEntradaVolume = 12f;

    [Header("Renderer del Jumpscare")]
    [SerializeField] private Camera camaraJugador;
    [SerializeField] private int indicePCRenderer = 1;

    [Header("Llanto anterior y Pilar sentada")]
    [SerializeField] private EventoPilarPasillo eventoPilarPasillo;

    private bool activado = false;

    private void Awake()
    {
        activado = false;

        // Pilar Demon debe estar OCULTA al empezar la noche.
        if (pilarDemon != null)
            pilarDemon.SetActive(false);

        // El efecto visual empieza sin intensidad.
        if (volumePilar != null)
        {
            volumePilar.enabled = true;
            volumePilar.weight = 0f;
        }
    }

    private void OnTriggerEnter(Collider otro)
    {
        if (activado)
            return;

        // Solo Lucas puede iniciar el evento.
        if (otro.GetComponentInParent<PlayerController>() == null)
            return;

        // Debe haber recogido la llave y terminado el evento del bano.
        if (Act2ManagerDemo.Instance == null ||
            !Act2ManagerDemo.Instance.PuedeActivarEventosPostBano)
            return;

        // Pilar debe haber terminado de correr por el pasillo.
        if (eventoPilarPasillo == null ||
            !eventoPilarPasillo.RecorridoFinalizado)
            return;

        activado = true;

        // Recién ACA desaparece Pilar sentada y se detiene el llanto.
        // OcultarPilarSentada() ya llama a DetenerLlanto3D().
        eventoPilarPasillo.OcultarPilarSentada();

        // La Pilar normal que corrio por el pasillo tambien queda desactivada.
        if (pilarNormal != null)
            pilarNormal.SetActive(false);

        if (pilarDemon != null)
            pilarDemon.SetActive(true);

        // Sacudir la camara SOLO durante el jumpscare.
        if (sacudidaCamara != null)
            StartCoroutine(sacudidaCamara.Shake(duracionSacudida, intensidadSacudida));

        if (sonido1 != null)
            sonido1.Play();

        if (sonido2 != null)
            sonido2.Play();

        if (volumePilar != null)
            StartCoroutine(ActivarVolume());

        if (camaraJugador != null)
        {
            var cameraData = camaraJugador.GetUniversalAdditionalCameraData();
            if (cameraData != null)
                cameraData.SetRenderer(indicePCRenderer);
        }

        Debug.Log("[Pilar Demon] Jumpscare activado.", this);
    }

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private IEnumerator ActivarVolume()
    {
        volumePilar.weight = 0f;

        while (volumePilar.weight < 1f)
        {
            volumePilar.weight = Mathf.MoveTowards(
                volumePilar.weight,
                1f,
                Mathf.Max(0.01f, velocidadEntradaVolume) * Time.deltaTime
            );

            yield return null;
        }
    }
}
