using System.Collections.Generic;
using UnityEngine;


public class Rocola : MonoBehaviour
{
    [System.Serializable]
    public class cancion
    {
        public string nombre;
        public AudioClip audio;
    }

    [Header("Canciones")]
    public List<cancion> canciones = new List<cancion>();

    [Header("audio")]
    public AudioSource audioSource;

    [Header("Estado")]
    public bool  reproduciendo = false;

    private int cancionActual = -1;


    //INTERACCION

    public void Interact()
    {
        Debug.Log("Interactuando");
        for (int i = 0; i<canciones.Count; i++)
        {
            Debug.Log(
                i + ":" + canciones[i].nombre
            );
        }
    }



    //REPRODUCIR CANCION

    public void ReproducirCancion(int indice)
    {
        if (audioSource==null)
        {
            Debug.LogWarning("la rocola no tiene un audiosurce asignado");

            return;
        }

        if (indice < 0 || indice>= canciones.Count)
        {
            Debug.LogWarning("indice de cancion no valido" + indice);

            return; 
        }

        cancion cancion = canciones[indice];

        if (cancion.audio != null) 
        {
            Debug.LogWarning("la cancion '" + cancion.nombre + "'no tiene un AudioClip asignado");

            return;
        }

        audioSource.clip = cancion.audio;
        audioSource.Play();

        cancionActual = indice;
        reproduciendo = true;

        Debug.Log("reproduciendo:" + cancion.nombre);

    }

    //DETENER CANCION

    public void DetenerCanciones()
    {
        if (audioSource==null)
            return;

        audioSource.Stop();

        reproduciendo = false;
        cancionActual = -1;

        Debug.Log("Cancion detenida");
    }

    //PAUSAR / CONTINUAR

    public void PausarCacion()
    {
        if(audioSource==null)
            return;

        Debug.Log("Cancion continuada.");
    }

    //INFORMACION

    public string ObtenerNombreCancion(int indice)
    {
        if (indice < 0 || indice >= canciones.Count)
            return "";
        return canciones[indice].nombre;
    }

    public int ObtenerCantidadCanciones()
    {
        return canciones.Count;
    }

    public int ObtenerCancionActual()
    {
        return cancionActual;
    }
}
