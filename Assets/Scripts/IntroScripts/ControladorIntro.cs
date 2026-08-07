using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class ControladorIntro : MonoBehaviour
{
    [Header("UI Componentes")]
    [SerializeField] private TMP_Text textoUI;
    [SerializeField] private CanvasGroup fondoNegroCanvas;

    [Header("Cámaras (Cinemachine)")]
    [SerializeField] private GameObject camaraBar3D;
    [SerializeField] private GameObject camaraSotano3D;
    [SerializeField] private CinemachineSplineCart splineCartSotano;

    [Header("Sótano")]
    [SerializeField] private PuertaSotanoIntro puertaSotano;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSourceAmbiente;
    [SerializeField] private AudioSource audioSourceTipeo;
    [SerializeField] private AudioSource audioSourceSFX;

    [Header("Clips de Audio")]
    [SerializeField] private AudioClip clipAmbienteGrave;
    [SerializeField] private AudioClip clipTipeoLargo; // Poner acá el audio grabado del teclado
    [SerializeField] private AudioClip clipGritoMujer;

    [Header("Tiempos y Ajustes")]
    [SerializeField] private float velocidadTipeo = 0.06f;
    [SerializeField] private float tiempoEsperaTexto = 3.5f;
    [SerializeField] private float duracionFadeTexto = 1.2f;
    [SerializeField] private float duracionFadeNegro = 1.5f;
    [SerializeField] private float duracionRecorridoSotano = 3.0f;

    [Header("Posicionamiento de Texto")]
    [SerializeField] private float posYCentro = 0f;
    [SerializeField] private float posYAbajo = 100f;

    [Header("Siguiente Escena")]
    [SerializeField] private string nombreEscenaGameplay = "Noche1";

    private void Start()
    {
        if (audioSourceAmbiente != null && clipAmbienteGrave != null)
        {
            audioSourceAmbiente.clip = clipAmbienteGrave;
            audioSourceAmbiente.loop = true;
            audioSourceAmbiente.Play();
        }

        StartCoroutine(SecuenciaIntroCompleta());
    }

    private IEnumerator SecuenciaIntroCompleta()
    {
        if (fondoNegroCanvas != null) fondoNegroCanvas.alpha = 1f;
        ActivarCamara(camaraBar3D);

        // BLOQUE 1: PANTALLA EN NEGRO
        yield return StartCoroutine(MostrarFraseConFade(
            "Llevo tres semanas durmiendo dos horas por día. Pero las facturas no dejan de llegar... y el banco no espera.", 
            posCentrada: true
        ));

        // BLOQUE 2: REVELACIÓN DEL BAR 3D
        yield return StartCoroutine(FadeNegro(1f, 0f));
        yield return StartCoroutine(MostrarFraseConFade(
            "Odio este trabajo. Odio este lugar. Pero perderlo significaría quedarme en la calle.", 
            posCentrada: false
        ));
        yield return StartCoroutine(FadeNegro(0f, 1f));

        // BLOQUE 3: LA SOSPECHA
        yield return StartCoroutine(MostrarFraseConFade(
            "Solo tengo que aguantar hasta el amanecer. Ignorar los ruidos de abajo y terminar el turno...", 
            posCentrada: true
        ));

        // BLOQUE 4: SÓTANO Y CLÍMAX
        if (splineCartSotano != null) splineCartSotano.SplinePosition = 0f;

        ActivarCamara(camaraSotano3D);
        yield return new WaitForSeconds(0.2f);

        StartCoroutine(FadeNegro(1f, 0f));

        if (puertaSotano != null)
        {
            puertaSotano.AbrirPuertas();
        }

        float tiempo = 0f;
        while (tiempo < duracionRecorridoSotano)
        {
            tiempo += Time.deltaTime;
            float progreso = Mathf.Clamp01(tiempo / duracionRecorridoSotano);

            if (splineCartSotano != null) splineCartSotano.SplinePosition = progreso;
            yield return null;
        }

        if (splineCartSotano != null) splineCartSotano.SplinePosition = 1f;

        // CORTE A NEGRO INSTANTÁNEO Y GRITO
        if (fondoNegroCanvas != null) fondoNegroCanvas.alpha = 1f;

        if (audioSourceAmbiente != null) audioSourceAmbiente.Stop();

        if (audioSourceSFX != null && clipGritoMujer != null)
        {
            audioSourceSFX.PlayOneShot(clipGritoMujer);
        }

        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(nombreEscenaGameplay);
    }

    private void ActivarCamara(GameObject camaraAActivar)
    {
        if (camaraBar3D != null) camaraBar3D.SetActive(false);
        if (camaraSotano3D != null) camaraSotano3D.SetActive(false);
        if (camaraAActivar != null) camaraAActivar.SetActive(true);
    }

    private IEnumerator MostrarFraseConFade(string mensaje, bool posCentrada)
    {
        if (textoUI == null) yield break;

        RectTransform rect = textoUI.rectTransform;
        rect.anchoredPosition = new Vector2(0f, posCentrada ? posYCentro : posYAbajo);

        textoUI.gameObject.SetActive(true);
        Color colorOriginal = textoUI.color;
        textoUI.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, 1f);

        textoUI.text = "";

        // 1. ARRANCAMOS EL AUDIO DE TIPEO
        if (audioSourceTipeo != null && clipTipeoLargo != null)
        {
            audioSourceTipeo.clip = clipTipeoLargo;
            audioSourceTipeo.loop = true; // Para que no se corte si el mensaje es muy largo
            audioSourceTipeo.Play();
        }

        // 2. EFECTO DE TIPEO
        foreach (char letra in mensaje.ToCharArray())
        {
            textoUI.text += letra;
            yield return new WaitForSeconds(velocidadTipeo);
        }

        // 3. APAGAMOS EL AUDIO APENAS TERMINA DE ESCRIBIR EL TEXTO
        if (audioSourceTipeo != null)
        {
            audioSourceTipeo.Stop();
        }

        // Tiempo de lectura con el texto completo en pantalla
        yield return new WaitForSeconds(tiempoEsperaTexto);

        // Fade Out progresivo del texto
        float t = 0f;
        while (t < duracionFadeTexto)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / duracionFadeTexto);
            textoUI.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, alpha);
            yield return null;
        }

        textoUI.text = "";
    }

    private IEnumerator FadeNegro(float inicio, float fin)
    {
        if (fondoNegroCanvas == null) yield break;

        float t = 0f;
        while (t < duracionFadeNegro)
        {
            t += Time.deltaTime;
            fondoNegroCanvas.alpha = Mathf.Lerp(inicio, fin, t / duracionFadeNegro);
            yield return null;
        }
        fondoNegroCanvas.alpha = fin;
    }
}