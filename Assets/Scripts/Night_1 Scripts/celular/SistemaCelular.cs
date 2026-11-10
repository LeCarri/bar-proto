using UnityEngine;
using TMPro;

public class SistemaCelular : MonoBehaviour
{
    public static SistemaCelular Instance { get; private set; }

    [Header("Modelo 3D y Pantalla (Canvas)")]
    [Tooltip("El GameObject del celular ubicado en la mano/cámara del jugador.")]
    public GameObject objetoCelularEnMano;
    [Tooltip("El Panel o Canvas UI que está sobre la pantalla del celular 3D.")]
    public GameObject panelTelefonoUI;
    public TextMeshProUGUI textoRemitente;
    public TextMeshProUGUI textoMensaje;
    public GameObject notificacionHUD; // Aviso "Presiona [T] para abrir"

    [Header("Configuración de Teclas")]
    public KeyCode teclaAbrirCelular = KeyCode.T;

    [Header("Audio")]
    public AudioSource sonidoAbrir;
    public AudioSource sonidoCerrar;

    private bool tieneNotificacionPendiente = false;
    private bool celularAbierto = false;
    private bool esSoloNotificacionFinal = false; // Flag para controlar si solo muestra "1 mensaje nuevo"
    private string remitenteActual = "";
    private string mensajeActual = "";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (objetoCelularEnMano != null) objetoCelularEnMano.SetActive(false);
        if (panelTelefonoUI != null) panelTelefonoUI.SetActive(false);
        if (notificacionHUD != null) notificacionHUD.SetActive(false);
    }

    private void Update()
    {
        // Abrir / Cerrar con la tecla T
        if ((tieneNotificacionPendiente || celularAbierto) && Input.GetKeyDown(teclaAbrirCelular))
        {
            if (!celularAbierto)
            {
                AbrirCelular();
            }
            else
            {
                CerrarCelular();
            }
        }
    }

    // 1. Para mensajes normales que se leen en pantalla
    public void RecibirMensaje(string remitente, string mensaje)
    {
        remitenteActual = remitente;
        mensajeActual = mensaje;
        esSoloNotificacionFinal = false;
        tieneNotificacionPendiente = true;

        if (notificacionHUD != null)
        {
            notificacionHUD.SetActive(true);
        }
    }

    // 2. Para la notificación de fin de noche (Sin revelar el contenido)
    public void RecibirNotificacionSinLeer(string remitente)
    {
        remitenteActual = remitente;
        mensajeActual = "1 mensaje nuevo";
        esSoloNotificacionFinal = true;
        tieneNotificacionPendiente = true;

        if (notificacionHUD != null)
        {
            notificacionHUD.SetActive(true);
        }
    }

    public void AbrirCelular()
    {
        celularAbierto = true;
        tieneNotificacionPendiente = false;

        // Detener vibración
        if (Act1Manager.Instance != null &&
            Act1Manager.Instance.sonidoVibracionCelular != null)
        {
            Act1Manager.Instance.sonidoVibracionCelular.Stop();
        }

        if (notificacionHUD != null)
            notificacionHUD.SetActive(false);

        // Mostrar celular en la mano
        if (objetoCelularEnMano != null)
            objetoCelularEnMano.SetActive(true);

        if (panelTelefonoUI != null)
            panelTelefonoUI.SetActive(true);

        // Actualizar mensajes
        if (textoRemitente != null)
            textoRemitente.text = remitenteActual;

        if (textoMensaje != null)
            textoMensaje.text = mensajeActual;

        if (sonidoAbrir != null)
            sonidoAbrir.Play();

        // NUEVO: indicar cómo guardar el teléfono
        if (Act1Manager.Instance != null)
        {
            Act1Manager.Instance.MostrarDialogo(
                "Presiona [T] para guardar el teléfono."
            );
        }
    }

    public void CerrarCelular()
    {
        celularAbierto = false;

        // Desactivamos de golpe el modelo 3D y el Canvas
        if (objetoCelularEnMano != null) objetoCelularEnMano.SetActive(false);
        if (panelTelefonoUI != null) panelTelefonoUI.SetActive(false);

        if (sonidoCerrar != null) sonidoCerrar.Play();

        // Solo notifica al Act1Manager si no era la pantalla de cierre
        if (!esSoloNotificacionFinal && Act1Manager.Instance != null)
        {
            Act1Manager.Instance.MensajeCelularLeido();
        }
    }
}