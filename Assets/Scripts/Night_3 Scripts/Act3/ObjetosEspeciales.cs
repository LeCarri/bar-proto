using UnityEngine;

public class ObjetosEspeciales : MonoBehaviour
{
    [Header("Diálogo del objeto")]
    [TextArea(3, 6)]
    public string textoHistoria;

    [Header("Configuración")]
    public bool sePuedeRepetir = true;

    private bool yaInspeccionado = false;

    public void Interactuar()
    {
        if (yaInspeccionado && !sePuedeRepetir)
            return;

        if (Act3Manager.Instance == null)
        {
            Debug.LogWarning("No se encontró Act3Manager.");
            return;
        }

        if (string.IsNullOrWhiteSpace(textoHistoria))
        {
            Debug.LogWarning(
                "El objeto " + gameObject.name + " no tiene texto de historia."
            );

            return;
        }

        Act3Manager.Instance.MostrarDialogo(textoHistoria);

        yaInspeccionado = true;
    }
}