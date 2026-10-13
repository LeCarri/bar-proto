using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// IA independiente para el enemigo nuevo de Noche 2.
/// EnemyCore (en este mismo objeto) se encarga de la vida y de recibir la linterna.
/// NO agregar PersecutionEnemy, SombraWalkTest u otra IA al mismo enemigo.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemigoNoche2AI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animatorEnemigo;

    [Header("Animator - nombres exactos de estados")]
    [SerializeField] private string estadoIdle = "Idle";
    [SerializeField] private string estadoRun = "Run";
    [SerializeField] private string estadoAttack = "Attack";
    [Min(0f)] [SerializeField] private float transicionAnimaciones = 0.08f;

    [Header("Persecucion")]
    [Min(0.1f)] [SerializeField] private float velocidadCarrera = 4.5f;
    [Min(0.05f)] [SerializeField] private float intervaloActualizarDestino = 0.15f;
    [Min(0f)] [SerializeField] private float esperaAntesDePerseguir = 1f;

    [Header("Ataque")]
    [Min(0.1f)] [SerializeField] private float distanciaAtaque = 1.6f;
    [Min(0f)] [SerializeField] private float tiempoHastaGolpe = 0.45f;
    [Min(0.05f)] [SerializeField] private float duracionAnimacionAtaque = 1.0f;
    [Min(0f)] [SerializeField] private float pausaEntreAtaques = 1.0f;
    [Min(1)] [SerializeField] private int danioAlJugador = 1;

    private NavMeshAgent agente;
    private Transform jugador;
    private Coroutine rutinaInicio;
    private Coroutine rutinaAtaque;
    private bool preparadoEnIdle;
    private bool persiguiendo;
    private bool atacando;
    private bool persecucionSolicitada;
    private float proximaActualizacionDestino;
    private float siguienteAtaqueDisponible;
    private string animacionActual;

    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        if (animatorEnemigo == null)
            animatorEnemigo = GetComponentInChildren<Animator>(true);
    }

    /// <summary>Al entrar a la noche, el enemigo aun no debe aparecer.</summary>
    public void PrepararAntesDelBano()
    {
        DetenerRutinas();
        preparadoEnIdle = false;
        persiguiendo = false;
        atacando = false;
        persecucionSolicitada = false;
        jugador = null;

        if (agente != null && agente.enabled && agente.isOnNavMesh)
        {
            agente.isStopped = true;
            agente.ResetPath();
        }

        gameObject.SetActive(false);
    }

    /// <summary>Se llama al terminar Naniela (tambien desde el debug).</summary>
    public void AparecerEnIdle()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (agente == null)
            agente = GetComponent<NavMeshAgent>();
        if (animatorEnemigo == null)
            animatorEnemigo = GetComponentInChildren<Animator>(true);

        DetenerRutinas();
        preparadoEnIdle = true;
        persiguiendo = false;
        atacando = false;
        persecucionSolicitada = false;
        jugador = null;
        siguienteAtaqueDisponible = 0f;

        agente.speed = velocidadCarrera;
        agente.stoppingDistance = distanciaAtaque * 0.85f;
        if (agente.isOnNavMesh)
        {
            agente.isStopped = true;
            agente.ResetPath();
        }
        else
        {
            Debug.LogWarning("[EnemigoNoche2] El NavMeshAgent NO esta sobre el NavMesh. Verifica el Bake y la posicion del enemigo.", this);
        }

        if (animatorEnemigo != null)
        {
            animatorEnemigo.applyRootMotion = false;
            animatorEnemigo.Play(estadoIdle, 0, 0f);
            animacionActual = estadoIdle;
        }
        Debug.Log("[EnemigoNoche2] Aparecio en Idle esperando al jugador.", this);
    }

    /// <summary>Se llama desde el trigger; arranca la persecucion despues del retraso.</summary>
    public void ActivarPersecucion(Transform objetivo)
    {
        if (!preparadoEnIdle || persecucionSolicitada || objetivo == null)
            return;

        persecucionSolicitada = true;
        jugador = objetivo;
        rutinaInicio = StartCoroutine(EsperarYPerseguir());
    }

    private IEnumerator EsperarYPerseguir()
    {
        yield return new WaitForSeconds(esperaAntesDePerseguir);
        rutinaInicio = null;

        if (!preparadoEnIdle || jugador == null || agente == null || !agente.isOnNavMesh)
        {
            Debug.LogWarning("[EnemigoNoche2] No pudo comenzar la persecucion: revisa jugador o NavMeshAgent.", this);
            yield break;
        }

        persiguiendo = true;
        agente.isStopped = false;
        agente.speed = velocidadCarrera;
        proximaActualizacionDestino = 0f;
        ReproducirEstado(estadoRun);
        Debug.Log("[EnemigoNoche2] Comenzo a perseguir a Lucas.", this);
    }

    private void Update()
    {
        if (!persiguiendo || atacando || jugador == null || agente == null || !agente.isOnNavMesh)
            return;

        if (Time.time >= proximaActualizacionDestino)
        {
            proximaActualizacionDestino = Time.time + intervaloActualizarDestino;
            agente.SetDestination(jugador.position);
        }

        // Usar la distancia por el camino NavMesh, no solamente la distancia recta:
        // evita atacar a Lucas a traves de una pared o de un mueble.
        bool cercaPorCamino = !agente.pathPending &&
            agente.pathStatus == NavMeshPathStatus.PathComplete &&
            agente.remainingDistance <= distanciaAtaque;

        if (cercaPorCamino)
        {
            if (!agente.isStopped)
                agente.isStopped = true;

            if (Time.time >= siguienteAtaqueDisponible)
            {
                rutinaAtaque = StartCoroutine(Atacar());
            }
            else
            {
                ReproducirEstado(estadoIdle);
            }
        }
        else
        {
            if (agente.isStopped)
                agente.isStopped = false;
            ReproducirEstado(estadoRun);
        }
    }

    private IEnumerator Atacar()
    {
        atacando = true;
        agente.isStopped = true;
        agente.velocity = Vector3.zero;

        if (animatorEnemigo != null)
        {
            animatorEnemigo.Play(estadoAttack, 0, 0f);
            animacionActual = estadoAttack;
        }

        yield return new WaitForSeconds(tiempoHastaGolpe);

        // Volver a verificar distancia: Lucas pudo escapar durante la animacion.
        if (jugador != null && agente != null && agente.isOnNavMesh)
        {
            // La proximidad directa y la ruta completa son necesarias para golpear.
            float distanciaReal = Vector3.Distance(transform.position, jugador.position);
            bool rutaValida = agente.pathStatus == NavMeshPathStatus.PathComplete &&
                               agente.remainingDistance <= distanciaAtaque + 0.4f;
            if (distanciaReal <= distanciaAtaque + 0.4f && rutaValida)
            {
                if (PlayerHealth.Instance != null)
                    PlayerHealth.Instance.RecibirDanio(danioAlJugador);
                else
                    Debug.LogWarning("[EnemigoNoche2] No se encontro PlayerHealth.Instance.", this);
            }
        }

        yield return new WaitForSeconds(Mathf.Max(0f, duracionAnimacionAtaque - tiempoHastaGolpe));
        siguienteAtaqueDisponible = Time.time + pausaEntreAtaques;
        atacando = false;
        rutinaAtaque = null;
        if (persiguiendo)
            ReproducirEstado(estadoRun);
    }

    /// <summary>Opcional: conectar a EnemyCore.alMorir para frenar la IA antes de destruirlo.</summary>
    public void DetenerIA()
    {
        DetenerRutinas();
        preparadoEnIdle = false;
        persiguiendo = false;
        atacando = false;
        persecucionSolicitada = false;
        if (agente != null && agente.enabled && agente.isOnNavMesh)
        {
            agente.isStopped = true;
            agente.ResetPath();
        }
    }

    public void DesactivarEnemigo()
    {
        DetenerIA();
        gameObject.SetActive(false);
    }

    private void ReproducirEstado(string nombreEstado)
    {
        if (animatorEnemigo == null || animacionActual == nombreEstado) return;
        animatorEnemigo.CrossFadeInFixedTime(nombreEstado, transicionAnimaciones);
        animacionActual = nombreEstado;
    }

    private void DetenerRutinas()
    {
        if (rutinaInicio != null)
        {
            StopCoroutine(rutinaInicio);
            rutinaInicio = null;
        }
        if (rutinaAtaque != null)
        {
            StopCoroutine(rutinaAtaque);
            rutinaAtaque = null;
        }
    }
}
