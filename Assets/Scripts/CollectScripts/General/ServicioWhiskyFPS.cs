using System.Collections;
using UnityEngine;

public class ServicioWhiskyFPS : MonoBehaviour
{
    [Header("Animación")]
    [SerializeField] private Animator animator;
    [SerializeField] private string nombreAnimacion = "WhiskyService_R";

    [Header("Brazos")]
    [SerializeField] private GameObject mallaBrazos;

    [Header("Botellas")]
    [SerializeField] private GameObject botellaMundo;
    [SerializeField] private GameObject botellaMano;
    [SerializeField] private Transform bottleSocket;
    [SerializeField] private ServicioWhiskyVisual visualWhisky;

    [Header("Resultado del servicio")]
    [SerializeField] private ItemSO itemWhiskyServido;

    [Header("Whisky tomado por Lucas")]
    [SerializeField] private float tiempoAntesDeTomar = 1f;
    [SerializeField] private AudioSource sonidoTrago;

    private bool reproduciendo;

    public bool EstaSirviendo => reproduciendo;

    private void Start()
    {
        if (mallaBrazos != null)
            mallaBrazos.SetActive(false);

        if (botellaMundo != null)
            botellaMundo.SetActive(true);

        if (botellaMano != null)
            botellaMano.SetActive(false);
    }

    public void IniciarServicio()
    {
        if (reproduciendo)
            return;

        reproduciendo = true;

        Debug.Log("INICIO SERVICIO WHISKY");

        if (mallaBrazos != null)
            mallaBrazos.SetActive(true);

        if (botellaMundo != null)
            botellaMundo.SetActive(true);

        if (botellaMano != null)
            botellaMano.SetActive(false);

        if (animator != null)
        {
            animator.Play(nombreAnimacion, 0, 0f);
            animator.Update(0f);
        }
    }

    // Animation Event
    public void AgarrarBotella()
    {
        Debug.Log("EVENTO: AGARRAR BOTELLA");

        if (botellaMundo != null)
            botellaMundo.SetActive(false);

        if (botellaMano != null)
            botellaMano.SetActive(true);
    }

    // Animation Event
    public void FinalizarServicio()
    {
        Debug.Log("EVENTO: FINALIZAR SERVICIO WHISKY");

        if (botellaMano != null)
            botellaMano.SetActive(false);

        if (botellaMundo != null)
            botellaMundo.SetActive(true);

        if (mallaBrazos != null)
            mallaBrazos.SetActive(false);

        reproduciendo = false;

        if (animator != null)
            animator.Play("Idle", 0, 0f);

        // Ocultamos el vaso utilizado durante la animación.
        if (visualWhisky != null)
            visualWhisky.OcultarVaso();

        // Al terminar, SIEMPRE aparece el vaso de whisky
        // en la mano normal del jugador.
        if (ControladorMano3D.Instance != null &&
            itemWhiskyServido != null)
        {
            ControladorMano3D.Instance.EquiparItem(itemWhiskyServido);

            if (Act1Manager.Instance != null)
                Act1Manager.Instance.tieneObjetoEnMano = true;

            bool alguienPidioWhisky = false;

            ClienteInteractuable[] clientes =
                Object.FindObjectsByType<ClienteInteractuable>(
                    FindObjectsSortMode.None
                );

            foreach (ClienteInteractuable cliente in clientes)
            {
                // Solo nos interesa el cliente cuyo turno está activo.
                if (!cliente.esSuTurno)
                    continue;

                // Tiene que haber realizado ya su pedido.
                if (cliente.estadoActual !=
                    ClienteInteractuable.EstadoCliente.EsperandoPedido)
                    continue;

                ItemSO pedido = cliente.itemPedidoRequerido;

                if (pedido != null &&
                    (
                        (Act1Manager.Instance != null &&
                         pedido == Act1Manager.Instance.itemWhisky)
                        ||
                        pedido.nombreItem.ToLower().Contains("whisky")
                    ))
                {
                    alguienPidioWhisky = true;
                    break;
                }
            }

            if (alguienPidioWhisky)
            {
                // Era un pedido real.
                // El vaso permanece en la mano hasta entregarlo.
                Debug.Log(
                    "[Whisky] Pedido de whisky preparado. El vaso queda en la mano."
                );
            }
            else
            {
                // Nadie pidió whisky.
                // Lucas lo mantiene un momento y después se lo toma.
                StartCoroutine(TomarWhiskyAutomaticamente());
            }
        }
        else
        {
            Debug.LogWarning(
                "[ServicioWhiskyFPS] No se pudo equipar el whisky. " +
                "Revisar ControladorMano3D e ItemWhiskyServido."
            );
        }
    }

    private IEnumerator TomarWhiskyAutomaticamente()
    {
        // El vaso permanece visible en PuntoMano.
        yield return new WaitForSeconds(tiempoAntesDeTomar);

        // Sonido de Lucas tomando el whisky.
        if (sonidoTrago != null)
            sonidoTrago.Play();

        // Desaparece el vaso.
        if (ControladorMano3D.Instance != null)
            ControladorMano3D.Instance.VaciarMano();

        if (Act1Manager.Instance != null)
            Act1Manager.Instance.tieneObjetoEnMano = false;

        Debug.Log("[Whisky] Lucas se tomó el whisky.");
    }

    // Animation Event
    public void MostrarVaso()
    {
        Debug.Log("EVENTO: MOSTRAR VASO");

        if (visualWhisky != null)
            visualWhisky.MostrarVaso();
        else
            Debug.LogError("visualWhisky NO ESTÁ ASIGNADO");
    }

    // Animation Event
    public void IniciarVertido()
    {
        Debug.Log("EVENTO: INICIAR VERTIDO");

        if (visualWhisky != null)
            visualWhisky.IniciarVertido();
        else
            Debug.LogError("visualWhisky NO ESTÁ ASIGNADO");
    }

    // Animation Event
    public void DetenerVertido()
    {
        Debug.Log("EVENTO: DETENER VERTIDO");

        if (visualWhisky != null)
            visualWhisky.DetenerVertido();
        else
            Debug.LogError("visualWhisky NO ESTÁ ASIGNADO");
    }
}