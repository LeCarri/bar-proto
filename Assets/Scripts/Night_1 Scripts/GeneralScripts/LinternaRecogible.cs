using UnityEngine;

public class LinternaRecogible : MonoBehaviour, IInteractable
{
    [Header("Referencias")]
    public GameObject linternaEnMano; // Arrastrá acá la linterna que es hija de la cámara
    public string mensajeUI = "Presiona E para agarrar la linterna";
    

    public string GetDescription()
    {
        return mensajeUI;
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        // 1. Activamos la linterna que Lucas tiene en la mano
        if (linternaEnMano != null)
        {
            linternaEnMano.SetActive(true);
            
            // Opcional: Si querés que empiece apagada y Lucas la tenga que prender:
            linternaEnMano.GetComponentInChildren<Light>().enabled = false;
        }

        // 2. Feedback sonoro (Opcional pero recomendado)
        //AudioSource.PlayClipAtPoint(sonidoClick, transform.position);

        Debug.Log("Lucas agarró la linterna. Ahora puede defenderse.");

        // 3. Hacemos que la linterna de la mesa desaparezca
        gameObject.SetActive(false); 
    }
}
