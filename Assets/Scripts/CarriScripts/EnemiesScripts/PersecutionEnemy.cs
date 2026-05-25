using UnityEngine;
using UnityEngine.AI;

public class PersecutionEnemy : EnemyCore
{
    private NavMeshAgent agent;
    private Transform player;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            player = playerGO.transform;
        else
            Debug.LogWarning("[PersecutionEnemy] No se encontró un GameObject con tag 'Player'.");

        if (agent != null && !agent.isOnNavMesh)
            Debug.LogWarning("[PersecutionEnemy] El agente no está sobre un NavMesh. " +
                             "Bakeá el NavMesh en la escena (Window → AI → Navigation → Bake) " +
                             "o quitá el componente NavMeshAgent del prefab de sombras del Acto 2.");
    }

   void Update()
{
    if (health <= 0) return;
    if (agent == null || !agent.isOnNavMesh || player == null) return;

    // 1. Calculamos la distancia real primero, para que la linterna no la pise
    float distanciaAlJugador = Vector3.Distance(transform.position, player.position);

    // 2. CASO A: El enemigo llegó a Lucas (Distancia de ataque)
    if (distanciaAlJugador <= agent.stoppingDistance)
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero; // Destruye la inercia física
        agent.ResetPath();             // Borra la ruta para que no empuje

        // Forzamos a que mire a Lucas a la cara en la barra
        Vector3 direccionLook = (player.position - transform.position).normalized;
        direccionLook.y = 0;
        if (direccionLook != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direccionLook), Time.deltaTime * 15f);
        }

        Debug.Log("La Sombra está en posición y atacando.");
        return; // Ya llegó, no nos importa si lo iluminan o no, se queda ahí atacando
    }

    // 3. CASO B: Está lejos, pero Lucas lo está iluminando con la linterna (Se congela en el camino)
    if (isBeingIlluminated)
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero; // Evita que deslice mientras lo encandilás
        return;
    }

    // 4. CASO C: Está lejos y nadie lo ilumina (Persigue a Lucas)
    agent.isStopped = false;
    agent.SetDestination(player.position);
}
}