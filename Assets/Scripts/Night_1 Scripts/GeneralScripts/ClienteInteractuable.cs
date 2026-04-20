using UnityEngine;

public class ClienteInteractuable : MonoBehaviour, IInteractable
{
    [Header("Datos del Cliente")]
    public string nombreCliente = "Cliente";
    [TextArea(3, 10)]
    public string dialogoPedido = "Hola... ¿me das una cerveza?";

    [Header("Referencias Visuales")]
    public GameObject indicadorVioleta; // El triángulo que flota

    private bool yaAtendido = false;

    public void Interact()
    {
        // Solo interactuamos si no fue atendido
        if (!yaAtendido)
        {
            Atender();
        }
        else
        {
            // Opcional: Diálogo corto por si le volvés a hablar
            //FindObjectOfType<Act1Manager>().MostrarDialogo(nombreCliente + ": Ya estoy bien, gracias.");
        }
    }

        public string GetDescription()
    {
        if (yaAtendido) return "";
        return "Presiona [E] para atender a " + nombreCliente;
    }

    void Atender()
    {
        yaAtendido = true;

        // 1. Apagamos la flecha violeta
        if (indicadorVioleta != null) 
            indicadorVioleta.SetActive(false);

        // 2. Mostramos su pedido en la UI de Lucas
        FindObjectOfType<Act1Manager>().MostrarDialogo(nombreCliente + ": " + dialogoPedido);

        // 3. Le avisamos al Manager que sume uno para habilitar la cocina/olor
        FindObjectOfType<Act1Manager>().ClienteAtendido();

        Debug.Log(nombreCliente + " ha sido atendido.");
    }
}