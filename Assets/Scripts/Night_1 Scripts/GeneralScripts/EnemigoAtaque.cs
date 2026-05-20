using UnityEngine;

public class EnemigoAtaque : MonoBehaviour
{
    [Header("Configuración de Ataque")]
    public float cantidadDanio = 20f; // Cuánta vida le saca a Lucas

    // Esta función de Unity se ejecuta automáticamente cuando el enemigo choca con algo
    private void OnCollisionEnter(Collision collision)
    {
        // 1. Preguntamos: ¿Lo que acabo de chocar tiene el Tag "Player"?
        if (collision.gameObject.CompareTag("Player"))
        {
            // 2. Si es el jugador, le "pedimos" su componente de vida
            PlayerHealth vidaDelJugador = collision.gameObject.GetComponent<PlayerHealth>();

            // 3. Si encontramos el script de vida, le mandamos el daño
            if (vidaDelJugador != null)
            {
                vidaDelJugador.RecibirDanio(cantidadDanio);
                Debug.Log("¡El enemigo golpeó al jugador!");
            }
        }
    }
}