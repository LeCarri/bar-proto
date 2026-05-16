using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Act3Manager : MonoBehaviour
{
    public static Act3Manager Instance;

    public GameObject clientesActo3;

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
        if (PlayerInteract.Instance != null &&
        PlayerInteract.Instance.currentPickup != null)
        {
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

    //DIÁLOGOS 

    public void MostrarDialogo(string mensaje)
    {
        if (textoSubtitulos == null) return;
        Debug.Log("MOSTRANDO: " + mensaje);


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

    
    //ILUMINACIÓN
    
    public void CambiarIluminacion(string estado)
    {
        Debug.Log("Cambiando iluminación a: " + estado);

        if (lucesNormales != null) lucesNormales.SetActive(false);
        if (lucesServicio != null) lucesServicio.SetActive(false);
        if (lucesCombate != null) lucesCombate.SetActive(false);

        switch (estado)
        {
            case "Normal":
                if (lucesNormales != null) lucesNormales.SetActive(true);
                break;

            case "Servicio":
                if (lucesServicio != null) lucesServicio.SetActive(true);
                break;

            case "Combate":
                if (lucesCombate != null) lucesCombate.SetActive(true);
                break;

            default:
                Debug.LogWarning("Estado de luz inválido: " + estado);
                break;
        }
    }

    
    //SECUENCIA INICIAL
    IEnumerator SecuenciaInicio()
    {
        Debug.Log("Inicio Acto 3");

        if (effectoParpadeo != null)
            effectoParpadeo.IniciarParpadeo();

        yield return new WaitForSeconds(1.5f);

        MostrarDialogo("Ya casi... una ronda más y bajo a buscarlas. Tienen que estar por despertar.");


        yield return new WaitForSeconds(3f);

        CambiarIluminacion("Servicio");

        if (effectoParpadeo != null)
            effectoParpadeo.IniciarParpadeo();

        clientesActo3.SetActive(true);

    }


    //CLIENTES 

    public bool tienePedido = false;

    public bool TienePedidoEntregable()
    {
        return tienePedido;
    }

    public void TomarPedido()
    {
        tienePedido = true;
    }

    public void EntregarPedido()
    {
        tienePedido = false;
    }

    public void ClienteCompletado()
    {
        Debug.Log("Cliente completado");
    }

    //SOLTAR CLIENTES
    IEnumerator SoltarCliente()
    {
        yield return new WaitForSeconds(0.5f);

        if (PlayerInteract.Instance != null)
        {
            PlayerInteract.Instance.ResetCurrentPickup();
        }
    }


    //PARANOIA
    IEnumerator MantenerParanoiaMinima()
    {
        while (true)
        {
            // Mantiene viva la paranoia del Act3
            ParanoiaSystem.Instance.AddParanoia(1f);

            yield return new WaitForSeconds(5f);
        }
    }


    //SOTANO
    public void IrASotano()
    {
        enSotano = true;
        SceneManager.LoadScene("Sotano"); // nombre de la escena
    }
}