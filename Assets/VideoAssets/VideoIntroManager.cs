using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class VideoIntroManager : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    [SerializeField] private string nombreEscenaSiguiente = "MainMenu";
    [SerializeField] private GameObject panelNegroDeCarga; // Arrastrá tu panel acá
    
    [Header("Congelar al final")]
    [SerializeField] private float tiempoDeCongelamiento = 1.0f;
    [SerializeField] private float margenFinal = 0.1f;

    private bool pausadoParaCongelar = false;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        
        // Nos suscribimos al evento cuando el video realmente arranca a reproducir fotogramas
        videoPlayer.started += OnVideoStarted;
        videoPlayer.loopPointReached += OnIntroFinished;
    }

    void OnVideoStarted(VideoPlayer vp)
    {
        // Apenas empieza el video, apagamos el fondo negro para que se vea fluido
        if (panelNegroDeCarga != null)
        {
            panelNegroDeCarga.SetActive(false);
        }
    }

    void Update()
    {
        if (!pausadoParaCongelar && videoPlayer.isPlaying)
        {
            double tiempoRestante = (videoPlayer.frameCount / videoPlayer.frameRate) - videoPlayer.time;
            
            if (tiempoRestante <= margenFinal)
            {
                StartCoroutine(CongelarYContinuar());
            }
        }

        if (Input.anyKeyDown)
        {
            CambiarEscena();
        }
    }

    IEnumerator CongelarYContinuar()
    {
        pausadoParaCongelar = true;
        videoPlayer.Pause();
        yield return new WaitForSeconds(tiempoDeCongelamiento);
        videoPlayer.Play();
    }

    void OnIntroFinished(VideoPlayer vp)
    {
        CambiarEscena();
    }

    void CambiarEscena()
    {
        SceneManager.LoadScene(nombreEscenaSiguiente);
    }
}