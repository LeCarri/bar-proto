using UnityEngine;

// Este va en todos los enemigos, es para manejar el dema de la vida, el daño, que mueran etc.
// Las caracteristicas especiales de cada uno seran scripts de la clase EnemyCore

public class EnemyCore : MonoBehaviour
{
    public float health = 100f;
    public float deathSpeed = 20f; // Qué tan rápido se desvanece
    protected bool isBeingIlluminated = false;

public void TakeDamage(float amount)
{
    health -= amount;
    isBeingIlluminated = true;

    Debug.Log("Daño recibido. Vida actual: " + health);

    if (health <= 0) Die();
}
    protected virtual void Die()
    {
        Act1Manager manager = Object.FindAnyObjectByType<Act1Manager>();
        if (manager != null) manager.EnemigoEliminado();

        Destroy(gameObject);
    }

    // Resetear el flag cada frame para que el jugador deba mantener la luz
    protected void LateUpdate() => isBeingIlluminated = false;
}
