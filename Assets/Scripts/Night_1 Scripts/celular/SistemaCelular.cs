using UnityEngine;
using TMPro;
using System.Collections;

public class SistemaCelular : MonoBehaviour
{
    public static SistemaCelular Instance { get; private set; }

    [Header("Modelo 3D en Mano")]
    [Tooltip("El GameObject del celular ubicado en la mano/cámara del jugador.")]
    public GameObject objetoCelularEnMano;

    [Header("UI del Celular")]
    [Tooltip("Canvas o Panel UI con el texto del mensaje.")]
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

    [Header("Ajustes de Animación")]
    public RectTransform rectPanelUI; 
    public float posOcultoY = -1080f; 
    public float posVisibleY = 0f;    
    public float duracionAnimacion = 0.35f;

    private Coroutine rutinaAnimacion;

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

    // 1. Para mensajes normales que se leen en pantalla (Servicio)
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

        if (notificacionHUD != null) notificacionHUD.SetActive(false);
        if (objetoCelularEnMano != null) objetoCelularEnMano.SetActive(true);
        if (panelTelefonoUI != null) panelTelefonoUI.SetActive(true);

        // Asignamos la UI (Mostrará "1 mensaje nuevo" si es el final de la noche)
        if (textoRemitente != null) textoRemitente.text = remitenteActual;
        if (textoMensaje != null) textoMensaje.text = mensajeActual;

        if (rutinaAnimacion != null) StopCoroutine(rutinaAnimacion);
        rutinaAnimacion = StartCoroutine(MoverPanel(posOcultoY, posVisibleY));

        if (sonidoAbrir != null) sonidoAbrir.Play();
    }

    public void CerrarCelular()
    {
        celularAbierto = false;

        if (rutinaAnimacion != null) StopCoroutine(rutinaAnimacion);
        rutinaAnimacion = StartCoroutine(MoverPanelYCerrar(posVisibleY, posOcultoY));

        if (sonidoCerrar != null) sonidoCerrar.Play();
    }

    private IEnumerator MoverPanel(float inicioY, float finY)
    {
        float tiempo = 0f;
        Vector2 pos = rectPanelUI.anchoredPosition;

        while (tiempo < duracionAnimacion)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, tiempo / duracionAnimacion);
            pos.y = Mathf.Lerp(inicioY, finY, t);
            rectPanelUI.anchoredPosition = pos;
            yield return null;
        }

        pos.y = finY;
        rectPanelUI.anchoredPosition = pos;
    }

    private IEnumerator MoverPanelYCerrar(float inicioY, float finY)
    {
        yield return MoverPanel(inicioY, finY);

        if (objetoCelularEnMano != null) objetoCelularEnMano.SetActive(false);
        if (panelTelefonoUI != null) panelTelefonoUI.SetActive(false);

        // Solo notifica al Act1Manager si no era la pantalla de cierre
        if (!esSoloNotificacionFinal && Act1Manager.Instance != null)
        {
            Act1Manager.Instance.MensajeCelularLeido();
        }
    }
}