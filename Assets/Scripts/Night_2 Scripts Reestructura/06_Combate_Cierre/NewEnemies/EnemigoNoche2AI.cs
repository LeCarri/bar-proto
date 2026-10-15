using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemigo de Noche 2: aparece en Idle despues de Naniela,
/// persigue a Lucas al cruzar el trigger, y causa dano por contacto
/// mediante ContactoDanioNoche2 (componente separado, en un hijo trigger).
/// No utiliza la animacion Attack.
/// EnemyCore + SombraDesintegracion siguen gestionando dano y muerte.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemigoNoche2AI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animatorEnemigo;

    [Header("Animator - nombres exactos")]
    [SerializeField] private string estadoIdle = "Idle";
    [SerializeField] private string estadoRun = "Run";
    [Min(0f)] [SerializeField] private float transicionAnimaciones = 0.08f;

    [Header("Persecucion")]
    [Min(0.1f)] [SerializeField] private float velocidadCarrera = 4.5f;
    [Min(0.05f)] [SerializeField] private float intervaloActualizarDestino = 0.15f;
    [Min(0f)] [SerializeField] private float esperaAntesDePerseguir = 1f;

    [Header("Levantarse antes de perseguir (solo enemigo del suelo)")]
    [Tooltip("Activar solamente en el enemigo que comienza comiendo en el suelo.")]
    [SerializeField] private bool reproducirGetUp = false;
    [SerializeField] private string estadoGetUp = "GetUp";
    [Tooltip("Duracion REAL del clip GetUp en segundos. El enemigo permanece inmovil hasta que termina.")]
    [Min(0f)] [SerializeField] private float duracionGetUp = 1.5f;
    [Tooltip("Evita que el enemigo vuelva a su animacion de comer cuando alcanza a Lucas.")]
    [SerializeField] private bool mantenerRunAlLlegar = false;

    [Tooltip("Acercarse a Lucas, sin detenerse a varios metros para atacar.")]
    [Min(0f)] [SerializeField] private float distanciaFrenado = 0.1f;

    private NavMeshAgent agente;
    private Transform jugador;
    private Coroutine rutinaInicio;
    private bool preparadoEnIdle;
    private bool persiguiendo;
    private bool persecucionSolicitada;
    private float proximaActualizacionDestino;
    private string animacionActual;

    // El sensor de contacto solo hace dano durante la persecucion.
    public bool EstaPersiguiendo => persiguiendo && isActiveAndEnabled;

    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        if (animatorEnemigo == null)
            animatorEnemigo = GetComponentInChildren<Animator>(true);
    }

    public void PrepararAntesDelBano()
    {
        DetenerIA();
        gameObject.SetActive(false);
    }

    // Llamado por Act2ManagerDemo al terminar el evento de Naniela o desde el debug.
    public void AparecerEnIdle()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (agente == null)
            agente = GetComponent<NavMeshAgent>();
        if (animatorEnemigo == null)
            animatorEnemigo = GetComponentInChildren<Animator>(true);

        DetenerIA();
        preparadoEnIdle = true;
        agente.speed = velocidadCarrera;
        agente.stoppingDistance = distanciaFrenado;

        if (agente.enabled && agente.isOnNavMesh)
        {
            agente.isStopped = true;
            agente.ResetPath();
        }
        else
        {
            Debug.LogWarning("[EnemigoNoche2] Agente fuera del NavMesh al aparecer.", this);
        }

        if (animatorEnemigo != null)
        {
            animatorEnemigo.applyRootMotion = false;
            animatorEnemigo.Play(estadoIdle, 0, 0f);
            animacionActual = estadoIdle;
        }
        Debug.Log("[EnemigoNoche2] Aparecio en Idle esperando el trigger.", this);
    }

    // Llamado por TriggerPersecucionNoche2, sin modificar ese script.
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
        if (esperaAntesDePerseguir > 0f)
            yield return new WaitForSeconds(esperaAntesDePerseguir);

        // El segundo enemigo se incorpora del suelo ANTES de correr.
        // El agente sigue detenido: solo se anima la malla, sin desplazar la raiz.
        if (reproducirGetUp && animatorEnemigo != null)
        {
            int hashGetUp = Animator.StringToHash(estadoGetUp);
            int hashGetUpCompleto = Animator.StringToHash("Base Layer." + estadoGetUp);
            if (!animatorEnemigo.HasState(0, hashGetUp) &&
                !animatorEnemigo.HasState(0, hashGetUpCompleto))
            {
                Debug.LogError("[EnemigoNoche2] No existe el estado GetUp: " + estadoGetUp, this);
                rutinaInicio = null;
                persecucionSolicitada = false;
                yield break;
            }

            
                animatorEnemigo.Play(estadoGetUp, 0, 0f);
                animacionActual = estadoGetUp;

                Debug.Log("[EnemigoNoche2] Comenzo a levantarse.", this);

                // Esperar a que Unity actualice el Animator.
                yield return null;

                // Esperar hasta completar la animación GetUp.
                while (
                    animatorEnemigo.GetCurrentAnimatorStateInfo(0).IsName(estadoGetUp) &&
                    animatorEnemigo.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f
                )
                {
                    yield return null;
                }

                Debug.Log("[EnemigoNoche2] Termino de levantarse.", this);

        }

        rutinaInicio = null;

        if (!preparadoEnIdle || jugador == null || agente == null ||
            !agente.enabled || !agente.isOnNavMesh)
        {
            Debug.LogWarning("[EnemigoNoche2] No pudo iniciar la persecucion: revisa objetivo y NavMeshAgent.", this);
            yield break;
        }

        persiguiendo = true;
        agente.speed = velocidadCarrera;
        agente.stoppingDistance = distanciaFrenado;
        agente.isStopped = false;
        proximaActualizacionDestino = 0f;
        ReproducirEstado(estadoRun);
        Debug.Log("[EnemigoNoche2] Comenzo a perseguir a Lucas (dano por contacto).", this);
        // Reproducir voces cuando comienza la persecución
        if (Act2ManagerDemo.Instance != null)
        {
            Act2ManagerDemo.Instance.IniciarEnemyVoices();
        }
    }

    private void Update()
    {
        if (!persiguiendo || jugador == null || agente == null ||
            !agente.enabled || !agente.isOnNavMesh)
            return;

        if (Time.time >= proximaActualizacionDestino)
        {
            proximaActualizacionDestino = Time.time + intervaloActualizarDestino;
            agente.SetDestination(jugador.position);
        }

        // Si el agente llego hasta el jugador, puede quedarse quieto.
        // El sensor hijo sigue detectando contacto y aplicando dano.
        bool llego = !agente.pathPending &&
                     agente.hasPath &&
                     agente.remainingDistance <= distanciaFrenado + 0.15f;
        // El enemigo del suelo no debe volver a echarse a comer al alcanzar a Lucas.
        ReproducirEstado(llego && !mantenerRunAlLlegar ? estadoIdle : estadoRun);
    }

    public void DetenerIA()
    {
        if (rutinaInicio != null)
        {
            StopCoroutine(rutinaInicio);
            rutinaInicio = null;
        }

        preparadoEnIdle = false;
        persiguiendo = false;
        persecucionSolicitada = false;
        jugador = null;

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
        if (animatorEnemigo == null || animacionActual == nombreEstado)
            return;

        animatorEnemigo.CrossFadeInFixedTime(nombreEstado, transicionAnimaciones);
        animacionActual = nombreEstado;
    }
}
