using UnityEngine;

public class BotellaEspecial : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Debug para ver en consola quién te está tocando (temporal)
        Debug.Log("Objeto tocando la botella: " + other.name + " | Tag: " + other.tag);

        // --- ESTA LÍNEA ES LA CRÍTICA ---
        // Solo ejecuta la lógica si el objeto que colisiona tiene el tag "Player"
        if (other.CompareTag("Player")) 
        {
            Debug.Log("¡Lucas recogió la botella!");
            
            Act1Manager manager = Object.FindAnyObjectByType<Act1Manager>();
            
            if (manager != null)
            {
                manager.AlRecogerBotellaEspecial();
                gameObject.SetActive(false); // Desaparece la botella
            }
        }
    }
}