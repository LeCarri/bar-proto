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
        // 1. Control de seguridad: Si la cámara principal no está lista, evitamos calcular en este frame
        if (Camera.main == null) return;

        // Calculamos el rayo usando el Viewport de forma matemática (0.5, 0.5 es el centro exacto)
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        bool hitSomething = false;

        // 2. Filtro rígido IF/ELSE para matar la intermitencia de frames
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            // Buscamos si el objeto con el que chocamos tiene la interfaz de interacción
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null && interactable.CanInteract())
            {
                // Marcamos que encontramos algo válido para congelar el estado de la UI
                hitSomething = true;
                ShowInteractionUI(interactable.GetDescription());

                // Dibujamos la línea de debug verde fija desde la cámara hasta el punto exacto de impacto
                Debug.DrawLine(ray.origin, hit.point, Color.green);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                }
            }
            else
            {
                // Si chocamos contra la capa pero el objeto no es interactuable, la línea se queda roja fija en el impacto
                Debug.DrawLine(ray.origin, hit.point, Color.red);
            }
        }
        else
        {
            // Si el rayo se pierde en el aire del bar, se dibuja en rojo clavado a la distancia máxima (no al infinito)
            Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red);
        }
        
        // 3. Solo apagamos el cartel si en este frame realmente no estamos mirando nada de la capa interactuable
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