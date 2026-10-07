using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SegundaCriaturaPared : MonoBehaviour
{
    [Header("Ruta por pared")]
    [Tooltip("SOLO puntos de pared. NO incluir el punto del piso.")]
    public Transform[] puntosRuta;

    [Header("Velocidad por punto")]
    [Tooltip("Velocidad usada para llegar a cada punto. El índice 0 no se usa.")]
    public float[] velocidadesPorPunto;

    [Tooltip("Pausa al llegar a cada punto. El índice 0 no se usa.")]
    public float[] pausasPorPunto;

    [Header("Movimiento pared")]
    public float velocidadBase = 2.2f;
    public float velocidadRotacion = 7f;
    public float distanciaLlegada = 0.05f;

    [Header("Salto instantáneo al piso")]
    public Transform puntoPiso;

    [Tooltip("Pequeña pausa antes de desaparecer de la pared y aparecer en el piso.")]
    public float pausaAntesDeBajar = 0.08f;

    public float radioBusquedaNavMesh = 1.5f;

    [Header("Visual especial de pared")]
    public Transform pivotVisualPared;
    public Transform visualOffset;

    [Tooltip("Posición local del VisualOffset cuando ya está en el piso.")]
    public Vector3 offsetVisualPiso = Vector3.zero;

    [Tooltip("Rotación local del PivotVisualPared cuando está de pie.")]
    public Vector3 rotacionVisualPiso = Vector3.zero;

    [Header("Persecución")]
    public NavMeshAgent navMeshAgent;
    public SombraWalkTest persecucion;

    [Header("Animación")]
    public Animator animator;
    public string parametroMovimiento = "Walk";

    [Tooltip("Adapta la velocidad de la animación a la velocidad de cada tramo.")]
    public bool adaptarVelocidadAnimacion = true;

    [Header("Colisión durante recorrido")]
    public Collider[] collidersFisicos;

    [Header("DEBUG - Preview de ruta")]
    [Range(0, 20)]
    public int puntoPreview = 0;

    [Header("Audio - Caminata por pared")]
    public AudioSource audioCaminataPared;

    [Range(0.5f, 2f)]
    public float pitchMinCaminata = 0.8f;

    [Range(0.5f, 2.5f)]
    public float pitchMaxCaminata = 1.6f;


    [Header("Contacto visual")]
    public Camera camaraJugador;
    public Transform puntoMiradaCriatura;

    [Range(0.01f, 0.5f)]
    public float toleranciaMirada = 0.18f;

    public Transform objetivoSacudida;
    public float duracionSacudidaMirada = 0.18f;
    public float fuerzaSacudidaMirada = 0.045f;
    public AudioSource audioImpactoMirada;

    public bool actualizarPreview = false;

    private bool secuenciaIniciada;

    private bool estaEnPared = false;
    private bool yaSacudioPorMirada = false;


    private void Awake()
    {
        // Mientras está en pared, NavMesh NO interviene.
        if (navMeshAgent != null)
            navMeshAgent.enabled = false;

        if (persecucion != null)
            persecucion.enabled = false;

        if (animator != null)
            animator.applyRootMotion = false;
    }


    public void IniciarSecuencia()
    {
        if (secuenciaIniciada)
            return;

        secuenciaIniciada = true;

        // Asegurarnos de que solamente este script
        // controle el movimiento mientras está en pared.
        if (navMeshAgent != null)
            navMeshAgent.enabled = false;

        if (persecucion != null)
            persecucion.enabled = false;


        // Sin colisión física durante la aparición.
        foreach (Collider col in collidersFisicos)
        {
            if (col != null)
                col.enabled = false;
        }

        estaEnPared = true;

        if (audioCaminataPared != null)
        {
            audioCaminataPared.Stop();
            audioCaminataPared.loop = true;
            audioCaminataPared.volume = 1f;
            audioCaminataPared.Play();
        }

        StartCoroutine(RecorrerPared());
    }


    private IEnumerator RecorrerPared()
    {
        if (puntosRuta == null ||
            puntosRuta.Length < 2)
        {
            Debug.LogError(
                "[SegundaCriaturaPared] Faltan puntos de pared."
            );

            yield break;
        }


        // ==========================================
        // SPAWN
        // ==========================================

        transform.position =
            puntosRuta[0].position;


        // SOLO rota el visual.
        if (pivotVisualPared != null)
        {
            pivotVisualPared.rotation =
                puntosRuta[0].rotation;
        }


        if (animator != null &&
            !string.IsNullOrEmpty(parametroMovimiento))
        {
            animator.SetBool(
                parametroMovimiento,
                true
            );
        }


        // ==========================================
        // RECORRIDO TIPO ARAÑA
        // ==========================================

        for (int i = 1; i < puntosRuta.Length; i++)
        {
            Transform destino =
                puntosRuta[i];

            if (destino == null)
                continue;


            float velocidadActual =
                ObtenerVelocidad(i);

            if (audioCaminataPared != null)
            {
                float proporcion =
                    velocidadActual /
                    Mathf.Max(0.01f, velocidadBase);

                audioCaminataPared.pitch =
                    Mathf.Clamp(
                        proporcion,
                        pitchMinCaminata,
                        pitchMaxCaminata
                    );

                if (!audioCaminataPared.isPlaying)
                    audioCaminataPared.Play();
            }


            // La animación acompaña los cambios
            // bruscos de velocidad.
            if (animator != null &&
                adaptarVelocidadAnimacion)
            {
                animator.speed =
                    Mathf.Clamp(
                        velocidadActual /
                        Mathf.Max(0.01f, velocidadBase),
                        0.5f,
                        2.5f
                    );
            }


            // ======================================
            // MOVERSE HACIA EL PUNTO
            // ======================================

            while (
                Vector3.Distance(
                    transform.position,
                    destino.position
                ) > distanciaLlegada
            )
            {
                // ROOT:
                // solamente posición.
                transform.position =
                    Vector3.MoveTowards(
                        transform.position,
                        destino.position,
                        velocidadActual *
                        Time.deltaTime
                    );


                // VISUAL:
                // solamente rotación.
                if (pivotVisualPared != null)
                {
                    pivotVisualPared.rotation =
                        Quaternion.Slerp(
                            pivotVisualPared.rotation,
                            destino.rotation,
                            velocidadRotacion *
                            Time.deltaTime
                        );
                }


                yield return null;
            }


            // Clavar exactamente el punto.
            transform.position =
                destino.position;


            if (pivotVisualPared != null)
            {
                pivotVisualPared.rotation =
                    destino.rotation;
            }


            // ======================================
            // PAUSA TIPO ARAÑA
            // ======================================

            float pausa =
                ObtenerPausa(i);

            if (pausa > 0f)
            {
                if (animator != null)
                    animator.speed = 0f;

                if (audioCaminataPared != null)
                    audioCaminataPared.Pause();

                yield return new WaitForSeconds(
                    pausa
                );

                if (animator != null)
                    animator.speed = 1f;

                if (audioCaminataPared != null)
                    audioCaminataPared.UnPause();
            }
        }


        // ==========================================
        // TERMINÓ LA PARED
        // ==========================================

        if (pausaAntesDeBajar > 0f)
        {
            yield return new WaitForSeconds(
                pausaAntesDeBajar
            );
        }


        yield return StartCoroutine(
            TransicionParpadeoAlPiso()
        );
    }

    private IEnumerator TransicionParpadeoAlPiso()
    {
        // ==========================================
        // MISMO FALLO DE LINTERNA DEL PRIMER EVENTO
        // ==========================================

        if (Act1Manager.Instance != null)
        {
            yield return StartCoroutine(
                Act1Manager.Instance.EjecutarFalloLinterna()
            );
        }


        // ==========================================
        // EN EL PUNTO DE MAYOR DISTORSIÓN:
        // PARED -> PISO INSTANTÁNEAMENTE
        // ==========================================

        yield return StartCoroutine(
            SaltarAlPiso()
        );


        // ==========================================
        // RECUPERACIÓN DEL MISMO VOLUME
        // ==========================================

        if (Act1Manager.Instance != null)
        {
            yield return StartCoroutine(
                Act1Manager.Instance.EjecutarRecuperacionLinterna()
            );
        }
    }


    private float ObtenerVelocidad(int indice)
    {
        if (velocidadesPorPunto != null &&
            indice >= 0 &&
            indice < velocidadesPorPunto.Length &&
            velocidadesPorPunto[indice] > 0f)
        {
            return velocidadesPorPunto[indice];
        }

        return velocidadBase;
    }


    private float ObtenerPausa(int indice)
    {
        if (pausasPorPunto != null &&
            indice >= 0 &&
            indice < pausasPorPunto.Length)
        {
            return Mathf.Max(
                0f,
                pausasPorPunto[indice]
            );
        }

        return 0f;
    }


    private IEnumerator SaltarAlPiso()
    {

        estaEnPared = false;

        if (audioCaminataPared != null)
        {
            audioCaminataPared.Stop();
            audioCaminataPared.pitch = 1f;
        }

        if (puntoPiso == null)
        {
            Debug.LogError(
                "[SegundaCriaturaPared] Punto Piso no asignado."
            );

            yield break;
        }


        // ==========================================
        // BUSCAR NAVMESH CERCA DEL PUNTO DE PISO
        // ==========================================

        NavMeshHit hit;

        bool encontroNavMesh =
            NavMesh.SamplePosition(
                puntoPiso.position,
                out hit,
                radioBusquedaNavMesh,
                NavMesh.AllAreas
            );


        if (!encontroNavMesh)
        {
            Debug.LogError(
                "[SegundaCriaturaPared] " +
                "No encontró NavMesh cerca de Punto Piso."
            );

            yield break;
        }


        // ==========================================
        // SNAP INSTANTÁNEO
        // PARED -> PISO
        // ==========================================

        transform.position =
            hit.position;


        // El root vuelve a una rotación normal:
        // solo conservamos Y.
        transform.rotation =
            Quaternion.Euler(
                0f,
                puntoPiso.eulerAngles.y,
                0f
            );


        // Quitar TODA la rotación especial de pared.
        if (pivotVisualPared != null)
        {
            pivotVisualPared.localRotation =
                Quaternion.Euler(
                    rotacionVisualPiso
                );
        }


        // Quitar el offset usado para pegarlo
        // visualmente a la pared.
        if (visualOffset != null)
        {
            visualOffset.localPosition =
                offsetVisualPiso;
        }


        // Volver a velocidad normal de animación.
        if (animator != null)
        {
            animator.speed = 1f;

            if (!string.IsNullOrEmpty(
                parametroMovimiento))
            {
                animator.SetBool(
                    parametroMovimiento,
                    false
                );
            }
        }


        yield return null;


        // ==========================================
        // VOLVER A ACTIVAR COLISIONES
        // ==========================================

        foreach (Collider col in collidersFisicos)
        {
            if (col != null)
                col.enabled = true;
        }


        // ==========================================
        // ACTIVAR NAVMESH
        // ==========================================

        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;

            navMeshAgent.Warp(
                hit.position
            );

            navMeshAgent.isStopped = false;
        }


        // ==========================================
        // PERSECUCIÓN NORMAL
        // ==========================================

        if (persecucion != null)
        {
            persecucion.enabled = true;
        }


        Debug.Log(
            "[SegundaCriaturaPared] " +
            "Ya está en el piso. Persecución normal."
        );
    }


    private void OnValidate()
    {
        if (!actualizarPreview)
            return;

        if (puntosRuta == null ||
            puntosRuta.Length == 0)
            return;


        int indice =
            Mathf.Clamp(
                puntoPreview,
                0,
                puntosRuta.Length - 1
            );


        if (puntosRuta[indice] == null)
            return;


        // El root solamente se mueve.
        transform.position =
            puntosRuta[indice].position;


        // El visual es el que rota.
        if (pivotVisualPared != null)
        {
            pivotVisualPared.rotation =
                puntosRuta[indice].rotation;
        }
    }

    private void Update()
    {
        if (!estaEnPared ||
            yaSacudioPorMirada ||
            camaraJugador == null ||
            puntoMiradaCriatura == null)
        {
            return;
        }

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
            yaSacudioPorMirada = true;

            // La música recupera de golpe la tensión.
            if (Act1Manager.Instance != null)
            {
                Act1Manager.Instance
                    .RestaurarMusicaSegundoEnemigo();
            }

            StartCoroutine(
                SacudidaContactoVisual()
            );
        }
    }

    private IEnumerator SacudidaContactoVisual()
    {
        if (audioImpactoMirada != null)
        {
            audioImpactoMirada.Stop();
            audioImpactoMirada.Play();
        }

        if (objetivoSacudida == null)
            yield break;

        Vector3 posicionOriginal =
            objetivoSacudida.localPosition;

        float tiempo = 0f;

        while (tiempo < duracionSacudidaMirada)
        {
            tiempo += Time.deltaTime;

            objetivoSacudida.localPosition =
                posicionOriginal +
                Random.insideUnitSphere *
                fuerzaSacudidaMirada;

            yield return null;
        }

        objetivoSacudida.localPosition =
            posicionOriginal;
    }
}