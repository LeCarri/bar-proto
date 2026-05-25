using UnityEngine;
using UnityEngine.AI;

public class PersecutionEnemy : EnemyCore
{
    private NavMeshAgent agent;
    private Transform player;

    [Header("Configuración de Ataque")]
    [SerializeField] private float damage = 50f;        // El daño que definió tu amigo
    [SerializeField] private float attackCooldown = 1.5f; // Tiempo de espera entre golpes
    private float nextAttackTime = 0f;                 // Temporizador interno

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            player = playerGO.transform;
        else
            Debug.LogWarning("[PersecutionEnemy] No se encontró un GameObject con tag 'Player'.");
    }

   void Update()
    {
        if (health <= 0) return;
        if (agent == null || !agent.isOnNavMesh || player == null) return;

        // 1. Calculamos la distancia real PRIMERO
        float distanciaAlJugador = Vector3.Distance(transform.position, player.position);

        // ========================================================
        // 👑 PRIORIDAD 1: SI YA LLEGÓ A LUCAS, ATACA SIEMPRE
        // ========================================================
        if (distanciaAlJugador <= agent.stoppingDistance)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero; 
            agent.ResetPath();             

            // Forzamos a que mire a Lucas a la cara
            Vector3 direccionLook = (player.position - transform.position).normalized;
            direccionLook.y = 0;
            if (direccionLook != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direccionLook), Time.deltaTime * 15f);
            }

            if (Time.time >= nextAttackTime)
            {
                if (PlayerHealth.Instance != null)
                {
                    PlayerHealth.Instance.RecibirDanio(damage);
                    Debug.Log($"[Ataque] La Sombra te encajó {damage} de daño.");
                }

                nextAttackTime = Time.time + attackCooldown;
            }

            return; // Excluye los casos de movimiento y congelamiento por luz porque ya está encima tuyo
        }

        // ========================================================
        // 🔦 PRIORIDAD 2: SI ESTÁ LEJOS Y LO ILUMINÁS, SE CONGELA
        // ========================================================
        if (isBeingIlluminated)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero; 
            return;
        }

        // ========================================================
        // 🏃‍♂️ PRIORIDAD 3: SI ESTÁ LEJOS Y A OSCURAS, PERSIGUE
        // ========================================================
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }
}