using UnityEngine;
using UnityEngine.AI;

public class PersecutionEnemy : EnemyCore
{
    private NavMeshAgent agent;
    private Transform player;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (health > 0)
        {
            // Si no lo estamos iluminando, camina hacia el jugador
            // Si lo iluminamos, se queda quieto (opcional, para dar ventaja)
            agent.isStopped = isBeingIlluminated; 
            
            if (!agent.isStopped)
                agent.SetDestination(player.position);
        }
    }
}