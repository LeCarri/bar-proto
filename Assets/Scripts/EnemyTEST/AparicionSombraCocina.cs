using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class AparicionSombraCocina : MonoBehaviour
{
    [Header("Jugador")]
    public Camera camaraJugador;

    [Header("Linterna")]
    public Light luzLinterna;

    [Tooltip("Cantidad de apagados/prendidos antes del apagón final.")]
    public int cantidadTitileos = 4;

    [Tooltip("Tiempo entre cada cambio de estado de la linterna.")]
    public float intervaloTitileo = 0.12f;

    [Tooltip("Tiempo que queda completamente a oscuras.")]
    public float tiempoOscuridad = 1f;

    [Header("Audio linterna")]
    public AudioSource audioTitileo;

    [Header("Criatura")]
    public GameObject criatura;

    [Tooltip("Punto que el jugador debe mirar para activar el susto.")]
    public Transform puntoMiradaCriatura;

    [Range(0.01f, 0.5f)]
    public float toleranciaMirada = 0.18f;

    [Tooltip("Cuánto tiempo debe mantener la mirada. 0 = instantáneo.")]
    public float tiempoMiradaNecesario = 0f;

    [Header("Audio aparición")]
    public AudioSource sonidoAparicion;

    [Header("Persecución")]
    public SombraWalkTest scriptPersecucion;
    public NavMeshAgent navMeshAgent;
    [Header("FX - Combate")]
    public Volume volumenCombate;
    [Header("FX - Sacudida")]
    public Transform objetivoSacudida;

    public float duracionSacudida = 0.25f;

    public float fuerzaSacudida = 0.08f;

    [Range(0f, 1f)]
    public float pesoVolumeCombate = 1f;

    public float velocidadEntradaVolumeCombate = 8f;

    private Coroutine rutinaVolumeCombate;

    private bool secuenciaIniciada;
    private bool esperandoMirada;
    private bool revelada;

    private float tiempoMirando;

    private void Start()
    {
        // La criatura empieza escondida.
        if (criatura != null)
            criatura.SetActive(false);

        if (audioTitileo != null)
            audioTitileo.playOnAwake = false;

        if (sonidoAparicion != null)
            sonidoAparicion.playOnAwake = false;
    }

    private void Update()
    {
        if (!esperandoMirada || revelada)
            return;

        DetectarMiradaCriatura();
    }

    // Este método se llama desde alLlegarAC.
    public void IniciarAparicion()
    {
        if (secuenciaIniciada)
            return;

        secuenciaIniciada = true;

        StartCoroutine(SecuenciaAparicion());
    }

    private IEnumerator SecuenciaAparicion()
    {
        if (luzLinterna == null)
        {
            Debug.LogError(
                "[AparicionSombra] Falta referencia a la luz de la linterna."
            );

            yield break;
        }

        // -------------------------------------------
        // TITILEO
        // -------------------------------------------

        if (audioTitileo != null)
        {
            audioTitileo.loop = true;
            audioTitileo.Play();
        }

        for (int i = 0; i < cantidadTitileos; i++)
        {
            luzLinterna.enabled = false;

            yield return new WaitForSeconds(
                Mathf.Max(0.01f, intervaloTitileo)
            );

            luzLinterna.enabled = true;

            yield return new WaitForSeconds(
                Mathf.Max(0.01f, intervaloTitileo)
            );
        }

        // -------------------------------------------
        // APAGÓN FINAL
        // -------------------------------------------

        luzLinterna.enabled = false;

        if (audioTitileo != null)
        {
            audioTitileo.Stop();
        }

        // -------------------------------------------
        // LA CRIATURA APARECE EN LA OSCURIDAD
        // -------------------------------------------

        if (criatura != null)
            criatura.SetActive(true);

        // Todavía no dejamos que persiga.
        if (scriptPersecucion != null)
            scriptPersecucion.enabled = false;

        if (navMeshAgent != null)
            navMeshAgent.enabled = false;

        // Tiempo completamente a oscuras.
        yield return new WaitForSeconds(
            Mathf.Max(0f, tiempoOscuridad)
        );

        // -------------------------------------------
        // VUELVE LA LINTERNA
        // -------------------------------------------

        luzLinterna.enabled = true;

        // Ahora esperamos que el jugador la mire.
        esperandoMirada = true;
        tiempoMirando = 0f;

        Debug.Log(
            "[AparicionSombra] Criatura visible. Esperando mirada."
        );
    }

    private void DetectarMiradaCriatura()
    {
        if (camaraJugador == null ||
            puntoMiradaCriatura == null)
            return;

        Vector3 viewport =
            camaraJugador.WorldToViewportPoint(
                puntoMiradaCriatura.position
            );

        bool mirando =
            viewport.z > 0f &&
            Mathf.Abs(viewport.x - 0.5f) <= toleranciaMirada &&
            Mathf.Abs(viewport.y - 0.5f) <= toleranciaMirada;

        if (mirando)
        {
            tiempoMirando += Time.deltaTime;

            if (tiempoMirando >= tiempoMiradaNecesario)
                RevelarCriatura();
        }
        else
        {
            tiempoMirando = 0f;
        }
    }

    private void RevelarCriatura()
    {
        if (revelada)
            return;

        revelada = true;
        esperandoMirada = false;

        // SONIDO FUERTE
        if (sonidoAparicion != null)
        {
            sonidoAparicion.loop = false;
            sonidoAparicion.Play();
        }

        // SACUDIDA BRUSCA
        StartCoroutine(SacudirCamara());

        // EFECTO VISUAL DEL COMBATE
        ActivarVolumeCombate();

        Debug.Log(
            "[AparicionSombra] Jugador vio la criatura."
        );

        // Comienza persecución.
        if (navMeshAgent != null)
            navMeshAgent.enabled = true;

        if (scriptPersecucion != null)
            scriptPersecucion.enabled = true;
    }

        private void ActivarVolumeCombate()
    {
        if (volumenCombate == null)
            return;

        if (rutinaVolumeCombate != null)
            StopCoroutine(rutinaVolumeCombate);

        rutinaVolumeCombate = StartCoroutine(
            TransicionarVolumeCombate(pesoVolumeCombate)
        );
    }

    private IEnumerator TransicionarVolumeCombate(float objetivo)
    {
        while (Mathf.Abs(volumenCombate.weight - objetivo) > 0.01f)
        {
            volumenCombate.weight = Mathf.MoveTowards(
                volumenCombate.weight,
                objetivo,
                velocidadEntradaVolumeCombate * Time.deltaTime
            );

            yield return null;
        }

        volumenCombate.weight = objetivo;
        rutinaVolumeCombate = null;
    }
        private IEnumerator SacudirCamara()
    {
        if (objetivoSacudida == null)
            yield break;

        Vector3 posicionOriginal =
            objetivoSacudida.localPosition;

        float tiempo = 0f;

        while (tiempo < duracionSacudida)
        {
            tiempo += Time.deltaTime;

            Vector3 desplazamiento =
                Random.insideUnitSphere * fuerzaSacudida;

            objetivoSacudida.localPosition =
                posicionOriginal + desplazamiento;

            yield return null;
        }

        objetivoSacudida.localPosition =
            posicionOriginal;
    }
}