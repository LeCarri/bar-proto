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
        health -= amount * Time.deltaTime;
        isBeingIlluminated = true; // Flag para efectos visuales

        if (health <= 0) Die();
    }

    protected virtual void Die()
    {
        // Aquí podés instanciar partículas de humo o simplemente destruir
        Destroy(gameObject);
    }

    // Resetear el flag cada frame para que el jugador deba mantener la luz
    protected void LateUpdate() => isBeingIlluminated = false;
}
