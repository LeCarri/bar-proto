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

        agent.isStopped = isBeingIlluminated;

        if (!agent.isStopped)
            agent.SetDestination(player.position);
    }
}