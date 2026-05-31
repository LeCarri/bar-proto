using UnityEngine;
using UnityEngine.AI;

public class PersecutionEnemy : EnemyCore
{
    private NavMeshAgent agent;
    private Transform player;

    [Header("Configuración de Ataque")]
    [SerializeField] private float damage = 50f;           // El daño configurado
    [SerializeField] private float attackCooldown = 1.5f;  // Tiempo de espera entre golpes
    private float nextAttackTime = 0f;                     // Temporizador interno

    [Header("Configuración de Animación")]
    [SerializeField] private Animator animator;
    [SerializeField] private string walkingBoolName = "IsWalking";
    [SerializeField] private string attackTriggerName = "Attack";

    [Header("Configuración de Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioClip freezeClip;

    [Header("Volúmenes de Audio")]
    [Range(0f, 1f)]
    [SerializeField] private float attackVolume = 0.9f;

    [Range(0f, 1f)]
    [SerializeField] private float footstepVolume = 0.45f;

    [Range(0f, 1f)]
    [SerializeField] private float freezeVolume = 0.5f;

    [Header("Configuración de Pasos")]
    [SerializeField] private float footstepInterval = 0.55f;
    [SerializeField] private float minSpeedForFootsteps = 0.15f;
    private float nextFootstepTime = 0f;

    [Header("Variación de Pitch")]
    [SerializeField] private bool randomizePitch = true;
    [SerializeField] private float pitchMin = 0.9f;
    [SerializeField] private float pitchMax = 1.1f;

    [Header("Diagnóstico Temporal")]
    [SerializeField] private bool debugAttackDiagnostics = true;
    [SerializeField] private float debugLogInterval = 0.5f;
    private float nextDebugLogTime = 0f;

    private bool wasBeingIlluminated = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.transform;
            Debug.Log($"[PersecutionEnemy][Diagnóstico] Player encontrado correctamente: '{playerGO.name}'.", this);
        }
        else
        {
            Debug.LogWarning("[PersecutionEnemy] No se encontró un GameObject con tag 'Player'.");
        }
    }

    void Update()
    {
        if (health <= 0)
        {
            SetWalking(false);
            if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
            return;
        }

        if (agent == null || !agent.isOnNavMesh || player == null)
        {
            SetWalking(false);
            return;
        }

        // 1. Calculamos la distancia real
        float distanciaAlJugador = Vector3.Distance(transform.position, player.position);
        LogNavigationDiagnostics(distanciaAlJugador, distanciaAlJugador <= agent.stoppingDistance);

        // ========================================================
        // 👑 PRIORIDAD 1: EL ENEMIGO LLEGÓ A VOS (ATAQUE Y ENFOQUE)
        // ========================================================
        if (distanciaAlJugador <= agent.stoppingDistance)
        {
            // Frenamos el agente de forma limpia sin borrar el Path de navegación
            agent.isStopped = true;
            agent.velocity = Vector3.zero; 

            SetWalking(false);

            // Rotación suave hacia Lucas
            Vector3 direccionLook = (player.position - transform.position).normalized;
            direccionLook.y = 0;
            if (direccionLook != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direccionLook), Time.deltaTime * 15f);
            }

            // Ejecución del sartenazo de daño bajo Cooldown
            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown; // Se asigna el cooldown antes para blindar el frame

                ReproducirAtaqueVisualYSonoro();

                PlayerHealth playerHealth = PlayerHealth.Instance;
                if (playerHealth != null)
                {
                    playerHealth.RecibirDanio(damage);
                    Debug.Log($"[Ataque] La Sombra te encajó {damage} de daño.");
                }
            }

            wasBeingIlluminated = false;
            return; 
        }

        // ========================================================
        // 🔦 PRIORIDAD 2: SI ESTÁ LEJOS Y LO ILUMINÁS, SE CONGELA
        // ========================================================
        if (isBeingIlluminated)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            SetWalking(false);

            if (!wasBeingIlluminated)
            {
                ReproducirSonido(freezeClip, freezeVolume, false); // No cambiamos el pitch para el freeze tétrico
                wasBeingIlluminated = true;
            }

            return;
        }

        // ========================================================
        // 🏃‍♂️ PRIORIDAD 3: SI ESTÁ LEJOS Y A OSCURAS, PERSIGUE
        // ========================================================
        wasBeingIlluminated = false;

        agent.isStopped = false;
        agent.SetDestination(player.position);

        bool estaCaminando = agent.velocity.magnitude > minSpeedForFootsteps;
        SetWalking(estaCaminando);

        if (estaCaminando)
        {
            ReproducirPasos();
        }
    }

    private void ReproducirAtaqueVisualYSonoro()
    {
        if (!string.IsNullOrEmpty(attackTriggerName) && CanUseAnimator("SetTrigger", attackTriggerName))
        {
            animator.SetTrigger(attackTriggerName);
        }

        // Reproduce el sonido de ataque fijando un pitch normal/estable
        ReproducirSonido(attackClip, attackVolume, false); 
    }

    private void ReproducirPasos()
    {
        if (footstepClip == null || audioSource == null) return;
        if (Time.time < nextFootstepTime) return;

        // A los pasos sí les metemos variación aleatoria para que no cansen el oído
        ReproducirSonido(footstepClip, footstepVolume, true);
        nextFootstepTime = Time.time + footstepInterval;
    }

    private void ReproducirSonido(AudioClip clip, float volume, bool usarVariacionPitch)
    {
        if (clip == null || audioSource == null) return;

        // Modificamos el pitch de forma controlada según el tipo de sonido
        if (randomizePitch && usarVariacionPitch)
        {
            audioSource.pitch = Random.Range(pitchMin, pitchMax);
        }
        else
        {
            audioSource.pitch = 1f; // Reseteo total para ataques y efectos críticos
        }

        audioSource.PlayOneShot(clip, volume);
    }

    private void LogNavigationDiagnostics(float distanciaAlJugador, bool entraAtaque)
    {
        if (!debugAttackDiagnostics) return;
        if (Time.time < nextDebugLogTime) return;

        nextDebugLogTime = Time.time + debugLogInterval;

        Debug.Log(
            $"[PersecutionEnemy] Estado: Perseguiendo | " +
            $"distancia={distanciaAlJugador:F2} | " +
            $"stoppingDistance={agent.stoppingDistance:F2} | " +
            $"ataqueListo={Time.time >= nextAttackTime}", 
            this
        );
    }

    private void SetWalking(bool value)
    {
        if (string.IsNullOrEmpty(walkingBoolName)) return;
        if (!CanUseAnimator("SetBool", walkingBoolName)) return;

        animator.SetBool(walkingBoolName, value);
    }

    private bool CanUseAnimator(string operation, string parameterName)
    {
        if (animator == null || animator.runtimeAnimatorController == null || !animator.isActiveAndEnabled)
            return false;

        return true;
    }
}