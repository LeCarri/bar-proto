using UnityEngine;
using System.Collections;

public class EffectoParpadeo : MonoBehaviour
{
    [Header("Referencias")]
    //public GameObject grupoLuces; 
    public GameObject pantallaNegra; 
    public AudioSource audioLuces; // Arrastrá acá un AudioSource con el sonido de chispazo

    [Header("Configuración")]
    public float velocidadBase = 0.1f;
    public int cantidadDeParpadeos = 4;

    public void IniciarParpadeo()
    {
        StartCoroutine(SecuenciaParpadeo());
    }

    IEnumerator SecuenciaParpadeo()
    {
        for (int i = 0; i < cantidadDeParpadeos; i++)
        {
            // Apagamos y suena el chispazo
            ToggleEfecto(false);
            if (audioLuces) audioLuces.Play(); 
            yield return new WaitForSeconds(velocidadBase);

            // Encendemos
            ToggleEfecto(true);
            yield return new WaitForSeconds(velocidadBase * 0.8f);
        }

        // Apagón final antes de la aparición
        ToggleEfecto(false);
        yield return new WaitForSeconds(0.5f);

        // Todo vuelve a la normalidad
        ToggleEfecto(true);
    }

    void ToggleEfecto(bool estado)
    {
        //if (grupoLuces) grupoLuces.SetActive(estado);
        if (pantallaNegra) pantallaNegra.SetActive(!estado);
    }
}