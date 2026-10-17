using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Presentacion de objetivos para la Noche 1.
/// No decide el progreso: Act1Manager sigue siendo la fuente de verdad.
/// </summary>
public class ObjetivosUI : MonoBehaviour
{
    public enum TipoBebida { Cerveza, Whisky }
    private enum Modo { Ninguno, Simple, Checklist, Clientes }

    [System.Serializable]
    public class TarjetaCliente
    {
        public RectTransform raiz;
        public TMP_Text nombre;
        public TMP_Text bebida;
        public Image iconoBebida;
    }

    [Header("Contenedor que se despliega desde arriba")]
    [SerializeField] private RectTransform panelAnimado;
    [SerializeField] private CanvasGroup grupoPanel;
    [SerializeField] private float distanciaEntrada = 350f;
    [SerializeField] private float duracionEntrada = 0.35f;
    [SerializeField] private float duracionSalida = 0.2f;

    [Header("Tres versiones del panel (solo una activa)")]
    [SerializeField] private GameObject panelCorto;
    [SerializeField] private GameObject panelIntermedio;
    [SerializeField] private GameObject panelLargo;

    [Header("Objetivo puntual / panel corto")]
    [SerializeField] private TMP_Text textoObjetivoSimple;

    [Header("Checklist / panel largo")]
    [SerializeField] private TMP_Text textoSillas;
    [SerializeField] private TMP_Text textoMesas;
    [SerializeField] private TMP_Text textoSuelo;
    [SerializeField] private TMP_Text contadorSillas;
    [SerializeField] private TMP_Text contadorMesas;
    [SerializeField] private TMP_Text contadorSuelo;
    [Tooltip("Hijo con la marca de verificacion, encima del cuadrado vacio")]
    [SerializeField] private GameObject marcaSillas;
    [SerializeField] private GameObject marcaMesas;
    [SerializeField] private GameObject marcaSuelo;

    [Header("Textos personalizados del checklist")]
    [SerializeField] private TMP_Text textoPistaHerramienta; // Opcional para Noche 2

    [Header("Pedidos / panel intermedio")]
    [SerializeField] private TMP_Text contadorClientes;
    [Tooltip("Crear dos tarjetas dentro de un Viewport con RectMask2D")]
    [SerializeField] private TarjetaCliente tarjetaActual;
    [SerializeField] private TarjetaCliente tarjetaEntrante;
    [SerializeField] private float alturaTarjeta = 100f;
    [SerializeField] private float duracionCarrusel = 0.4f;
    [Tooltip("Orden equivalente al array clientesEnOrden del Act1Manager")]
    [SerializeField] private TipoBebida[] bebidasPorCliente =
    {
        TipoBebida.Cerveza, TipoBebida.Cerveza,
        TipoBebida.Whisky, TipoBebida.Whisky
    };
    [SerializeField] private Sprite iconoCerveza;
    [SerializeField] private Sprite iconoWhisky;

    [Header("Sonidos de interfaz (AudioSource 2D)")]
    [SerializeField] private AudioSource fuenteSonidos;
    [SerializeField] private AudioClip sonidoNuevoObjetivo;
    [SerializeField] private AudioClip sonidoTareaCompletada;
    [SerializeField] private AudioClip sonidoSiguienteCliente;

    private Modo modoActual = Modo.Ninguno;
    private Vector2 posicionVisible;
    private Coroutine rutinaPanel;
    private Coroutine rutinaCarrusel;
    private bool inicializoChecklist;
    private bool sillasHechas;
    private bool mesasHechas;
    private bool sueloHecho;
    private string ultimoObjetivoSimple = "";
    private string etiquetaSillas = "Acomodar sillas";
    private string etiquetaMesas = "Limpiar mesas";
    private string etiquetaSuelo = "Trapear el suelo";

    /// <summary>
    /// Permite reutilizar el mismo prefab entre Noche 1 y Noche 2.
    /// Los nombres originales de Noche 1 se mantienen si no se llama.
    /// </summary>
    public void ConfigurarEtiquetasChecklist(string primera, string segunda, string tercera)
    {
        etiquetaSillas = primera ?? "Acomodar sillas";
        etiquetaMesas = segunda ?? "Limpiar mesas";
        etiquetaSuelo = tercera ?? "Trapear el suelo";
    }

    /// <summary>
    /// Pista opcional, debajo de las filas: no cambia ni oculta el checklist.
    /// Se usa en Noche 2 cuando falta una herramienta.
    /// </summary>
    public void ActualizarPistaChecklist(string mensaje)
    {
        if (textoPistaHerramienta == null) return;
        bool mostrar = !string.IsNullOrWhiteSpace(mensaje);
        textoPistaHerramienta.gameObject.SetActive(mostrar);
        if (mostrar) textoPistaHerramienta.text = mensaje;
    }

    private void Awake()
    {
        if (panelAnimado == null) panelAnimado = GetComponent<RectTransform>();
        if (grupoPanel == null) grupoPanel = GetComponent<CanvasGroup>();

        if (panelAnimado == null || grupoPanel == null)
        {
            Debug.LogError("[ObjetivosUI] Asigna Panel Animado y Grupo Panel en el Inspector.", this);
            enabled = false;
            return;
        }

        posicionVisible = panelAnimado.anchoredPosition;
        grupoPanel.alpha = 0f;
        grupoPanel.interactable = false;
        grupoPanel.blocksRaycasts = false;
        panelAnimado.anchoredPosition = posicionVisible + Vector2.up * distanciaEntrada;

        ActivarSolo(null);
        if (tarjetaActual != null && tarjetaActual.raiz != null)
            tarjetaActual.raiz.gameObject.SetActive(false);
        if (tarjetaEntrante != null && tarjetaEntrante.raiz != null)
            tarjetaEntrante.raiz.gameObject.SetActive(false);
    }

    public void MostrarObjetivoSimple(string mensaje)
    {
        if (!enabled) return;
        if (modoActual == Modo.Simple && ultimoObjetivoSimple == mensaje && grupoPanel.alpha > 0.99f)
            return;

        // Si la ultima tarjeta aun esta animandose, el objetivo nuevo tiene prioridad.
        if (rutinaCarrusel != null)
        {
            StopCoroutine(rutinaCarrusel);
            rutinaCarrusel = null;
        }
        ultimoObjetivoSimple = mensaje;
        if (textoObjetivoSimple != null) textoObjetivoSimple.text = mensaje;
        MostrarModo(Modo.Simple, panelCorto);
    }

    /// <summary>
    /// Actualiza cantidades sin mover el panel. Solo suena al finalizar una fila entera.
    /// </summary>
    public void ActualizarChecklist(int sillas, int totalSillas, int mesas, int totalMesas,
                                     int suelo, int totalSuelo)
    {
        if (!enabled) return;

        bool s = totalSillas > 0 && sillas >= totalSillas;
        bool m = totalMesas > 0 && mesas >= totalMesas;
        bool b = totalSuelo > 0 && suelo >= totalSuelo;

        if (textoSillas != null) textoSillas.text = s ? $"<s>{etiquetaSillas}</s>" : etiquetaSillas;
        if (textoMesas != null) textoMesas.text = m ? $"<s>{etiquetaMesas}</s>" : etiquetaMesas;
        if (textoSuelo != null) textoSuelo.text = b ? $"<s>{etiquetaSuelo}</s>" : etiquetaSuelo;

        if (contadorSillas != null) contadorSillas.text = $"{sillas}/{totalSillas}";
        if (contadorMesas != null) contadorMesas.text = $"{mesas}/{totalMesas}";
        if (contadorSuelo != null) contadorSuelo.text = $"{suelo}/{totalSuelo}";

        if (marcaSillas != null) marcaSillas.SetActive(s);
        if (marcaMesas != null) marcaMesas.SetActive(m);
        if (marcaSuelo != null) marcaSuelo.SetActive(b);

        if (inicializoChecklist && ((!sillasHechas && s) || (!mesasHechas && m) || (!sueloHecho && b)))
            Reproducir(sonidoTareaCompletada);

        sillasHechas = s;
        mesasHechas = m;
        sueloHecho = b;
        inicializoChecklist = true;

        // Solo se despliega al entrar en la fase. Los cambios posteriores son estaticos.
        if (modoActual != Modo.Checklist)
            MostrarModo(Modo.Checklist, panelLargo);
    }

    public void IniciarClientes(string nombre, int indice, int total)
    {
        if (!enabled) return;
        if (rutinaCarrusel != null) StopCoroutine(rutinaCarrusel);
        rutinaCarrusel = null;

        if (contadorClientes != null) contadorClientes.text = $"{indice}/{total}";
        PrepararTarjeta(tarjetaActual, nombre, indice);
        if (tarjetaActual != null && tarjetaActual.raiz != null)
        {
            tarjetaActual.raiz.anchoredPosition = Vector2.zero;
            tarjetaActual.raiz.gameObject.SetActive(true);
        }
        if (tarjetaEntrante != null && tarjetaEntrante.raiz != null)
            tarjetaEntrante.raiz.gameObject.SetActive(false);

        MostrarModo(Modo.Clientes, panelIntermedio);
    }

    public void SiguienteCliente(string nombre, int indice, int total)
    {
        if (!enabled || modoActual != Modo.Clientes) return;
        if (rutinaCarrusel != null) StopCoroutine(rutinaCarrusel);
        rutinaCarrusel = StartCoroutine(AnimarSiguienteCliente(nombre, indice, total));
    }

    public void FinalizarClientes(int total)
    {
        if (!enabled || modoActual != Modo.Clientes) return;
        if (rutinaCarrusel != null) StopCoroutine(rutinaCarrusel);
        rutinaCarrusel = StartCoroutine(AnimarFinClientes(total));
    }

    /// <summary>
    /// Noche 2: el ultimo cliente normal se retira hacia arriba y, una vez
    /// terminada la animacion, aparece el objetivo narrativo del cliente corrupto.
    /// </summary>
    public void FinalizarClientesYMostrarObjetivo(string proximoObjetivo, int total)
    {
        if (!enabled) return;
        if (modoActual != Modo.Clientes)
        {
            MostrarObjetivoSimple(proximoObjetivo);
            return;
        }
        if (rutinaCarrusel != null) StopCoroutine(rutinaCarrusel);
        rutinaCarrusel = StartCoroutine(AnimarFinClientesYAbrirObjetivo(proximoObjetivo, total));
    }

    /// <summary>
    /// Para cinemáticas, jumpscares, quiebres y otros momentos sin objetivo visible.
    /// No desactivar el GameObject con SetActive(false): animamos el CanvasGroup.
    /// </summary>
    public void Ocultar()
    {
        if (!enabled) return;
        if (rutinaCarrusel != null)
        {
            StopCoroutine(rutinaCarrusel);
            rutinaCarrusel = null;
        }
        if (rutinaPanel != null) StopCoroutine(rutinaPanel);
        rutinaPanel = StartCoroutine(AnimarPanel(false));
        modoActual = Modo.Ninguno;
    }

    private void MostrarModo(Modo nuevoModo, GameObject panelVisible)
    {
        if (rutinaPanel != null) StopCoroutine(rutinaPanel);
        ActivarSolo(panelVisible);
        modoActual = nuevoModo;
        grupoPanel.alpha = 0f;
        panelAnimado.anchoredPosition = posicionVisible + Vector2.up * distanciaEntrada;
        Reproducir(sonidoNuevoObjetivo);
        rutinaPanel = StartCoroutine(AnimarPanel(true));
    }

    private void ActivarSolo(GameObject elegido)
    {
        if (panelCorto != null) panelCorto.SetActive(panelCorto == elegido);
        if (panelIntermedio != null) panelIntermedio.SetActive(panelIntermedio == elegido);
        if (panelLargo != null) panelLargo.SetActive(panelLargo == elegido);
    }

    private IEnumerator AnimarPanel(bool mostrar)
    {
        float duracion = Mathf.Max(0.01f, mostrar ? duracionEntrada : duracionSalida);
        Vector2 inicio = panelAnimado.anchoredPosition;
        Vector2 fin = mostrar ? posicionVisible : posicionVisible + Vector2.up * distanciaEntrada;
        float alphaInicio = grupoPanel.alpha;
        float alphaFinal = mostrar ? 1f : 0f;
        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempo / duracion));
            panelAnimado.anchoredPosition = Vector2.LerpUnclamped(inicio, fin, t);
            grupoPanel.alpha = Mathf.Lerp(alphaInicio, alphaFinal, t);
            yield return null;
        }
        panelAnimado.anchoredPosition = fin;
        grupoPanel.alpha = alphaFinal;
        rutinaPanel = null;
    }

    private IEnumerator AnimarSiguienteCliente(string nombre, int indice, int total)
    {
        if (tarjetaActual == null || tarjetaActual.raiz == null ||
            tarjetaEntrante == null || tarjetaEntrante.raiz == null)
        {
            // Sin las dos tarjetas configuradas, igual actualizamos el pedido.
            PrepararTarjeta(tarjetaActual, nombre, indice);
            if (contadorClientes != null) contadorClientes.text = $"{indice}/{total}";
            Reproducir(sonidoSiguienteCliente);
            yield break;
        }

        PrepararTarjeta(tarjetaEntrante, nombre, indice);
        tarjetaActual.raiz.anchoredPosition = Vector2.zero;
        tarjetaEntrante.raiz.anchoredPosition = Vector2.down * alturaTarjeta;
        tarjetaEntrante.raiz.gameObject.SetActive(true);
        Reproducir(sonidoSiguienteCliente);

        float tiempo = 0f;
        float duracion = Mathf.Max(0.01f, duracionCarrusel);
        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempo / duracion));
            tarjetaActual.raiz.anchoredPosition = Vector2.up * (alturaTarjeta * t);
            tarjetaEntrante.raiz.anchoredPosition = Vector2.up * (alturaTarjeta * (t - 1f));
            yield return null;
        }

        tarjetaActual.raiz.gameObject.SetActive(false);
        tarjetaActual.raiz.anchoredPosition = Vector2.down * alturaTarjeta;
        tarjetaEntrante.raiz.anchoredPosition = Vector2.zero;
        TarjetaCliente temporal = tarjetaActual;
        tarjetaActual = tarjetaEntrante;
        tarjetaEntrante = temporal;

        if (contadorClientes != null) contadorClientes.text = $"{indice}/{total}";
        rutinaCarrusel = null;
    }

    private IEnumerator AnimarFinClientes(int total)
    {
        Reproducir(sonidoSiguienteCliente);
        if (contadorClientes != null) contadorClientes.text = $"{total}/{total}";

        if (tarjetaActual != null && tarjetaActual.raiz != null)
        {
            float tiempo = 0f;
            float duracion = Mathf.Max(0.01f, duracionCarrusel);
            while (tiempo < duracion)
            {
                tiempo += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempo / duracion));
                tarjetaActual.raiz.anchoredPosition = Vector2.up * (alturaTarjeta * t);
                yield return null;
            }
        }

        yield return new WaitForSecondsRealtime(0.7f);
        rutinaCarrusel = null;
        Ocultar();
    }

    private IEnumerator AnimarFinClientesYAbrirObjetivo(string siguiente, int total)
    {
        Reproducir(sonidoSiguienteCliente);
        if (contadorClientes != null) contadorClientes.text = $"{total}/{total}";
        if (tarjetaActual != null && tarjetaActual.raiz != null)
        {
            Vector2 desde = tarjetaActual.raiz.anchoredPosition;
            float t = 0f;
            float duracion = Mathf.Max(0.01f, duracionCarrusel);
            while (t < duracion)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duracion));
                tarjetaActual.raiz.anchoredPosition = Vector2.Lerp(desde, Vector2.up * alturaTarjeta, a);
                yield return null;
            }
            tarjetaActual.raiz.gameObject.SetActive(false);
        }
        rutinaCarrusel = null;
        MostrarObjetivoSimple(siguiente);
    }

    private void PrepararTarjeta(TarjetaCliente tarjeta, string nombre, int indice)
    {
        if (tarjeta == null) return;
        TipoBebida bebida =
            bebidasPorCliente != null && indice >= 0 && indice < bebidasPorCliente.Length
                ? bebidasPorCliente[indice] : TipoBebida.Cerveza;

        if (tarjeta.nombre != null) tarjeta.nombre.text = nombre;
        if (tarjeta.bebida != null) tarjeta.bebida.text = bebida.ToString();
        if (tarjeta.iconoBebida != null)
        {
            Sprite sprite = bebida == TipoBebida.Cerveza ? iconoCerveza : iconoWhisky;
            tarjeta.iconoBebida.sprite = sprite;
            tarjeta.iconoBebida.enabled = sprite != null;
        }
    }

    private void Reproducir(AudioClip clip)
    {
        if (fuenteSonidos != null && clip != null)
            fuenteSonidos.PlayOneShot(clip);
    }
}
