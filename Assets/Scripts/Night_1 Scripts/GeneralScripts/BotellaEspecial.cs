using UnityEngine;

public class BotellaEspecial : MonoBehaviour, IInteractable
{
    private bool recogida = false;

    // --- MÉTODOS DE IINTERACTABLE ---
    public bool CanInteract()
    {
        return !recogida; // Devuelve true para que el Raycast se ponga VERDE
    }

    public string GetDescription()
    {
        return "Presiona [E] para tomar la botella";
    }

    public void Interact()
    {
        RecogerBotella();
    }

    // --- LÓGICA DE LA BOTELLA ---
    public void RecogerBotella()
    {
        if (recogida) return;
        recogida = true;

        if (Act1Manager.Instance != null)
        {
            Act1Manager.Instance.AlRecogerBotellaEspecial();
        }

        OcultarBotella();
    }

    private void OcultarBotella()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer r in renderers) r.enabled = false;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders) c.enabled = false;
    }
}