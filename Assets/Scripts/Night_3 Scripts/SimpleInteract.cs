using UnityEngine;

public class SimpleInteract : MonoBehaviour
{
    public ClienteAct3 clienteReal;
    public ClienteSotano clienteSotano;

    private IInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<IInteractable>();

        if (interactable == null)
            interactable = GetComponentInParent<IInteractable>();
    }

    public void Interact()
    {
        if (clienteReal != null)
        {
            clienteReal.Interact();
            return;
        }

        if (clienteSotano != null)
        {
            clienteSotano.Interact();
            return;
        }

        if (interactable != null && interactable.CanInteract())
        {
            interactable.Interact();
            return;
        }
    }
}