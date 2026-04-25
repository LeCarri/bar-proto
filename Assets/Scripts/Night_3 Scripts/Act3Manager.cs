using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Act3Manager : MonoBehaviour
{
    [Header("UI y Diálogos")]
    public TextMeshProUGUI textoSubtitulos;

    [Header("Efectos")]
    public EffectoParpadeo effectoParpadeo;

    [Header("Sistemas de Iluminación")]
    public GameObject lucesNormales;
    public GameObject lucesServicio;
    public GameObject lucesCombate;

    private Coroutine dialogoActual;

    //CAMBIAR LAS LUCES
    public void CambiarIluminacion(string estado)
    {
        Debug.Log("Cambiando iluminación a: " + estado);

        if (lucesNormales != null) lucesNormales.SetActive(false);
        if (lucesServicio != null) lucesServicio.SetActive(false);
        if (lucesCombate != null) lucesCombate.SetActive(false);

        switch (estado)
        {
            case "Normal":
                if (lucesNormales != null) lucesNormales.SetActive(true);
                break;

            case "Servicio":
                if (lucesServicio != null) lucesServicio.SetActive(true);
                break;

            case "Combate":
                if (lucesCombate != null) lucesCombate.SetActive(true);
                break;

            default:
                Debug.LogWarning("El estado de luz '" + estado + "' no existe.");
                break;
        }
    }
    //DIALOGOS
    public void MostrarDialogo(string mensaje)
    {
        if (textoSubtitulos != null)
        {
            // Si ya hay un diálogo corriendo, lo frenamos
            if (dialogoActual != null)
                StopCoroutine(dialogoActual);

            textoSubtitulos.text = mensaje;
            dialogoActual = StartCoroutine(LimpiarTextoCoroutine());
        }
    }

    IEnumerator LimpiarTextoCoroutine()
    {
        yield return new WaitForSeconds(2f);
        textoSubtitulos.text = "";
    }

    void Start()
    {
        StartCoroutine(SecuenciaInicio());
    }

    IEnumerator SecuenciaInicio()
    {
        Debug.Log("Start ejecutado");

        effectoParpadeo.IniciarParpadeo();

        yield return new WaitForSeconds(1.5f);

        MostrarDialogo("Ya casi... una ronda más y bajo a buscarlas.Tienen que estar por despertar");

        yield return new WaitForSeconds(3f);

        CambiarIluminacion("Servicio");
        effectoParpadeo.IniciarParpadeo();
    }



    void Update()
    {

    }
}