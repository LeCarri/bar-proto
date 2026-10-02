using UnityEngine;
using UnityEngine.AI;

public class SombraWalkTest : MonoBehaviour
{
    [Header("Referencias")]
    public Transform jugador;

    [Header("Persecución")]
    public float distanciaDetencion = 1.5f;

    private NavMeshAgent agente;

    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (agente == null)
        {
            Debug.LogError("[SombraWalkTest] No hay NavMeshAgent.");
            enabled = false;
            return;
        }

        agente.stoppingDistance = distanciaDetencion;
    }

    private void Update()
    {
        if (jugador == null)
            return;

        if (agente == null || !agente.isOnNavMesh)
            return;

        agente.SetDestination(jugador.position);
    }
}