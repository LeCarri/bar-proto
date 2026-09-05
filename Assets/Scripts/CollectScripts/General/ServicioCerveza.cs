using UnityEngine;

public class ServicioCerveza : MonoBehaviour
{
    [Header("Items")]
    [SerializeField] private ItemSO itemVasoVacio;
    [SerializeField] private ItemSO itemCerveza;

    [Header("Visuales")]
    [SerializeField] private GameObject vasoEnCanillaVisual;
    [SerializeField] private ServicioCervezaVisual servicioVisual;

    public bool VasoEnCanilla { get; private set; }

    public ItemSO ItemVasoVacio => itemVasoVacio;

    public void ColocarVaso()
    {
        if (ControladorMano3D.Instance == null)
            return;

        if (ControladorMano3D.Instance.ObtenerItemActual() != itemVasoVacio)
            return;

        ControladorMano3D.Instance.VaciarMano();

        VasoEnCanilla = true;

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

        servicioVisual.Servir(FinalizarServicio);
    }

    private void FinalizarServicio()
    {
        VasoEnCanilla = false;

        if (vasoEnCanillaVisual != null)
            vasoEnCanillaVisual.SetActive(false);

        if (ControladorMano3D.Instance != null)
        {
            ControladorMano3D.Instance.EquiparItem(itemCerveza);
        }
    }
}