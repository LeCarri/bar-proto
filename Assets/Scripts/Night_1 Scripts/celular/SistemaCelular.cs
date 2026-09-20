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
    [Tooltip("Canvas o Panel UI con el texto del mensaje (puede estar proyectado en la pantalla del celular 3D o en Canvas pantalla).")]
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
    private string remitenteActual = "";
    private string mensajeActual = "";

    [Header("Ajustes de Animación ")]
    public RectTransform rectPanelUI; // Arrastrá el RectTransform del panel acá
    public float posOcultoY = -1080f; // Posición fuera de pantalla abajo
    public float posVisibleY = 0f;    // Posición visible en pantalla
    public float duracionAnimacion = 0.35f;

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

    public void RecibirMensaje(string remitente, string mensaje)
    {
        remitenteActual = remitente;
        mensajeActual = mensaje;
        tieneNotificacionPendiente = true;

        if (notificacionHUD != null)
        {
            notificacionHUD.SetActive(true);
        }
    }

  private Coroutine rutinaAnimacion;

public void AbrirCelular()
{
    celularAbierto = true;
    tieneNotificacionPendiente = false;

    if (notificacionHUD != null) notificacionHUD.SetActive(false);
    if (objetoCelularEnMano != null) objetoCelularEnMano.SetActive(true);
    if (panelTelefonoUI != null) panelTelefonoUI.SetActive(true);

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
        float t = Mathf.SmoothStep(0f, 1f, tiempo / duracionAnimacion); // Transición suave (Ease In/Out)
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

    if (Act1Manager.Instance != null)
    {
        Act1Manager.Instance.MensajeCelularLeido();
    }
}
}