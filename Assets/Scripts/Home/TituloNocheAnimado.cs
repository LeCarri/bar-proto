
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TituloNocheAnimado : MonoBehaviour
{
    [Header("Interfaz")]
    [SerializeField] private CanvasGroup grupoTitulo;
    [SerializeField] private TMP_Text textoNoche;
    [SerializeField] private GameObject numeroViewport;
    [SerializeField] private RectTransform contador;

    [Header("Máquina de escribir")]
    [SerializeField] private string palabra = "NOCHE";
    [SerializeField] private float tiempoPorLetra = 0.13f;

    [Header("Contador vertical")]
    [SerializeField] private float alturaNumero = 120f;
    [SerializeField] private float esperaAntesContador = 0.5f;
    [SerializeField] private float duracionContador = 0.65f;

    [Header("Final")]
    [SerializeField] private float tiempoVisible = 1.5f;
    [SerializeField] private float duracionFade = 0.7f;

    [Header("Sonidos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoLetra;
    [SerializeField] private AudioClip sonidoContador;
    [SerializeField] private AudioClip sonidoFinal;

    [Header("Activación")]
    [SerializeField] private bool reproducirAlIniciar = false;
    [SerializeField] private UnityEvent alFinalizar;

    private Coroutine animacionActual;

    private void Start()
    {
        if (reproducirAlIniciar)
            ReproducirTitulo();
    }

    public void ReproducirTitulo()
    {
        if (animacionActual != null)
            StopCoroutine(animacionActual);

        gameObject.SetActive(true);
        animacionActual = StartCoroutine(AnimarTitulo());
    }

    private IEnumerator AnimarTitulo()
    {
        grupoTitulo.alpha = 1f;
        textoNoche.text = "";

        numeroViewport.SetActive(false);
        contador.anchoredPosition = Vector2.zero;

        // 1. Escribir NOCHE letra por letra
        foreach (char letra in palabra)
        {
            textoNoche.text += letra;

            ReproducirSonido(sonidoLetra, 0.4f);

            yield return new WaitForSecondsRealtime(tiempoPorLetra);
        }

        // 2. Aparece inicialmente el número 1
        numeroViewport.SetActive(true);

        yield return new WaitForSecondsRealtime(esperaAntesContador);

        // 3. Desplazamiento vertical del 1 al 2
        ReproducirSonido(sonidoContador, 0.8f);

        float tiempo = 0f;
        Vector2 origen = Vector2.zero;
        Vector2 destino = new Vector2(0f, alturaNumero);

        while (tiempo < duracionContador)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(tiempo / duracionContador);

            // Movimiento rápido al inicio y suave al final
            float suavizado = 1f - Mathf.Pow(1f - t, 3f);

            contador.anchoredPosition =
                Vector2.LerpUnclamped(origen, destino, suavizado);

            yield return null;
        }

        contador.anchoredPosition = destino;

        // 4. Golpe sonoro al llegar al 2
        ReproducirSonido(sonidoFinal, 1f);

        yield return new WaitForSecondsRealtime(tiempoVisible);

        // 5. Desaparece el título
        float fade = 0f;

        while (fade < duracionFade)
        {
            fade += Time.unscaledDeltaTime;

            grupoTitulo.alpha =
                1f - Mathf.Clamp01(fade / duracionFade);

            yield return null;
        }

        grupoTitulo.alpha = 0f;
        animacionActual = null;

        // Avisar que terminó la animación
        alFinalizar?.Invoke();
    }

    private void ReproducirSonido(AudioClip clip, float volumen)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip, volumen);
    }
}
