using UnityEngine;
using System.Collections;
public class TriggerRegreso : MonoBehaviour
{
    private bool activado = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !activado)
        {
            activado = true;
            StartCoroutine(AparicionMujer());
        }
    }

    IEnumerator AparicionMujer()
    {
        Act1Manager manager = FindObjectOfType<Act1Manager>();
        
        // Parpadeo de luces
        if (manager.effectoParpadeo != null) manager.effectoParpadeo.IniciarParpadeo();
        
        yield return new WaitForSeconds(0.3f);

        // Aparece la mujer
        manager.objetoMujer.SetActive(true);

         yield return new WaitForSeconds(2f);
        
        // Diálogo inicial (Voz familiar)
        manager.MostrarDialogo("Mujer: Hola, Lucas... ¿Todavía servís lo mismo de siempre?");
        
        // Bloqueamos el movimiento de Lucas para que sea una escena tensa
        // manager.BloquearMovimiento(true); 
    }
}