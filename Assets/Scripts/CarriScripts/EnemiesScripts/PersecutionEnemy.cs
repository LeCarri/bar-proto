using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class PersecutionEnemy : EnemyCore
{
    private NavMeshAgent agent;
    private Transform player;
    private AudioSource audioSource;

    [Header("Configuración de Combate")]
    [SerializeField] private float damage = 25f; 
    [SerializeField] private float attackCooldown = 1.5f; 
    private float nextAttackTime = 0f; 

    [Header("Componentes Visuales")]
    [SerializeField] private Animator animator;

    [Header("Archivos de Audio (Clips)")]
    [SerializeField] private AudioClip footstepSequenceClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip freezeClip;

    [Header("Mezcla de Sonido")]
    [Range(0f, 1f)] [SerializeField] private float footstepVolume = 0.4f;
    [Range(0f, 1f)] [SerializeField] private float attackVolume = 0.8f;
    [Range(0f, 1f)] [SerializeField] private float freezeVolume = 0.6f;

    [Header("Ajuste de Ritmo (Sincronización)")]
    [Tooltip("Modifica la velocidad del audio de pasos. Menos de 1 es más lento y pesado.")]
    [Range(0.4f, 1.2f)] [SerializeField] private float footstepSpeed = 0.75f; 
    
    [Tooltip("Distancia máxima a la que Lucas puede escuchar a este enemigo.")]
    [SerializeField] private float maxAudioDistance = 50f;

    private bool wasBeingIlluminated = false;

    void Start()
    {
        // 1. Inicializamos componentes del mismo GameObject
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        if (animator == null) animator = GetComponent<Animator>();

        // 2. CONFIGURACIÓN BLINDADA DEL AUDIOSOURCE (A prueba de errores del grupo)
        audioSource.playOnAwake = false;
        audioSource.loop = false; 
        audioSource.spatialBlend = 1.0f; // Forzamos audio 3D para el posicionamiento táctico
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.maxDistance = maxAudioDistance; // Evitamos que suene en todo el mapa si está lejos

        // 3. Buscamos a Lucas de forma segura por Tag
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.transform;
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] ¡CRÍTICO: No se encontró al jugador! Asegurate de que el personaje de Lucas tenga asignado el Tag 'Player' en el Inspector.");
        }
    }

    void Update()
    {
        // Regla de oro: Si no hay condiciones para actuar o el enemigo murió, apagamos todo
        if (health <= 0 || player == null || !agent.isOnNavMesh)
        {
            FrenarEnemigoPorCompleto();
            return;
        }

        float distanciaAlJugador = Vector3.Distance(transform.position, player.position);

        // ========================================================
        // MAQUINA DE ESTADOS INTEGRADA (Prioridades de comportamiento)
        // ========================================================

        // ESTADO 1: EN RANGO DE ATAQUE
        if (distanciaAlJugador <= agent.stoppingDistance)
        {
            ProcesarEstadoAtaque();
            return;
        }

        // ESTADO 2: CONGELADO POR LA LINTERNA
        if (isBeingIlluminated)
        {
            ProcesarEstadoCongelado();
            return;
        }

        // ESTADO 3: PERSECUCIÓN EN LA OSCURIDAD
        ProcesarEstadoPersecucion();
    }

    // ========================================================
    // LÓGICA DE LOS ESTADOS
    // ========================================================

    private void ProcesarEstadoPersecucion()
    {
        wasBeingIlluminated = false;
        agent.isStopped = false;
        agent.SetDestination(player.position);

        // Verificamos si el NavMesh se está desplazando realmente
        bool estaMoviendose = agent.velocity.magnitude > 0.15f;
        SetWalkingAnimation(estaMoviendose);
        
        GestionarBuclePasos(estaMoviendose);
    }

    private void ProcesarEstadoCongelado()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        SetWalkingAnimation(false);
        GestionarBuclePasos(false); // Al congelarse, los pasos se cortan en seco

        if (!wasBeingIlluminated)
        {
            ReproducirEfectoImpacto(freezeClip, freezeVolume);
            wasBeingIlluminated = true;
        }
    }

    private void ProcesarEstadoAtaque()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero; 
        SetWalkingAnimation(false);
        GestionarBuclePasos(false); // Si está pegando, no está caminando

        // Rotación ultra suave para que la sombra siempre encare a Lucas en la barra
        Vector3 direccionLook = (player.position - transform.position).normalized;
        direccionLook.y = 0;
        if (direccionLook != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direccionLook), Time.deltaTime * 15f);
        }

        // Control de cadencia de golpes (Cooldown)
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            
            if (animator != null) animator.SetTrigger("Attack");
            ReproducirEfectoImpacto(attackClip, attackVolume);

            PlayerHealth playerHealth = PlayerHealth.Instance;
            if (playerHealth != null)
            {
                playerHealth.RecibirDanio(damage);
            }
        }

        wasBeingIlluminated = false;
    }

    // ========================================================
    // GESTIÓN DE AUDIO Y ANIMACIÓN (Módulos limpios)
    // ========================================================

    private void GestionarBuclePasos(bool activar)
    {
        if (footstepSequenceClip == null || audioSource == null) return;

        if (activar)
        {
            // Si el enemigo corre, seteamos la pista larga en bucle con el pitch lento
            audioSource.clip = footstepSequenceClip;
            audioSource.loop = true;
            audioSource.volume = footstepVolume;
            audioSource.pitch = footstepSpeed; 

            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            // Si frena, atacando o congelado, apagamos el canal de pasos
            if (audioSource.isPlaying && audioSource.clip == footstepSequenceClip)
            {
                audioSource.Stop();
            }
        }
    }

    private void ReproducirEfectoImpacto(AudioClip clip, float volumen)
    {
        if (clip == null || audioSource == null) return;

        // PlayOneShot permite superponer el grito/golpe/congelado sin romper el canal principal
        audioSource.PlayOneShot(clip, volumen);
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
}