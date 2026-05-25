using UnityEngine;

public class Seguimiento : MonoBehaviour
{
    void LateUpdate()
    {
        Transform cam = Camera.main.transform;

        // Dirección hacia la cámara (ignorando diferencia en altura)
        Vector3 direccion = cam.position - transform.position;
        direccion.y = 0;

        // Evita errores si la dirección es cero
        if (direccion != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direccion);
        }
    }
}