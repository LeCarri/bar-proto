using UnityEngine;

public class ClienteSotano : MonoBehaviour
{
    [TextArea]
    public string dialogo;

    bool interactuado = false;

    public void Interact()
    {
        if (interactuado)
            return;

        interactuado = true;

        SotanoManager.Instance.MostrarDialogo(dialogo);

        SotanoManager.Instance.InteraccionCompletada();
    }
}