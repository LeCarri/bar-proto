using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;

public class Act3Manager : MonoBehaviour
{
    public static Act3Manager Instance;

    public GameObject clientesActo3;
    public GameObject enemigos;

    [Header("UI y Diálogos")]
    public TextMeshProUGUI textoSubtitulos;

    [Header("Efectos")]
    public EffectoParpadeo effectoParpadeo;

    [Header("Sistemas de Iluminación")]
    public GameObject lucesNormales;
    public GameObject lucesServicio;
    public GameObject lucesCombate;

    private Coroutine dialogoActual;

    public bool enSotano = false;
    public int clientesAtendidos = 0;

    private ClienteAct3 ultimoClienteDetectado;

    // PEDIDOS
    public string pedidoActual = "";
    public bool tienePedido = false;
    public bool tienePedidoBuscado = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ParanoiaSystem.Instance.AddParanoia(50f);

        StartCoroutine(MantenerParanoiaMinima());
        StartCoroutine(SecuenciaInicio());
    }

    void Update()
    {
        // PUERTA DEL SÓTANO
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(
                Camera.main.transform.position,
                Camera.main.transform.forward
            );

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 10f))
            {
                PuertaSotano puerta =
                    hit.collider.GetComponentInParent<PuertaSotano>();

                if (puerta != null)
                {
                    puerta.Interact();
                    return;
                }
            }
        }

        // PICKUPS
        if (PlayerInteract.Instance != null &&
            PlayerInteract.Instance.currentPickup != null)
        {
            // PEDIDO
            PedidoPickup pedido =
                PlayerInteract.Instance.currentPickup.GetComponent<PedidoPickup>();

            if (pedido != null)
            {
                pedido.Interact();

                StartCoroutine(SoltarCliente());
                return;
            }

            // CLIENTE
            ClienteAct3Pickup trigger =
                PlayerInteract.Instance.currentPickup.GetComponent<ClienteAct3Pickup>();

            if (trigger != null)
            {
                if (trigger.clienteReal != ultimoClienteDetectado)
                {
                    ultimoClienteDetectado = trigger.clienteReal;

                    trigger.clienteReal.Interact();

                    StartCoroutine(SoltarCliente());
                }
            }
        }
        else
        {
            ultimoClienteDetectado = null;
        }
    }

    // =========================
    // DIÁLOGOS
    // =========================

    public void MostrarDialogo(string mensaje)
    {
        if (textoSubtitulos == null) return;

        if (dialogoActual != null)
            StopCoroutine(dialogoActual);

        textoSubtitulos.text = mensaje;
        dialogoActual = StartCoroutine(LimpiarTextoCoroutine());
    }

    IEnumerator LimpiarTextoCoroutine()
    {
        yield return new WaitForSeconds(4f);
        textoSubtitulos.text = "";
    }

    // =========================
    // ILUMINACIÓN
    // =========================

    public void CambiarIluminacion(string estado)
    {
        if (lucesNormales != null) lucesNormales.SetActive(false);
        if (lucesServicio != null) lucesServicio.SetActive(false);
        if (lucesCombate != null) lucesCombate.SetActive(false);

        switch (estado)
        {
            case "Normal":
                lucesNormales.SetActive(true);
                break;

            case "Servicio":
                lucesServicio.SetActive(true);
                break;

            case "Combate":
                lucesCombate.SetActive(true);
                break;

            case "Apagado":
                break;
        }
    }

    // =========================
    // INICIO
    // =========================

    IEnumerator SecuenciaInicio()
    {
        if (effectoParpadeo != null)
            effectoParpadeo.IniciarParpadeo();

        yield return new WaitForSeconds(1.5f);

        MostrarDialogo(
            "Ya casi... una ronda más y bajo a buscarlas."
        );

        yield return new WaitForSeconds(3f);

        CambiarIluminacion("Servicio");

        if (effectoParpadeo != null)
            effectoParpadeo.IniciarParpadeo();

        clientesActo3.SetActive(true);
    }

    // =========================
    // PEDIDOS
    // =========================

    public bool TienePedidoEntregable()
    {
        return tienePedido && tienePedidoBuscado;
    }

    public void TomarPedido(string pedido)
    {
        pedidoActual = pedido;
        tienePedido = true;
        tienePedidoBuscado = false;

        Debug.Log("Pedido tomado: " + pedidoActual);
    }

    public void RecogerPedido(string objeto)
    {
        Debug.Log("Objeto recogido: " + objeto);
        Debug.Log("Pedido actual: " + pedidoActual);

        if (tienePedido && objeto == pedidoActual)
        {
            tienePedidoBuscado = true;

            Debug.Log("Pedido correcto.");

            MostrarDialogo(
                "Tengo el pedido. Hora de entregarlo."
            );
        }
    }

    public void EntregarPedido()
    {
        tienePedido = false;
        tienePedidoBuscado = false;
        pedidoActual = "";
    }

    // =========================
    // CLIENTES
    // =========================

    public void ClienteCompletado()
    {
        clientesAtendidos++;

        Debug.Log("Clientes completos: " + clientesAtendidos);

        if (clientesAtendidos == 2)
        {
            StartCoroutine(AvanzarNoche());
        }
    }

    // =========================
    // AVANZAR NOCHE
    // =========================

    IEnumerator AvanzarNoche()
    {
        Debug.Log("Los dos clientes fueron atendidos");

        yield return new WaitForSeconds(4f);

        if (effectoParpadeo != null)
            effectoParpadeo.IniciarParpadeo();

        clientesActo3.SetActive(false);

        yield return new WaitForSeconds(3f);

        MostrarDialogo("Listo... voy a buscarlas.");

        yield return new WaitForSeconds(5f);

        CambiarIluminacion("Apagado");

        yield return new WaitForSeconds(1.5f);

        enemigos.SetActive(true);
    }

    // =========================
    // UTILIDADES
    // =========================

    public IEnumerator SoltarCliente()
    {
        yield return new WaitForSeconds(0.5f);

        if (PlayerInteract.Instance != null)
            PlayerInteract.Instance.ResetCurrentPickup();
    }

    IEnumerator MantenerParanoiaMinima()
    {
        while (true)
        {
            ParanoiaSystem.Instance.AddParanoia(1f);
            yield return new WaitForSeconds(5f);
        }
    }

    // =========================
    // SÓTANO
    // =========================

    public void IrASotano()
    {
        enSotano = true;
        SceneManager.LoadScene("Basement");
    }
}