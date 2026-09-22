using System.Collections;
using UnityEngine;

public class ServicioWhiskyVisual : MonoBehaviour
{
    [Header("Vaso visual")]
    [SerializeField] private GameObject vasoVisual;
    [SerializeField] private Transform liquido;

    [Header("Chorro")]
    [SerializeField] private LineRenderer chorro;
    [SerializeField] private Transform bocaBotella;
    [SerializeField] private Transform puntoCaida;

    [Header("Forma del chorro")]
    [SerializeField] private float curvaturaChorro = 0.08f;

    [Header("Llenado")]
    [SerializeField] private float duracionLlenado = 1f;
    [SerializeField] private float escalaYVacio = 0.001f;
    [SerializeField] private float escalaYLleno = 1f;

    private Coroutine rutinaLlenado;
    private Vector3 escalaLiquidoLlena;

    private float alturaMallaLiquido = 1f;

    private void Awake()
    {
        if (liquido != null)
        {
            escalaLiquidoLlena = liquido.localScale;

            // Lo dejamos preparado vacío.
            PonerNivelLiquido(escalaYVacio);

            // Pero NO queremos verlo hasta que empiece a servir.
            liquido.gameObject.SetActive(false);
        }

        if (chorro != null)
        {
            chorro.positionCount = 3;
            chorro.useWorldSpace = true;
            chorro.enabled = false;
        }

        if (vasoVisual != null)
            vasoVisual.SetActive(false);
    }

    private void Update()
    {
        if (chorro == null ||
            !chorro.enabled ||
            bocaBotella == null ||
            puntoCaida == null)
            return;

        Vector3 inicio = bocaBotella.position;
        Vector3 final = puntoCaida.position;

        Vector3 medio =
            Vector3.Lerp(inicio, final, 0.5f);

        // Curva hacia abajo.
        medio += Vector3.down * curvaturaChorro;

        chorro.SetPosition(0, inicio);
        chorro.SetPosition(1, medio);
        chorro.SetPosition(2, final);
    }

    public void MostrarVaso()
    {
        Debug.Log("VISUAL: MOSTRAR VASO");

        if (vasoVisual != null)
            vasoVisual.SetActive(true);

        // El vaso aparece completamente vacío.
        if (liquido != null)
        {
            PonerNivelLiquido(escalaYVacio);
            liquido.gameObject.SetActive(false);
        }

        if (chorro != null)
            chorro.enabled = false;
    }

    public void IniciarVertido()
    {
        Debug.Log("VISUAL: INICIA VERTIDO");

        if (chorro != null)
            chorro.enabled = true;

        if (liquido != null)
        {
            PonerNivelLiquido(escalaYVacio);
            liquido.gameObject.SetActive(true);
        }

        if (rutinaLlenado != null)
            StopCoroutine(rutinaLlenado);

        rutinaLlenado =
            StartCoroutine(LlenarVaso());
    }

    public void DetenerVertido()
    {
        Debug.Log("VISUAL: DETIENE VERTIDO");

        // Se corta el chorro.
        if (chorro != null)
            chorro.enabled = false;

        // También dejamos de llenar.
        if (rutinaLlenado != null)
        {
            StopCoroutine(rutinaLlenado);
            rutinaLlenado = null;
        }

        // El vaso queda lleno y visible.
        if (liquido != null)
        {
            liquido.gameObject.SetActive(true);
            PonerNivelLiquido(escalaYLleno);
        }
    }

    public void OcultarVaso()
    {
        Debug.Log("VISUAL: OCULTAR VASO");

        if (chorro != null)
            chorro.enabled = false;

        if (rutinaLlenado != null)
        {
            StopCoroutine(rutinaLlenado);
            rutinaLlenado = null;
        }

        if (liquido != null)
        {
            PonerNivelLiquido(escalaYVacio);
            liquido.gameObject.SetActive(false);
        }

        if (vasoVisual != null)
            vasoVisual.SetActive(false);
    }

    private IEnumerator LlenarVaso()
    {
        float tiempo = 0f;

        while (tiempo < duracionLlenado)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(
                tiempo / duracionLlenado
            );

            float nivel = Mathf.Lerp(
                escalaYVacio,
                escalaYLleno,
                t
            );

            PonerNivelLiquido(nivel);

            yield return null;
        }

        PonerNivelLiquido(escalaYLleno);

        rutinaLlenado = null;
    }

    private void PonerNivelLiquido(float nivel)
{
    if (liquido == null)
        return;

    // Conservamos exactamente la escala original en X y Z.
    Vector3 nuevaEscala = escalaLiquidoLlena;

    // Solo cambia la altura.
    nuevaEscala.y =
        escalaLiquidoLlena.y * nivel;

    liquido.localScale = nuevaEscala;

    // NO modificamos posición ni rotación.
}
}