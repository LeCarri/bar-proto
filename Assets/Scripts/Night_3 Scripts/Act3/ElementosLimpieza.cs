using UnityEngine;

public class ElementosLimpieza : MonoBehaviour
{
    private bool recogido = false;

    public void Interact()
    {
        if (recogido)
            return;

        recogido = true;

        Debug.Log("Elementos de limpieza recogidos.");

        if (Act3Manager.Instance != null)
        {
            Act3Manager.Instance.RecogerElementosLimpieza();
        }
        else
        {
            Debug.LogWarning("Act3Manager no está disponible.");
        }

        gameObject.SetActive(false);
    }
}