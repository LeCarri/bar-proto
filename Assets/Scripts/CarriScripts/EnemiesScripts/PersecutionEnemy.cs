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

    [Tooltip("Distancia real a la que el enemigo puede atacar. Debe ser parecida o apenas mayor al Stopping Distance.")]
    [SerializeField] private float attackRange = 10f;

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

        RevisarConfiguracionInicial();
    }

void Update()
{
    if (health <= 0 || player == null || agent == null || !agent.isOnNavMesh)
    {
        FrenarEnemigoPorCompleto();
        return;
    }

    float distanciaAlJugador = Vector3.Distance(transform.position, player.position);

    // Primero revisamos si está siendo iluminado.
    // Esto tiene prioridad sobre atacar.
    if (isBeingIlluminated)
    {
        ProcesarEstadoCongelado();
        return;
    }

    // Después recién revisamos si está en rango de ataque.
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
        if (footstepAudioSource == null)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"[{gameObject.name}] No hay Footstep AudioSource asignado.");
            }

            return;
        }

        if (footstepSequenceClip == null)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"[{gameObject.name}] No hay clip de pasos asignado.");
            }

            return;
        }

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
        if (sfxAudioSource == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No hay SFX AudioSource asignado. No se puede reproducir {nombreDebug}.");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Falta asignar el clip de audio para {nombreDebug}.");
            return;
        }

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
            footstepAudioSource.playOnAwake = false;
        }

        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
            sfxAudioSource.playOnAwake = false;
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

    // Linear hace que el sonido 3D no se apague tan brusco.
    // Para pasos suele funcionar mejor que Logarithmic.
    source.rolloffMode = AudioRolloffMode.Linear;

    source.minDistance = minAudioDistance;
    source.maxDistance = maxAudioDistance;
    source.volume = 1f;
    source.pitch = 1f;
    source.mute = false;

    // Muy importante: si estaba sonando al iniciar, lo cortamos.
    if (source.isPlaying)
    {
        source.Stop();
    }
}    private void RevisarConfiguracionInicial()
    {
        if (!showDebugLogs) return;

        if (animator == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No hay Animator asignado.");
        }

        if (footstepSequenceClip == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Falta Footstep Sequence Clip.");
        }

        if (attackClip == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Falta Attack Clip.");
        }

        if (freezeClip == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Falta Freeze Clip.");
        }

        if (footstepAudioSource != null)
        {
            Debug.Log($"[{gameObject.name}] Footstep AudioSource listo. SpatialBlend: {footstepAudioSource.spatialBlend}, MaxDistance: {footstepAudioSource.maxDistance}");
        }

        if (sfxAudioSource != null)
        {
            Debug.Log($"[{gameObject.name}] SFX AudioSource listo. SpatialBlend: {sfxAudioSource.spatialBlend}, MaxDistance: {sfxAudioSource.maxDistance}");
        }
    }
}