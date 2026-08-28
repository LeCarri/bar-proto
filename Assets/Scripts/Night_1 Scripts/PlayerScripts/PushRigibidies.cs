using UnityEngine;

public class PushRigidbodies : MonoBehaviour
{
    [SerializeField] private float fuerzaEmpuje = 2.0f;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // Si no hay Rigidbody o es Kinematic, no hace nada
        if (body == null || body.isKinematic) return;

        // No empujar objetos debajo de los pies
        if (hit.moveDirection.y < -0.3f) return;

        // Calcular la dirección del empuje
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // Aplicar la fuerza
        body.AddForceAtPosition(pushDir * fuerzaEmpuje, hit.point, ForceMode.Impulse);
    }
}