using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración")]
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;

    void Update()
    {
        // Raycast desde el centro exacto de la cámara
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Visualización en la ventana Scene
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red);

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.green);

            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                    Debug.Log("Tocaste: " + hit.collider.name);
                }
            }
        }
    }
}