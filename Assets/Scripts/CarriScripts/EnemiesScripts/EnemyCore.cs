using UnityEngine;
using UnityEngine.Events;

// Este va en todos los enemigos.
// Maneja vida, daño y muerte.
// Los efectos especiales son opcionales mediante otros componentes.

public class EnemyCore : MonoBehaviour
{
    public float health = 100f;
    public float deathSpeed = 20f;
    [Header("Eventos")]
    public UnityEvent alMorir;

    protected bool isBeingIlluminated = false;

    // Vida inicial para calcular porcentaje de desintegración
    private float maxHealth;

    // Efecto opcional para las sombras nuevas
    private SombraDesintegracion sombraDesintegracion;

    // Candado para evitar procesar muerte varias veces
    private bool isDead = false;

    protected virtual void Awake()
    {
        // Guardamos la vida inicial
        maxHealth = health;

        // Si este enemigo tiene el nuevo sistema,
        // lo encontramos automáticamente.
        sombraDesintegracion =
            GetComponent<SombraDesintegracion>();
    }

    public void TakeDamage(float amount)
    {
        if (isDead)
            return;

        health -= amount;

        health = Mathf.Clamp(
            health,
            0f,
            maxHealth
        );

        isBeingIlluminated = true;

        // ==============================
        // DESINTEGRACIÓN VISUAL
        // ==============================

        if (sombraDesintegracion != null)
        {
            sombraDesintegracion.ActualizarDesintegracion(
                health,
                maxHealth
            );
        }

        Debug.Log(
            "Daño recibido. Vida actual: " + health
        );

        // ==============================
        // MUERTE
        // ==============================

        if (health <= 0f)
        {
            isDead = true;

            if (sombraDesintegracion != null)
            {
                sombraDesintegracion
                    .ForzarDesintegracionCompleta();
            }

            Die();
        }
    }


    protected virtual void Die()
    {
        // Ejecutar acciones asociadas a la muerte.
        alMorir?.Invoke();

        Act1Manager manager =
            Object.FindAnyObjectByType<Act1Manager>();

        if (manager != null)
        {
            manager.RegistarEnemigoEliminado();
        }

        // Evita falsas colisiones antes de destruirse.
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.enabled = false;

        Destroy(gameObject);
    }


    // El jugador debe mantener la luz sobre el enemigo.
    protected void LateUpdate()
    {
        isBeingIlluminated = false;
    }
}