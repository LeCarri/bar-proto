using System.Collections;
using UnityEngine;

public class ServicioCerveza : MonoBehaviour
{
    [Header("Scriptable Objects")]
    public ItemSO ItemVasoVacio;
    public ItemSO ItemCerveza;

    [Header("Referencias Visuales")]
    [SerializeField] private GameObject vasoEnCanillaVisual;
    [SerializeField] private ServicioCervezaVisual servicioVisual;

    [Header("Audio")]
    [SerializeField] private AudioSource sonidoServirCerveza;

    private bool vasoEnCanilla = false;
    private bool sirviendo = false;

    public bool VasoEnCanilla => vasoEnCanilla;

    public void ColocarVaso()
    {
        if (vasoEnCanilla || sirviendo) return;

        if (ControladorMano3D.Instance != null)
        {
            ControladorMano3D.Instance.VaciarMano();
        }

        if (Act1Manager.Instance != null)
        {
            Act1Manager.Instance.tieneObjetoEnMano = false;
        }

        vasoEnCanilla = true;

        if (vasoEnCanillaVisual != null)
            vasoEnCanillaVisual.SetActive(true);

        if (servicioVisual != null)
            servicioVisual.PrepararVasoVacio();

        Debug.Log("[ServicioCerveza] Vaso colocado en la canilla.");
    }

    public void Servir()
    {
        if (!vasoEnCanilla || sirviendo) return;

        sirviendo = true;

        if (sonidoServirCerveza != null)
        {
            sonidoServirCerveza.Play();
        }

        // Delegamos el proceso de servido y animación del chorro al script visual
        if (servicioVisual != null)
        {
            servicioVisual.Servir(FinalizarServido);
        }
        else
        {
            FinalizarServido();
        }
    }

    private void FinalizarServido()
    {
        if (sonidoServirCerveza != null && sonidoServirCerveza.isPlaying)
        {
            sonidoServirCerveza.Stop();
        }

        if (vasoEnCanillaVisual != null)
            vasoEnCanillaVisual.SetActive(false);

        // Liberamos los estados internos para poder volver a servir luego
        vasoEnCanilla = false;
        sirviendo = false;

        if (ControladorMano3D.Instance != null && ItemCerveza != null)
        {
            ControladorMano3D.Instance.EquiparItem(ItemCerveza);

            // Sincronizamos con el Act1Manager
            if (Act1Manager.Instance != null)
            {
                Act1Manager.Instance.tieneObjetoEnMano = true;
            }
        }

        Debug.Log("[ServicioCerveza] Cerveza servida, equipada en mano y canilla liberada.");
    }

    public void ResetearCanilla()
    {
        vasoEnCanilla = false;
        sirviendo = false;

        if (vasoEnCanillaVisual != null)
            vasoEnCanillaVisual.SetActive(false);

        if (sonidoServirCerveza != null && sonidoServirCerveza.isPlaying)
            sonidoServirCerveza.Stop();
    }
}