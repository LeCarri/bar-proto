using UnityEngine;

// Agregamos ", IInteractable" para que el Raycast de Lucas lo reconozca
public class BotellaEspecial : MonoBehaviour, IInteractable 
{
    public string GetDescription() 
    {
        return "Presiona E para recoger la botella";
    }
    // Esta es la función que va a llamar tu script de PlayerInteraction
    public void Interact() 
    {
        Debug.Log("¡Lucas recogió la botella usando la E!");

        // Buscamos al manager (usamos tu lógica de FindAnyObjectByType)
        Act1Manager manager = Object.FindAnyObjectByType<Act1Manager>();

        if (manager != null)
        {
            manager.AlRecogerBotellaEspecial();
            gameObject.SetActive(false); // La botella desaparece
        }
        else 
        {
            Debug.LogError("Ojo: No se encontró el Act1Manager en la escena.");
        }
    }

}