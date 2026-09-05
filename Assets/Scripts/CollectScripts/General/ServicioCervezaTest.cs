using UnityEngine;

public class ServicioCervezaTest : MonoBehaviour
{
    [Header("Items")]
    [SerializeField] private ItemSO itemVasoVacio;
    [SerializeField] private ItemSO itemCerveza;

    [Header("Visuales")]
    [SerializeField] private GameObject vasoEnCanillaVisual;
    [SerializeField] private ServicioCervezaVisual servicioVisual;

    public bool VasoEnCanilla { get; private set; }

    public ItemSO ItemVasoVacio => itemVasoVacio;

    public bool EstaSirviendo
    {
        get
        {
            return servicioVisual != null &&
                   servicioVisual.EstaSirviendo;
        }
    }

    private void Start()
    {
        VasoEnCanilla = false;

        if (vasoEnCanillaVisual != null)
            vasoEnCanillaVisual.SetActive(false);

        if (servicioVisual != null)
            servicioVisual.PrepararVasoVacio();
    }

    public void ColocarVaso()
    {
        if (ControladorMano3D.Instance == null)
            return;

        if (ControladorMano3D.Instance.ObtenerItemActual() != itemVasoVacio)
            return;

        // Sacamos el vaso vacío de la mano.
        ControladorMano3D.Instance.VaciarMano();

        VasoEnCanilla = true;

        // Mostramos el vaso debajo de la canilla.
        if (vasoEnCanillaVisual != null)
            vasoEnCanillaVisual.SetActive(true);

        if (servicioVisual != null)
            servicioVisual.PrepararVasoVacio();
    }

    public void Servir()
    {
        if (!VasoEnCanilla)
            return;

        if (servicioVisual == null)
            return;

        if (servicioVisual.EstaSirviendo)
            return;

        servicioVisual.Servir(FinalizarServicio);
    }

    private void FinalizarServicio()
    {
        VasoEnCanilla = false;

        // Desaparece el vaso que estaba físicamente
        // debajo de la canilla.
        if (vasoEnCanillaVisual != null)
            vasoEnCanillaVisual.SetActive(false);

        // Aparece la cerveza terminada en la mano.
        if (ControladorMano3D.Instance != null)
            ControladorMano3D.Instance.EquiparItem(itemCerveza);
    }
}