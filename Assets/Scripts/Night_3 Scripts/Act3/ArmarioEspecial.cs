using UnityEngine;

public class ArmarioElementosLimpieza : MonoBehaviour
{
    private bool utilizado = false;

    public void Interact()
    {
        if (utilizado)
            return;

        if (Act3Manager.Instance == null)
            return;

        if (!Act3Manager.Instance.sangreLimpiada)
        {
            Act3Manager.Instance.MostrarDialogo(
                "Primero tengo que terminar de limpiar."
            );

            return;
        }

        if (!Act3Manager.Instance.tieneElementosLimpieza)
        {
            Act3Manager.Instance.MostrarDialogo(
                "No tengo los elementos de limpieza."
            );

            return;
        }

        utilizado = true;

        Debug.Log(
            "Elementos de limpieza guardados en el armario."
        );

        Act3Manager.Instance.GuardarElementosLimpieza();
    }
}
