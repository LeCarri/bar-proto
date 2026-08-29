using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RocolaUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panelrocola;

    [Header("Rocola")]
    public Rocola rocola;

    [Header("Botones")]
    public Button[] botonescanciones;

    [Header("Texto de los botones")]
    public TextMeshProUGUI[] textocanciones;

    [Header("Texto de ayuda")]
    public TextMeshProUGUI textoayuda;


    private void Start()
    {
        if (panelrocola != null)
        {
            panelrocola.SetActive(false);
        }
    }


    
    // ABRIR ROCOLA
    

    public void Abrir(Rocola nuevarocola)
    {
        Debug.Log("UI: Entró a Abrir()");

        rocola = nuevarocola;

        if (panelrocola != null)
        {
            Debug.Log("UI: Panel encontrado. Activándolo...");

            panelrocola.SetActive(true);

            Debug.Log(
                "Panel activo: " +
                panelrocola.activeSelf
            );
        }
        else
        {
            Debug.LogError(
                "UI: PANEL ROCOLA ES NULL"
            );
        }


        // BLOQUEAR JUGADOR Y CAMARA

        if (rocola != null && rocola.jugador != null)
        {
            rocola.jugador.controlesBloqueados = true;
        }


        // MOSTRAR MOUSE

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


        // ACTUALIZAR BOTONES

        Actualizarcanciones();
    }


    
    // ACTUALIZAR LISTA
    

    void Actualizarcanciones()
    {
        if (rocola == null)
        {
            Debug.LogError(
                "UI: No hay una rocola asignada."
            );

            return;
        }

        Debug.Log(
            "UI: Cantidad de canciones: " +
            rocola.canciones.Count
        );


        for (int i = 0; i < botonescanciones.Length; i++)
        {
            if (i < rocola.canciones.Count)
            {
                botonescanciones[i].gameObject.SetActive(true);


                // NOMBRE DE LA CANCION

                if (i < textocanciones.Length)
                {
                    textocanciones[i].text =
                        rocola.canciones[i].nombre;
                }


                int indice = i;


                // LIMPIAR EVENTOS ANTERIORES

                botonescanciones[i].onClick.RemoveAllListeners();


                // AGREGAR EVENTO

                botonescanciones[i].onClick.AddListener(
                    () => SeleccionarCancion(indice)
                );


                Debug.Log(
                    "UI: Botón " +
                    i +
                    " conectado a canción " +
                    rocola.canciones[i].nombre
                );
            }
            else
            {
                botonescanciones[i].gameObject.SetActive(false);
            }
        }
    }


    
    // SELECCIONAR CANCION
   

    void SeleccionarCancion(int indice)
    {
        Debug.Log(
            "UI: Se hizo click en el botón. Índice: " +
            indice
        );


        if (rocola == null)
        {
            Debug.LogError(
                "UI: Rocola es NULL."
            );

            return;
        }


        if (indice < 0 || indice >= rocola.canciones.Count)
        {
            Debug.LogError(
                "UI: Índice inválido: " +
                indice
            );

            return;
        }


        rocola.ReproducirCancion(indice);


        if (textoayuda != null)
        {
            textoayuda.text =
                "Reproduciendo: " +
                rocola.canciones[indice].nombre;
        }
    }


   
    // CERRAR
    

    public void Cerrar()
    {
        if (panelrocola != null)
        {
            panelrocola.SetActive(false);
        }


        // DESBLOQUEAR JUGADOR

        if (rocola != null && rocola.jugador != null)
        {
            rocola.jugador.controlesBloqueados = false;
        }


        // OCULTAR MOUSE

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        rocola = null;
    }
}