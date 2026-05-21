using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración")]
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;

    [Header("UI")]
    public TextMeshProUGUI interactionText;

    void Update()
    {
        // Raycast desde el centro exacto de la cámara
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        bool hitSomething = false;

        // Visualización en la ventana Scene
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red);

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.green);

            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                hitSomething = true;
                // Mostramos el texto del objeto
                if (interactionText != null)
                {
                    interactionText.text = interactable.GetDescription();
                    interactionText.gameObject.SetActive(true);
                }

                    if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                    //Debug.Log("Tocaste: " + hit.collider.name);
                }
            }
        }
        
        if (!hitSomething && interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }
}