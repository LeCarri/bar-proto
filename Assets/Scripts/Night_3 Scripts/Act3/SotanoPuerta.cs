using UnityEngine;

public class PuertaSotano : MonoBehaviour
{
    public void Interact()
    {
        Debug.Log("Entrando al sótano");

        if (Act3Manager.Instance != null)
        {
            Act3Manager.Instance.IrASotano();
        }
    }
}