using UnityEngine;

// Colocar en un Empty independiente, delante de cada salida bloqueada.
// El sensor detecta cuando Lucas intenta atravesar el umbral.
[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class AvisoPuertaBloqueada : MonoBehaviour
{
    [Header("Jugador")]
    [SerializeField] private Transform jugador;

    private void Awake()
    {
        BoxCollider sensor = GetComponent<BoxCollider>();
        sensor.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider otro)
    {
        RevisarPaso(otro);
    }

    // Funciona incluso si el bloqueo se activa con Lucas en el sensor.
    private void OnTriggerStay(Collider otro)
    {
        RevisarPaso(otro);
    }

    private void RevisarPaso(Collider otro)
    {
        if (jugador == null || otro == null) return;

        Transform contacto = otro.transform;
        if (contacto != jugador && !contacto.IsChildOf(jugador))
            return;

        // Act1Manager verifica que el bloqueo siga activo
        // y que la linea aun no se haya mostrado.
        if (Act1Manager.Instance != null)
            Act1Manager.Instance.IntentarAbrirPuertaBloqueada();
    }
}
