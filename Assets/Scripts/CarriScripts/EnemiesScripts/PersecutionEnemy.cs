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

        // Si no se asignó manualmente el Animator desde el Inspector,
        // intenta buscarlo automáticamente en este mismo GameObject.
        if (animator == null)
            animator = GetComponent<Animator>();

        // Si no se asignó manualmente el AudioSource desde el Inspector,
        // intenta buscarlo automáticamente en este mismo GameObject.
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.transform;
            Debug.Log($"[PersecutionEnemy][Diagnóstico] Player encontrado correctamente: '{playerGO.name}' en '{playerGO.scene.name}'. Enemy: '{gameObject.name}'.", this);
        }
        else
        {
            Debug.LogWarning("[PersecutionEnemy] No se encontró un GameObject con tag 'Player'.");
            Debug.LogWarning($"[PersecutionEnemy][Diagnóstico] PlayerHealth.Instance es {(PlayerHealth.Instance != null ? "EXISTE" : "NULL")} al iniciar. Enemy: '{gameObject.name}'.", this);
        }
    }

    void Update()
    {
        if (health <= 0)
        {
            SetWalking(false);
            return;
        }

        if (agent == null || !agent.isOnNavMesh || player == null)
        {
            SetWalking(false);
            return;
        }

        // 1. Calculamos la distancia real primero
        float distanciaAlJugador = Vector3.Distance(transform.position, player.position);
        LogNavigationDiagnostics(distanciaAlJugador, distanciaAlJugador <= agent.stoppingDistance);

        // ========================================================
        // 👑 PRIORIDAD 1: EL ENEMIGO LLEGÓ A VOS (FRENADO ABSOLUTO)
        // ========================================================
        if (distanciaAlJugador <= agent.stoppingDistance)
        {
            Debug.Log($"[PersecutionEnemy][Diagnóstico] ENTRA al bloque de ataque. distance={distanciaAlJugador:F3}, stoppingDistance={agent.stoppingDistance:F3}, enemy='{gameObject.name}'.", this);

            // --- ACÁ METIMOS LAS LÍNEAS NUEVAS PARA TRABAR LA VELOCIDAD ---
            agent.isStopped = true;
            agent.velocity = Vector3.zero;            // Clava los frenos físicos
            if (agent.isOnNavMesh) agent.ResetPath(); // Borra la ruta para que no patine

            // Si llegó al jugador, ya no debería estar en animación de caminar.
            SetWalking(false);

            // Forzamos a que mire a Lucas a la cara en la barra
            Vector3 direccionLook = (player.position - transform.position).normalized;
            direccionLook.y = 0;
            if (direccionLook != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direccionLook), Time.deltaTime * 15f);
            }

            // Lógica del sartenazo de daño
            if (Time.time >= nextAttackTime)
            {
                Debug.Log($"[PersecutionEnemy][Diagnóstico] Cooldown listo. Ejecutando intento de ataque. Time={Time.time:F3}, nextAttackTime={nextAttackTime:F3}, enemy='{gameObject.name}'.", this);

                // Nueva parte: dispara animación y sonido de ataque
                ReproducirAtaqueVisualYSonoro();

                PlayerHealth playerHealth = PlayerHealth.Instance;
                Debug.Log($"[PersecutionEnemy][Diagnóstico] PlayerHealth.Instance es {(playerHealth != null ? "EXISTE" : "NULL")}. Enemy: '{gameObject.name}'.", this);

                if (playerHealth != null)
                {
                    Debug.Log($"[PersecutionEnemy][Diagnóstico] Llamando a RecibirDanio({damage}). Enemy: '{gameObject.name}'.", this);
                    playerHealth.RecibirDanio(damage);
                    Debug.Log($"[Ataque] La Sombra te encajó {damage} de daño.");
                }

                nextAttackTime = Time.time + attackCooldown;
            }
            else
            {
                Debug.Log($"[PersecutionEnemy][Diagnóstico] ENTRA al bloque de ataque, pero NO ejecuta ataque por cooldown. Time={Time.time:F3}, nextAttackTime={nextAttackTime:F3}, enemy='{gameObject.name}'.", this);
            }

            wasBeingIlluminated = false;

            return; // Corta acá para que no haga nada más mientras te pega
        }

        // ========================================================
        // 🔦 PRIORIDAD 2: SI ESTÁ LEJOS Y LO ILUMINÁS, SE CONGELA
        // ========================================================
        if (isBeingIlluminated)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            // Si está congelada por la luz, no debería estar caminando.
            SetWalking(false);

            // Nueva parte: sonido opcional cuando la luz la frena.
            // Solo suena una vez al entrar en estado iluminado, no en cada frame.
            if (!wasBeingIlluminated)
            {
                ReproducirSonido(freezeClip, freezeVolume);
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

        // Nueva parte: animación y pasos mientras persigue.
        bool estaCaminando = agent.velocity.magnitude > minSpeedForFootsteps;

        SetWalking(estaCaminando);

        if (estaCaminando)
        {
            ReproducirPasos();
        }
    }

    private void ReproducirAtaqueVisualYSonoro()
    {
        // Dispara la animación de ataque si existe un Animator asignado.
        if (string.IsNullOrEmpty(attackTriggerName))
        {
            Debug.LogWarning($"[PersecutionEnemy][Diagnóstico] NO ejecuta SetTrigger porque attackTriggerName está vacío. Enemy: '{gameObject.name}'.", this);
            return;
        }

        if (CanUseAnimator("SetTrigger", attackTriggerName))
        {
            Debug.Log($"[PersecutionEnemy][Diagnóstico] Ejecutando animator.SetTrigger('{attackTriggerName}') en Animator '{animator.name}' / GameObject '{animator.gameObject.name}'. Enemy: '{gameObject.name}'.", animator);
            animator.SetTrigger(attackTriggerName);
        }

        // Reproduce el sonido de ataque si hay AudioSource y clip asignados.
        if (audioSource != null && attackClip != null)
        {
            Debug.Log($"[PersecutionEnemy][Diagnóstico] Reproduciendo attackClip '{attackClip.name}' con AudioSource en '{audioSource.gameObject.name}'. Enemy: '{gameObject.name}'.", audioSource);
        }
        else
        {
            Debug.LogWarning($"[PersecutionEnemy][Diagnóstico] NO reproduce attackClip. audioSource={(audioSource != null ? audioSource.gameObject.name : "NULL")}, attackClip={(attackClip != null ? attackClip.name : "NULL")}, enemy='{gameObject.name}'.", this);
        }

        ReproducirSonido(attackClip, attackVolume);
    }

    private void LogNavigationDiagnostics(float distanciaAlJugador, bool entraAtaque)
    {
        if (!debugAttackDiagnostics) return;
        if (Time.time < nextDebugLogTime) return;

        nextDebugLogTime = Time.time + debugLogInterval;

        Debug.Log(
            $"[PersecutionEnemy][Diagnóstico] Estado persecución | " +
            $"enemy='{gameObject.name}' | " +
            $"player='{player.name}' | " +
            $"distance={distanciaAlJugador:F3} | " +
            $"remainingDistance={agent.remainingDistance:F3} | " +
            $"stoppingDistance={agent.stoppingDistance:F3} | " +
            $"pathPending={agent.pathPending} | " +
            $"hasPath={agent.hasPath} | " +
            $"isStopped={agent.isStopped} | " +
            $"entraAtaque={entraAtaque}",
            this
        );
    }

    private void SetWalking(bool value)
    {
        // Controla el parámetro booleano del Animator para caminar.
        // El Animator debe tener un Bool con el mismo nombre que walkingBoolName.
        if (string.IsNullOrEmpty(walkingBoolName)) return;
        if (!CanUseAnimator("SetBool", walkingBoolName)) return;

        animator.SetBool(walkingBoolName, value);
    }

    private bool CanUseAnimator(string operation, string parameterName)
    {
        if (animator == null)
        {
            Debug.LogWarning($"[PersecutionEnemy] No hay Animator asignado para {operation}('{parameterName}') en '{gameObject.name}'.", this);
            return false;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"[PersecutionEnemy] El Animator '{animator.name}' en '{animator.gameObject.name}' no tiene AnimatorController para {operation}('{parameterName}'). Enemy: '{gameObject.name}'.", animator);
            return false;
        }

        if (!animator.isActiveAndEnabled)
        {
            Debug.LogWarning($"[PersecutionEnemy] El Animator '{animator.name}' en '{animator.gameObject.name}' no está activo/habilitado para {operation}('{parameterName}'). Enemy: '{gameObject.name}'.", animator);
            return false;
        }

        return true;
    }

    private void ReproducirPasos()
    {
        if (footstepClip == null) return;
        if (audioSource == null) return;

        // Evita que el paso suene todos los frames.
        if (Time.time < nextFootstepTime) return;

        ReproducirSonido(footstepClip, footstepVolume);
        nextFootstepTime = Time.time + footstepInterval;
    }

    private void ReproducirSonido(AudioClip clip, float volume)
    {
        if (clip == null) return;
        if (audioSource == null) return;

        // Pequeña variación para que los pasos y ataques no suenen siempre idénticos.
        if (randomizePitch)
        {
            audioSource.pitch = Random.Range(pitchMin, pitchMax);
        }
        else
        {
            audioSource.pitch = 1f;
        }

        audioSource.PlayOneShot(clip, volume);
    }
}
