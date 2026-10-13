
using UnityEngine;
using UnityEngine.Events;

public class EnemyCore : MonoBehaviour
{
    [Header("Vida")]
    public float health = 100f;
    public float deathSpeed = 20f;

    [Header("Eventos")]
    public UnityEvent alMorir;

    protected bool isBeingIlluminated = false;

    private float maxHealth;

    private SombraDesintegracion sombraDesintegracion;

    private bool isDead = false;

    protected virtual void Awake()
    {
        maxHealth = Mathf.Max(1f, health);

        sombraDesintegracion =
            GetComponent<SombraDesintegracion>();
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        health = Mathf.Clamp(
            health - amount,
            0f,
            maxHealth
        );

        isBeingIlluminated = true;

        // Actualizar la desintegración visual.
        if (sombraDesintegracion != null)
        {
            sombraDesintegracion.ActualizarDesintegracion(
                health,
                maxHealth
            );
        }

        Debug.Log(
            "[EnemyCore] Daño recibido. Vida actual: " +
            health
        );

        if (health <= 0f)
        {
            isDead = true;

            // Desintegración definitiva.
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
        Debug.Log(
            "[EnemyCore] Enemigo derrotado: " +
            gameObject.name
        );

        // Cada enemigo tiene su propio evento
        // de muerte conectado desde el Inspector.
        alMorir?.Invoke();

        // Desactivar colisiones.
        Collider[] colliders =
            GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // Destruir el enemigo.
        Destroy(gameObject);
    }

    protected void LateUpdate()
    {
        isBeingIlluminated = false;
    }
}
