using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PersecutionEnemy : EnemyCore
{
    private NavMeshAgent agent;
    private Transform player;

    [Header("Configuración de Combate")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Tooltip("Distancia a la que atacará. DEBE ser un valor pequeño (ej: 1.5 a 2.5) acorde al cuerpo a cuerpo.")]
    [SerializeField] private float attackRange = 2f; 

    private float nextAttackTime = 0f;

    [Header("Componentes Visuales")]
    [SerializeField] private Animator animator;

    [Header("Archivos de Audio")]
    [SerializeField] private AudioClip footstepSequenceClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip freezeClip;

    [Header("Audio Sources")]
    [Tooltip("AudioSource exclusivo para pasos en loop.")]
    [SerializeField] private AudioSource footstepAudioSource;

    [Tooltip("AudioSource exclusivo para ataque, freeze y sonidos cortos.")]
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("Mezcla de Sonido")]
    [Range(0f, 1f)] [SerializeField] private float footstepVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float attackVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float freezeVolume = 1f;

    [Header("Ajuste de Ritmo")]
    [Tooltip("Modifica la velocidad del audio de pasos. Menos de 1 es más lento y pesado.")]
    [Range(0.4f, 1.2f)] [SerializeField] private float footstepSpeed = 0.75f;

    [Header("Audio 3D")]
    [Tooltip("Para probar sonidos, podés poner Spatial Blend en 0 desde el Inspector. Para juego final, dejalo en 1.")]
    [Range(0f, 1f)] [SerializeField] private float spatialBlend = 1f;

    [SerializeField] private float minAudioDistance = 2f;
    [SerializeField] private float maxAudioDistance = 35f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private bool wasBeingIlluminated = false;
    private bool yaNotificoMuerte = false;

    [Header("Integración Noche 1")]
    [Tooltip("Marcar en TRUE solo para la sombra dentro del depósito (Tutorial).")]
    [SerializeField] private bool esPrimeraSombraTutorial = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        PrepararAudioSources();
        BuscarJugador();
        RevisarConfiguracionInicial();
    }

    private void OnEnable()
    {
        // Aseguramos que se enganche al NavMesh al reaparecer o activarse
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (agent != null && NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2.5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            agent.isStopped = false;
        }

        // Si el jugador no se había encontrado en Start (o se activó tarde), reintentamos
        if (player == null)
        {
            BuscarJugador();
        }

        AsegurarPosicionEnNavMesh();
    }

   public void AsegurarPosicionEnNavMesh()
{
    if (agent == null) agent = GetComponent<NavMeshAgent>();

    if (agent != null)
    {
        agent.enabled = false;

        // Aumentamos a 20f para que encuentre la malla aunque haya mucha diferencia de Y
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 20.0f, NavMesh.AllAreas))
        {
            transform.position = hit.position; // Se teletransporta exacto a la superficie azul
            agent.enabled = true;
            agent.Warp(hit.position);
            
            if (showDebugLogs)
            {
                Debug.Log($"[{gameObject.name}] ¡Enganchado al NavMesh con éxito en {hit.position}!");
            }
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Ni siquiera a 20m se encontró NavMesh.");
        }
    }
}

    private void BuscarJugador()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");

        if (playerGO != null)
        {
            player = playerGO.transform;

            if (showDebugLogs)
            {
                Debug.Log($"[{gameObject.name}] Player encontrado: {playerGO.name}");
            }
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] No se encontró ningún GameObject con tag 'Player'.");
        }
    }

    void Update()
{
    // Depuración de bloqueo
    if (player == null)
    {
        Debug.LogWarning($"[{gameObject.name}] Detenido: Player es NULL.");
        FrenarEnemigoPorCompleto();
        return;
    }

    if (agent == null)
    {
        Debug.LogWarning($"[{gameObject.name}] Detenido: NavMeshAgent es NULL.");
        FrenarEnemigoPorCompleto();
        return;
    }

    if (!agent.isOnNavMesh)
    {
        Debug.LogWarning($"[{gameObject.name}] Detenido: ¡El agente NO está sobre el NavMesh!");
        FrenarEnemigoPorCompleto();
        return;
    }

    if (health <= 0)
    {
        Debug.LogWarning($"[{gameObject.name}] Detenido: La vida es <= 0.");
        FrenarEnemigoPorCompleto();
        return;
    }

    float distanciaAlJugador = Vector3.Distance(transform.position, player.position);

    if (isBeingIlluminated)
    {
        ProcesarEstadoCongelado();
        return;
    }

    if (distanciaAlJugador <= attackRange)
    {
        ProcesarEstadoAtaque();
        return;
    }

    ProcesarEstadoPersecucion();
}

    private void ProcesarEstadoPersecucion()
    {
        wasBeingIlluminated = false;

        agent.isStopped = false;
        agent.SetDestination(player.position);

        bool estaMoviendose = agent.velocity.magnitude > 0.05f;

        SetWalkingAnimation(estaMoviendose);
        GestionarBuclePasos(estaMoviendose);
    }

    private void ProcesarEstadoCongelado()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        SetWalkingAnimation(false);
        GestionarBuclePasos(false);

        if (!wasBeingIlluminated)
        {
            ReproducirSFX(freezeClip, freezeVolume, "Freeze");
            wasBeingIlluminated = true;
        }
    }

    private void ProcesarEstadoAtaque()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        SetWalkingAnimation(false);
        GestionarBuclePasos(false);

        MirarAlJugador();

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            if (animator != null)
            {
                animator.ResetTrigger("Attack");
                animator.SetTrigger("Attack");

                if (showDebugLogs)
                {
                    Debug.Log($"[{gameObject.name}] Trigger Attack enviado.");
                }
            }

            ReproducirSFX(attackClip, attackVolume, "Attack");

            PlayerHealth playerHealth = PlayerHealth.Instance;

            if (playerHealth != null)
            {
                playerHealth.RecibirDanio(damage);

                if (showDebugLogs)
                {
                    Debug.Log($"[{gameObject.name}] Daño aplicado: {damage}");
                }
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] PlayerHealth.Instance es NULL.");
            }
        }

        wasBeingIlluminated = false;
    }

    private void GestionarBuclePasos(bool activar)
    {
        if (footstepAudioSource == null || footstepSequenceClip == null) return;

        if (activar)
        {
            if (footstepAudioSource.clip != footstepSequenceClip)
            {
                footstepAudioSource.clip = footstepSequenceClip;
            }

            footstepAudioSource.loop = true;
            footstepAudioSource.volume = footstepVolume;
            footstepAudioSource.pitch = footstepSpeed;

            if (!footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Play();

                if (showDebugLogs)
                {
                    Debug.Log($"[{gameObject.name}] Reproduciendo pasos: {footstepSequenceClip.name}");
                }
            }
        }
        else
        {
            if (footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Stop();

                if (showDebugLogs)
                {
                    Debug.Log($"[{gameObject.name}] Pasos detenidos.");
                }
            }
        }
    }

    private void ReproducirSFX(AudioClip clip, float volumen, string nombreDebug)
    {
        if (sfxAudioSource == null || clip == null) return;

        sfxAudioSource.pitch = 1f;
        sfxAudioSource.volume = 1f;
        sfxAudioSource.PlayOneShot(clip, volumen);

        if (showDebugLogs)
        {
            Debug.Log($"[{gameObject.name}] Reproduciendo SFX {nombreDebug}: {clip.name} | Volumen: {volumen}");
        }
    }

    private void MirarAlJugador()
    {
        Vector3 direccionLook = player.position - transform.position;
        direccionLook.y = 0f;

        if (direccionLook.sqrMagnitude > 0.01f)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionLook.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.deltaTime * 15f);
        }
    }

    private void SetWalkingAnimation(bool value)
    {
        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.SetBool("IsWalking", value);
        }
    }

    private void FrenarEnemigoPorCompleto()
    {
        SetWalkingAnimation(false);
        GestionarBuclePasos(false);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }

    private void PrepararAudioSources()
    {
        if (footstepAudioSource == null)
        {
            footstepAudioSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
        }

        ConfigurarAudioSource3D(footstepAudioSource, true);
        ConfigurarAudioSource3D(sfxAudioSource, false);
    }

    private void ConfigurarAudioSource3D(AudioSource source, bool esLoop)
    {
        if (source == null) return;

        source.playOnAwake = false;
        source.loop = esLoop;
        source.spatialBlend = spatialBlend;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = minAudioDistance;
        source.maxDistance = maxAudioDistance;

        if (source.isPlaying)
        {
            source.Stop();
        }
    }

    private void RevisarConfiguracionInicial()
    {
        if (!showDebugLogs) return;

        if (animator == null) Debug.LogWarning($"[{gameObject.name}] No hay Animator asignado.");
        if (footstepSequenceClip == null) Debug.LogWarning($"[{gameObject.name}] Falta Footstep Sequence Clip.");
        if (attackClip == null) Debug.LogWarning($"[{gameObject.name}] Falta Attack Clip.");
        if (freezeClip == null) Debug.LogWarning($"[{gameObject.name}] Falta Freeze Clip.");
    }

    public void NotificarMuerteAlManager()
    {
        if (yaNotificoMuerte) return;
        yaNotificoMuerte = true;

        if (Act1Manager.Instance == null) return;

        if (esPrimeraSombraTutorial)
        {
            Debug.Log($"[{gameObject.name}] Primera sombra derrotada. Avanzando tutorial.");
            Act1Manager.Instance.PrimeraSombraDerrotada();
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Sombra de salón eliminada.");
            Act1Manager.Instance.RegistarEnemigoEliminado();
        }
    }

    // Al destruirse o desactivarse tras morir
    private void OnDisable()
    {
        if (health <= 0)
        {
            NotificarMuerteAlManager();
        }
    }
}