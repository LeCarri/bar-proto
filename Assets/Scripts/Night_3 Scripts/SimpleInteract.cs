using UnityEngine;

public class SimpleInteract : MonoBehaviour
{
    public ClienteAct3 clienteReal;
    public ClienteSotano clienteSotano;

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
    }
}