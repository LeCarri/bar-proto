using UnityEngine;

public class PuertaSotano : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Entrando al sótano");

        if (Act3Manager.Instance != null)
        {
            Act3Manager.Instance.IrASotano();
        }
    }

    public string GetDescription()
    {
        return "Presiona [E] para entrar al sótano";
    }

    public bool CanInteract()
    {
        return true;
    }
}