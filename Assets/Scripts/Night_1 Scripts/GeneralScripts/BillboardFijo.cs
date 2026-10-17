
using UnityEngine;

public class BillboardFijo : MonoBehaviour
{
    [SerializeField] private Transform camaraJugador;

    void LateUpdate()
    {
        if (camaraJugador == null)
        {
            if (Camera.main == null) return;

            camaraJugador = Camera.main.transform;
        }

        // Siempre orientado como la cámara,
        // sin modificar la posición del Canvas.
        transform.rotation = camaraJugador.rotation;
    }
}
    