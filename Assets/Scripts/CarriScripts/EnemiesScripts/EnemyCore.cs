using UnityEngine;

// Este va en todos los enemigos, es para manejar el dema de la vida, el daño, que mueran etc.
// Las caracteristicas especiales de cada uno seran scripts de la clase EnemyCore

public class EnemyCore : MonoBehaviour
{
    public float health = 100f;
    public float deathSpeed = 20f; // Qué tan rápido se desvanece
    protected bool isBeingIlluminated = false;

    // 🛡️ CANDADO DE SEGURIDAD ABSOLUTO
    private bool isDead = false;

    public void TakeDamage(float amount)
    {
        // Si ya empezó el proceso de muerte, ignoramos cualquier daño extra en este frame
        if (isDead) return;

        health -= amount;
        isBeingIlluminated = true;

        Debug.Log("Daño recibido. Vida actual: " + health);

        if (health <= 0) 
        {
            isDead = true; // Cerramos el candado inmediatamente antes de procesar la muerte
            Die();
        }
    }

    protected virtual void Die()
    {
        Act1Manager manager = Object.FindAnyObjectByType<Act1Manager>();
        if (manager != null) 
        {
            manager.EnemigoEliminado();
        }

        // Desactivamos el Collider para evitar falsas colisiones antes de ser destruido
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject);
    }

    // Resetear el flag cada frame para que el jugador deba mantener la luz
    protected void LateUpdate() => isBeingIlluminated = false;
}