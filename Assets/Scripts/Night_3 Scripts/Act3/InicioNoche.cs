using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InicioNoche : MonoBehaviour
{
    [Header("Jugador")]
    public PlayerController jugador;

    [Header("UI durante el inicio")]
    public GameObject uiParanoia;
    public GameObject uiObjetivos;

    [Header("Pantalla negra")]
    public Image pantallaNegra;

    [Header("Audios")]
    public AudioSource audioSource;
    public AudioClip audio1;
    public AudioClip audio2;
    public AudioClip audio3;

    [Header("Fade")]
    public float duracionFade = 2f;


    void Start()
    {
        StartCoroutine(SecuenciaInicio());
    }


    IEnumerator SecuenciaInicio()
    {
        // BLOQUEAR JUGADOR

        if (jugador != null)
        {
            jugador.controlesBloqueados = true;
        }

        // DESACTIVAR UI

        if (uiParanoia != null)
        {
            uiParanoia.SetActive(false);
        }

        if (uiObjetivos != null)
        {
            uiObjetivos.SetActive(false);
        }


        // ASEGURAR PANTALLA NEGRA

        if (pantallaNegra != null)
        {
            Color color = pantallaNegra.color;
            color.a = 1f;
            pantallaNegra.color = color;

            pantallaNegra.gameObject.SetActive(true);
        }


        // AUDIO 1

        if (audioSource != null && audio1 != null)
        {
            audioSource.clip = audio1;
            audioSource.Play();

            yield return new WaitForSeconds(audio1.length);
        }


        // AUDIO 2

        if (audioSource != null && audio2 != null)
        {
            audioSource.clip = audio2;
            audioSource.Play();

            yield return new WaitForSeconds(audio2.length);
        }

        // AUDIO 3

        if (audioSource != null && audio3 != null)
        {
            audioSource.clip = audio3;
            audioSource.Play();

            yield return new WaitForSeconds(audio3.length);
        }

        // FADE DE NEGRO

        yield return StartCoroutine(
            HacerFade()
        );


        // DESACTIVAR IMAGEN

        if (pantallaNegra != null)
        {
            pantallaNegra.gameObject.SetActive(false);
        }

        //REACTIVAR UI

        if (uiParanoia != null)
        {
            uiParanoia.SetActive(true);
        }

        if (uiObjetivos != null)
        {
            uiObjetivos.SetActive(true);
        }


        // DESBLOQUEAR JUGADOR

        if (jugador != null)
        {
            jugador.controlesBloqueados = false;
        }
    }


    IEnumerator HacerFade()
    {
        if (pantallaNegra == null)
            yield break;

        float tiempo = 0f;

        Color color = pantallaNegra.color;

        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;

            float progreso =
                Mathf.Clamp01(tiempo / duracionFade);

            color.a =
                Mathf.Lerp(1f, 0f, progreso);

            pantallaNegra.color = color;

            yield return null;
        }

        color.a = 0f;
        pantallaNegra.color = color;
    }
}
