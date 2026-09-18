using UnityEngine;

public class PuntoSuministro : MonoBehaviour, IInteractable
{
    [Header("Tipo de Suministro")]
    [Tooltip("Si está activo, usa la lógica del minijuego de cerveza. Si está desactivado, entrega directamente el item (Heladera/Estación).")]
    [SerializeField] private bool esBarra = true;

    [Header("Referencias de Canilla (Solo si esBarra = true)")]
    [SerializeField] private ServicioCerveza servicioCerveza;

    [Header("Referencias de Heladera/Estación (Solo si esBarra = false)")]
    [SerializeField] private ItemSO itemAEntregar; // Arrastrar acá ItemSO_Whisky, ItemSO_Honey, o ItemSO_VasoVacio
    [SerializeField] private AudioSource sonidoSuministro;

    private void Awake()
    {
        if (esBarra && servicioCerveza == null)
            servicioCerveza = GetComponentInChildren<ServicioCerveza>();
    }

    public bool CanInteract()
    {
        // ==========================================
        // CASO 1: ES LA BARRA (Canilla Cerveza)
        // ==========================================
        if (esBarra)
        {
            if (servicioCerveza == null) return false;

            // 1. Si ya hay un vaso en la canilla (listo para servir o agarrar)
            if (servicioCerveza.VasoEnCanilla)
                return true;

            // 2. Si el jugador tiene el vaso vacío en la mano para colocarlo
            if (ControladorMano3D.Instance != null && ControladorMano3D.Instance.TieneManoOcupada())
{
    ItemSO itemEnMano = ControladorMano3D.Instance.ObtenerItemActual();
    
    if (itemEnMano != null && servicioCerveza.ItemVasoVacio != null)
    {
        // Compara por referencia directa O por coincidencia en el nombre del item
        if (itemEnMano == servicioCerveza.ItemVasoVacio || 
            itemEnMano.nombreItem.ToLower().Contains("vaso"))
        {
            return true;
        }
    }
}
            return false;
        }

        // ==========================================
        // CASO 2: ES LA HELADERA / ESTACIÓN / DISPENSER DE VASOS
        // ==========================================
        if (ControladorMano3D.Instance != null && !ControladorMano3D.Instance.TieneManoOcupada())
        {
            return true;
        }

        return false;
    }

    public string GetDescription()
    {
        if (esBarra)
        {
            if (servicioCerveza != null && servicioCerveza.VasoEnCanilla)
            {
                return "Presiona [E] para servir / agarrar cerveza";
            }

            return "Presiona [E] para colocar el vaso";
        }

        return itemAEntregar != null 
            ? $"Presiona [E] para agarrar {itemAEntregar.nombreItem}" 
            : "Presiona [E] para interactuar";
    }

    public void Interact()
    {
        // ==========================================
        // CASO 1: ES LA BARRA (Cerveza)
        // ==========================================
        if (esBarra)
        {
            if (servicioCerveza == null) return;

            // Si hay vaso en la canilla -> Servimos / Agarramos
            if (servicioCerveza.VasoEnCanilla)
            {
                servicioCerveza.Servir();
                return;
            }

            // Si tenemos el vaso vacío en la mano -> Lo colocamos
            if (ControladorMano3D.Instance != null && ControladorMano3D.Instance.TieneManoOcupada())
            {
                servicioCerveza.ColocarVaso();
            }

            return;
        }

        // ==========================================
        // CASO 2: ES LA HELADERA / DISPENSER (Vasos Vacíos, Whisky, Honey)
        // ==========================================
        if (ControladorMano3D.Instance != null && !ControladorMano3D.Instance.TieneManoOcupada())
        {
            if (sonidoSuministro != null)
            {
                sonidoSuministro.Play();
            }

            if (itemAEntregar != null)
            {
                ControladorMano3D.Instance.EquiparItem(itemAEntregar);

                if (Act1Manager.Instance != null)
                {
                    Act1Manager.Instance.tieneObjetoEnMano = true;
                }
            }
        }
    }
}