using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class RocolaUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panelrocola;

    [Header("Rocola")]
    public Rocola rocola;

    [Header("Botones y Textos")]
    public Button[] botonescanciones;
    public TextMeshProUGUI[] textocanciones;

    [Header("Boton de Salir")]
    public Button botonCerrar; // Asignar este botón en el Inspector

    [Header("Texto de ayuda")]
    public TextMeshProUGUI textoayuda;

    private void Start()
    {
        if (panelrocola != null)
        {
            panelrocola.SetActive(false);
        }

        // Vincular botón de cerrar si está asignado en Inspector
        if (botonCerrar != null)
        {
            botonCerrar.onClick.RemoveAllListeners();
            botonCerrar.onClick.AddListener(Cerrar);
        }
    }

    private void Update()
    {
        // Permitir salir de la UI con Tecla Escape o E si la rocola está abierta
        if (panelrocola != null && panelrocola.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
            {
                Cerrar();
            }
        }
    }

    // ABRIR ROCOLA
    public void Abrir(Rocola nuevarocola)
    {
        rocola = nuevarocola;

        if (panelrocola != null)
        {
            panelrocola.SetActive(true);
        }
        else
        {
            Debug.LogError("UI: PANEL ROCOLA ES NULL");
            return;
        }

        // BLOQUEAR JUGADOR
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

    // ACTUALIZAR LISTA DE CANCIONES
    void Actualizarcanciones()
    {
        if (rocola == null)
        {
            Debug.LogError("UI: No hay una rocola asignada.");
            return;
        }

        for (int i = 0; i < botonescanciones.Length; i++)
        {
            if (i < rocola.canciones.Count)
            {
                botonescanciones[i].gameObject.SetActive(true);

                // NOMBRE DE LA CANCION
                if (i < textocanciones.Length && textocanciones[i] != null)
                {
                    textocanciones[i].text = rocola.canciones[i].nombre;
                }

                int indice = i; // Copia local para la closure de la lambda

                // LIMPIAR Y ASIGNAR EVENTO
                botonescanciones[i].onClick.RemoveAllListeners();
                botonescanciones[i].onClick.AddListener(() => SeleccionarCancion(indice));
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
        if (rocola == null) return;

        rocola.ReproducirCancion(indice);

        if (textoayuda != null)
        {
            textoayuda.text = "Reproduciendo: " + rocola.ObtenerNombreCancion(indice);
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
    }
}