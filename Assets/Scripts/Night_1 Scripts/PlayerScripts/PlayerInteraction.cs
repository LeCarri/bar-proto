using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración")]
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;

    [Header("UI")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private const string DefaultInteractionText = "Interactuar";

    void Awake()
    {
        HideInteractionUI();
    }

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

            if (interactable != null && interactable.CanInteract())
            {
                hitSomething = true;
                ShowInteractionUI(interactable.GetDescription());

                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                    //Debug.Log("Tocaste: " + hit.collider.name);
                }
            }
        }
        
        if (!hitSomething)
        {
            HideInteractionUI();
        }
    }

    private void ShowInteractionUI(string description)
    {
        if (interactionPanel != null)
        {
            interactionPanel.SetActive(true);
        }

        if (interactionText != null)
        {
            interactionText.text = DefaultInteractionText;
        }

        if (descriptionText != null)
        {
            descriptionText.text = GetContextDescription(description);
        }
    }

    private void HideInteractionUI()
    {
        if (interactionPanel != null)
        {
            interactionPanel.SetActive(false);
        }
    }

    private string GetContextDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "";
        }

        string contextDescription = description.Trim();
        contextDescription = RemovePrefix(contextDescription, "Presiona [E] para ");
        contextDescription = RemovePrefix(contextDescription, "Presiona E para ");
        contextDescription = RemovePrefix(contextDescription, "Press [E] to ");
        contextDescription = RemovePrefix(contextDescription, "Press E to ");

        if (contextDescription.Length == 0)
        {
            return "";
        }

        return char.ToUpper(contextDescription[0]) + contextDescription.Substring(1);
    }

    private string RemovePrefix(string text, string prefix)
    {
        if (text.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
        {
            return text.Substring(prefix.Length);
        }

        return text;
    }
}
