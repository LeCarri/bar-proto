using UnityEngine;

public class PuntoVasosTest : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private ItemSO itemVasoVacio;

    [Header("Visual")]
    [SerializeField] private GameObject vasoDisponible;

    private bool vasoTomado = false;

    public bool CanInteract()
    {
        if (vasoTomado)
            return false;

        if (ControladorMano3D.Instance != null &&
            ControladorMano3D.Instance.TieneManoOcupada())
        {
            return false;
        }

        return true;
    }

    public string GetDescription()
    {
        return "Presiona [E] para tomar un vaso";
    }

    public void Interact()
{
    Debug.Log("INTERACTUÉ CON PUNTO VASOS");

    if (!CanInteract())
    {
        Debug.Log("CanInteract dio FALSE");
        return;
    }

    TomarVaso();
}

    private void TomarVaso()
{
    if (ControladorMano3D.Instance == null)
    {
        Debug.LogError("NO EXISTE ControladorMano3D EN ESTA ESCENA");
        return;
    }

    vasoTomado = true;

    if (vasoDisponible != null)
        vasoDisponible.SetActive(false);

    ControladorMano3D.Instance.EquiparItem(itemVasoVacio);
}


    public void RestaurarVaso()
    {
        vasoTomado = false;

        if (vasoDisponible != null)
        {
            vasoDisponible.SetActive(true);
        }
    }
}