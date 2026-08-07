using UnityEngine;

public class ElementosLimpieza : MonoBehaviour
{
    public void Interact()
    {
        Debug.Log("Elementos de limpieza recogidos.");

        if (Act3Manager.Instance != null)
        {
            Act3Manager.Instance.RecogerElementosLimpieza();
        }

        gameObject.SetActive(false);
    }
}
