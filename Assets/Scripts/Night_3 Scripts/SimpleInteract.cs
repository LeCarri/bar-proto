using UnityEngine;
using UnityEngine;

public class SimpleInteract : MonoBehaviour
{
    public ClienteAct3 clienteReal;

    public void Interact()
    {
        if (clienteReal != null)
        {
            clienteReal.Interact();
        }
    }
}