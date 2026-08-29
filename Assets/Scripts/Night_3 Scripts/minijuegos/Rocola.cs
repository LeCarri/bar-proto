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

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("UI")]
    public RocolaUI rocolaUI;

    [Header("Estado")]
    public bool reproduciendo = false;

    [Header("Jugador")]
    public PlayerController jugador;

    private int cancionActual = -1;


    
    // INTERACCIÓN
    

    public void Interact()
    {
        Debug.Log("Interactuando con la rocola.");

        if (rocolaUI != null)
        {
            Debug.Log("ROCOLA: Abriendo UI.");

            rocolaUI.Abrir(this);
        }
        else
        {
            Debug.LogError(
                "ROCOLA: No hay RocolaUI asignado en el Inspector."
            );
        }
    }


    
    // REPRODUCIR CANCIÓN
   

    public void ReproducirCancion(int indice)
    {
        Debug.Log("BOTÓN: Intentando reproducir canción " + indice);

        if (audioSource == null)
        {
            Debug.LogWarning(
                "La rocola no tiene un AudioSource asignado."
            );

            return;
        }

        if (indice < 0 || indice >= canciones.Count)
        {
            Debug.LogWarning(
                "Índice de canción no válido: " + indice
            );

            return;
        }

        cancion cancionSeleccionada = canciones[indice];

        // ACÁ ESTABA EL ERROR
        if (cancionSeleccionada.audio == null)
        {
            Debug.LogWarning(
                "La canción '" +
                cancionSeleccionada.nombre +
                "' no tiene un AudioClip asignado."
            );

            return;
        }

        audioSource.clip = cancionSeleccionada.audio;
        audioSource.Play();

        cancionActual = indice;
        reproduciendo = true;

        Debug.Log(
            "Reproduciendo: " +
            cancionSeleccionada.nombre
        );
    }


    
    // DETENER CANCIÓN
    

    public void DetenerCanciones()
    {
        if (audioSource == null)
            return;

        audioSource.Stop();

        reproduciendo = false;
        cancionActual = -1;

        Debug.Log("Canción detenida.");
    }


   
    // PAUSAR / CONTINUAR
    

    public void PausarCancion()
    {
        if (audioSource == null)
            return;

        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("Canción pausada.");
        }
        else
        {
            audioSource.UnPause();
            Debug.Log("Canción continuada.");
        }
    }


    
    // INFORMACIÓN
    

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