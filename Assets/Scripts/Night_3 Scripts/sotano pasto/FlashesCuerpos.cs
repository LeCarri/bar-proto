using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlashesCuerposSotano : MonoBehaviour
{
    [Header("Jugador")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private MonoBehaviour movimientoLucas;
    [SerializeField] private MonoBehaviour camaraLucas;

    [Header("Panel de flashes")]
    [SerializeField] private GameObject panelFlashes;
    [SerializeField] private Image imagenCuerpo;
    [SerializeField] private Sprite[] imagenesCuerpos;

    [Header("Tiempos")]
    [SerializeField] private float duracionFlash = 0.12f;
    [SerializeField] private float intervaloFlash = 0.18f;

    private bool activado;

    private void Start()
    {
        if (panelFlashes != null)
            panelFlashes.SetActive(false);

        if (imagenCuerpo != null)
            imagenCuerpo.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activado)
            return;

        if (!other.CompareTag(playerTag))
            return;

        activado = true;
        StartCoroutine(SecuenciaFlashes());
    }

    private IEnumerator SecuenciaFlashes()
    {
        // Guardar el estado actual de los controles.
        bool movimientoEstabaActivo =
            movimientoLucas != null && movimientoLucas.enabled;

        bool camaraEstabaActiva =
            camaraLucas != null && camaraLucas.enabled;

        // Bloquear a Lucas.
        if (movimientoLucas != null)
            movimientoLucas.enabled = false;

        if (camaraLucas != null)
            camaraLucas.enabled = false;

        // Activar el panel negro.
        if (panelFlashes != null)
            panelFlashes.SetActive(true);

        if (imagenCuerpo != null)
            imagenCuerpo.enabled = false;

        // Mostrar las imágenes una por una.
        if (imagenesCuerpos != null)
        {
            foreach (Sprite imagen in imagenesCuerpos)
            {
                if (imagen == null)
                    continue;

                if (imagenCuerpo != null)
                {
                    imagenCuerpo.sprite = imagen;
                    imagenCuerpo.enabled = true;
                }

                yield return new WaitForSecondsRealtime(
                    duracionFlash
                );

                if (imagenCuerpo != null)
                    imagenCuerpo.enabled = false;

                yield return new WaitForSecondsRealtime(
                    intervaloFlash
                );
            }
        }

        // Ocultar las imágenes y el panel.
        if (panelFlashes != null)
            panelFlashes.SetActive(false);

        // Devolver el control.
        if (movimientoLucas != null)
            movimientoLucas.enabled = movimientoEstabaActivo;

        if (camaraLucas != null)
            camaraLucas.enabled = camaraEstabaActiva;

        Debug.Log("[Sotano] Secuencia de flashes terminada.");
    }
}
