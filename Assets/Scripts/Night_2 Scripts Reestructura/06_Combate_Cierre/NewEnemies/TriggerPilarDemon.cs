
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

    [Header("Sacudida de cámara")]
    [SerializeField] private CameraShake sacudidaCamara;
    [SerializeField] private float duracionSacudida = 0.35f;
    [SerializeField] private float intensidadSacudida = 5f;

    [Header("Efectos Visuales")]
    [SerializeField] private Volume volumePilar;
    [SerializeField] private float velocidadEntradaVolume = 12f;

    [Header("Renderer del Jumpscare")]
    [SerializeField] private Camera camaraJugador;
    [SerializeField] private int indicePCRenderer = 1;

    [Header("Llanto anterior")]
    [SerializeField] private EventoPilarPasillo eventoPilarPasillo;

    private bool activado = false;

    private void Awake()
    {
        // Pilar Demon empieza oculta.
        if (pilarDemon != null)
            pilarDemon.SetActive(false);

        if (volumePilar != null)
        {
            volumePilar.enabled = true;
            volumePilar.weight = 0f;
        }
    }

    private void OnTriggerEnter(Collider otro)
    {
        if (activado) return;

        // Detectar al jugador.
        if (otro.GetComponentInParent<PlayerController>() == null)
            return;

        activado = true;

        // Detener el llanto 3D anterior.
        if (eventoPilarPasillo != null)
            eventoPilarPasillo.DetenerLlanto3D();

        // Desaparece Pilar normal.
        if (pilarNormal != null)
            pilarNormal.SetActive(false);

        // Aparece Pilar Demon instantaneamente.
        if (pilarDemon != null)
            pilarDemon.SetActive(true);
            // Sacudida de cámara al aparecer Pilar Demon.
            if (sacudidaCamara != null)
            {
                StartCoroutine(
                    sacudidaCamara.Shake(duracionSacudida, intensidadSacudida)
                );
            }   
        // Reproducir ambos sonidos simultaneamente.
        if (sonido1 != null)
            sonido1.Play();

        if (sonido2 != null)
            sonido2.Play();

        Debug.Log("[Pilar Demon] Jumpscare activado.");

        if (volumePilar != null)
        StartCoroutine(ActivarVolume());

        if (camaraJugador != null)
        {
            var cameraData =
                camaraJugador.GetUniversalAdditionalCameraData();

            cameraData.SetRenderer(indicePCRenderer);
        }
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
