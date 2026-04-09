using UnityEngine;

public class StaticEnemy : EnemyCore
{
    [Header("Configuración Estática")]
    public float damageToPlayer = 10f;
    public float detectionRadius = 2f;
    
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // Si el jugador se acerca demasiado sin haberlo eliminado
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < detectionRadius)
        {
            // Aca llamariamos a la función de daño del jugador o subir paranoia
            Debug.Log("El jugador está demasiado cerca de la aberración");
        }

        // Si lo estamos iluminando, podemos agregar un efecto visual de vibración
        if (isBeingIlluminated)
        {
            transform.position += Random.insideUnitSphere * 0.02f; 
        }
    }

    protected override void Die()
    {
        // Efecto visual: que se "derrita" o desaparezca con un sonido de alivio
        base.Die();
    }
}