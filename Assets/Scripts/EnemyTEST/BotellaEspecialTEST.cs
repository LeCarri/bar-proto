
using UnityEngine;

public class BotellaEspecialTEST : MonoBehaviour, IInteractable
{
    private bool recogida = false;

    public bool CanInteract()
    {
        return !recogida;
    }

    public string GetDescription()
    {
        return "Presiona [E] para tomar la botella";
    }

    public void Interact()
    {
        if (recogida) return;

        recogida = true;

        if (EnemyTEST.Instance != null)
        {
            EnemyTEST.Instance.AlRecogerBotellaEspecial();
        }

        foreach (MeshRenderer r in
                 GetComponentsInChildren<MeshRenderer>())
        {
            r.enabled = false;
        }

        foreach (Collider c in
                 GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
    }
}
