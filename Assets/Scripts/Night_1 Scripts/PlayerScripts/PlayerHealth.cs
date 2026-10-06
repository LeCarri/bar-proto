using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Vida")]
    public float vidaActual = 100f;

    [Header("UI de Derrota")]
    public GameObject canvasDerrota;

    [Header("Fade a negro")]
    [Tooltip("CanvasGroup de una imagen negra que cubra toda la pantalla.")]
    public CanvasGroup fadeNegro;

    public float duracionFadeNegro = 0.8f;
    public float esperaEnNegro = 0.6f;

    [Header("Cámara - Derrota")]
    [Tooltip("Idealmente CameraOffset, no la Main Camera.")]
    public Transform pivotCamaraDerrota;

    public float duracionCaidaCamara = 0.9f;
    public float inclinacionCamara = 25f;
    public float descensoCamara = 0.25f;

    [Header("Linterna")]
    public Flashlight linterna;

    [Header("Controles del jugador")]
    [Tooltip("Arrastrá acá los scripts de movimiento/mirada que quieras bloquear.")]
    public MonoBehaviour[] controlesJugador;

    [Header("Audio derrota")]

    [Header("Volumen final de las bases en derrota")]

    [Range(0f, 1f)]
    public float volumenBaseEnemy1Derrota = 0.25f;

    [Range(0f, 1f)]
    public float volumenBaseEnemy2Derrota = 0.20f;

    [Tooltip("Audio de contacto con la Sombra. Se mantiene durante la caída.")]
    public AudioSource audioContacto;

    [Tooltip("Voces de la criatura.")]
    public AudioSource enemyVoices;

    [Tooltip("Base de combate que queremos mantener brevemente.")]
    public AudioSource baseEnemy1;

    [Tooltip("Base que se corta inmediatamente al morir.")]
    public AudioSource baseEnemy2;

    [Header("Tiempos")]
    public float duracionAntesDelNegro = 0.8f;

    private bool derrotado = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        ResolverCanvasDerrota();
    }

    private void Start()
    {
        Time.timeScale = 1f;

        ResolverCanvasDerrota();

        if (EsObjetoDeEscena(canvasDerrota))
            canvasDerrota.SetActive(false);

        if (fadeNegro != null)
        {
            fadeNegro.alpha = 0f;
            fadeNegro.blocksRaycasts = false;
            fadeNegro.interactable = false;
        }
    }

    public void RecibirDanio(float cantidad)
    {
        if (derrotado)
            return;

        vidaActual -= cantidad;
        vidaActual = Mathf.Max(vidaActual, 0f);

        Debug.Log("Vida del jugador: " + vidaActual);

        if (vidaActual <= 0f)
        {
            Morir();
        }
    }

    private void Morir()
    {
        if (derrotado)
            return;

        derrotado = true;

        Debug.Log("Lucas fue derrotado por las sombras...");

        StartCoroutine(SecuenciaDerrota());
    }

    private IEnumerator SecuenciaDerrota()
    {
        // ==========================================
        // 1. BLOQUEAR JUGADOR
        // ==========================================

        if (controlesJugador != null)
        {
            foreach (MonoBehaviour control in controlesJugador)
            {
                if (control != null)
                    control.enabled = false;
            }
        }

        // ==========================================
        // 2. APAGAR LINTERNA
        // ==========================================

        if (linterna != null)
        {
            linterna.PrepararParaDerrota();

            // Bloqueamos sus inputs,
            // pero la Light queda encendida.
            linterna.enabled = false;
        }

        // ==========================================
        // 3. AUDIO
        // ==========================================

        // Los demás siguen durante la caída.
        if (audioContacto != null && !audioContacto.isPlaying)
            audioContacto.Play();

        // ==========================================
        // 4. CAÍDA DE CÁMARA
        // ==========================================

        if (pivotCamaraDerrota != null)
        {
            Vector3 posicionInicial =
                pivotCamaraDerrota.localPosition;

            Quaternion rotacionInicial =
                pivotCamaraDerrota.localRotation;

            Vector3 posicionFinal =
                posicionInicial +
                Vector3.down * descensoCamara;

            Quaternion rotacionFinal =
                rotacionInicial *
                Quaternion.Euler(
                    0f,
                    0f,
                    inclinacionCamara
                );

            float tiempo = 0f;

            while (tiempo < duracionCaidaCamara)
            {
                tiempo += Time.deltaTime;

                float t = Mathf.Clamp01(
                    tiempo / duracionCaidaCamara
                );

                // Suavizamos el movimiento.
                float suave =
                    Mathf.SmoothStep(0f, 1f, t);

                pivotCamaraDerrota.localPosition =
                    Vector3.Lerp(
                        posicionInicial,
                        posicionFinal,
                        suave
                    );

                pivotCamaraDerrota.localRotation =
                    Quaternion.Slerp(
                        rotacionInicial,
                        rotacionFinal,
                        suave
                    );

                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(
                duracionAntesDelNegro
            );
        }

        // ==========================================
        // 5. FADE A NEGRO + FADE DE AUDIO
        // ==========================================

        // Guardamos los volúmenes originales antes de empezar.
        float volumenInicialBase1 =
            baseEnemy1 != null ? baseEnemy1.volume : 0f;

        float volumenInicialBase2 =
            baseEnemy2 != null ? baseEnemy2.volume : 0f;

        float volumenInicialVoces =
            enemyVoices != null ? enemyVoices.volume : 0f;

        float volumenInicialContacto =
            audioContacto != null ? audioContacto.volume : 0f;


        if (fadeNegro != null)
        {
            float tiempo = 0f;

            while (tiempo < duracionFadeNegro)
            {
                tiempo += Time.deltaTime;

                float t = Mathf.Clamp01(
                    tiempo / duracionFadeNegro
                );

                // Pantalla hacia negro
                fadeNegro.alpha = t;


                // ======================================
                // BASES DE COMBATE:
                // bajan pero NO desaparecen
                // ======================================

                if (baseEnemy1 != null)
                {
                    baseEnemy1.volume = Mathf.Lerp(
                        volumenInicialBase1,
                        volumenBaseEnemy1Derrota,
                        t
                    );
                }

                if (baseEnemy2 != null)
                {
                    baseEnemy2.volume = Mathf.Lerp(
                        volumenInicialBase2,
                        volumenBaseEnemy2Derrota,
                        t
                    );
                }


                // ======================================
                // VOCES:
                // desaparecen gradualmente
                // ======================================

                if (enemyVoices != null)
                {
                    enemyVoices.volume = Mathf.Lerp(
                        volumenInicialVoces,
                        0f,
                        t
                    );
                }


                // ======================================
                // CONTACTO:
                // desaparece gradualmente
                // ======================================

                if (audioContacto != null)
                {
                    audioContacto.volume = Mathf.Lerp(
                        volumenInicialContacto,
                        0f,
                        t
                    );
                }

                yield return null;
            }

            fadeNegro.alpha = 1f;
        }

        // ==========================================
        // 6. AUDIO AL QUEDAR EN NEGRO
        // ==========================================

        // Las bases continúan sonando a volumen bajo.
        // NO hacemos Stop().

        if (baseEnemy1 != null)
            baseEnemy1.volume = volumenBaseEnemy1Derrota;

        if (baseEnemy2 != null)
            baseEnemy2.volume = volumenBaseEnemy2Derrota;


        // Las voces y el sonido de contacto sí terminan.
        if (enemyVoices != null)
        {
            enemyVoices.volume = 0f;
            enemyVoices.Stop();
        }

        if (audioContacto != null)
        {
            audioContacto.volume = 0f;
            audioContacto.Stop();
        }

        // ==========================================
        // 7. MOSTRAR DERROTA
        // ==========================================

        ResolverCanvasDerrota();

        if (EsObjetoDeEscena(canvasDerrota))
        {
            canvasDerrota.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "No se encontró DefeatCanvas."
            );
        }

        // Ahora sí congelamos.
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResolverCanvasDerrota()
    {
        if (EsObjetoDeEscena(canvasDerrota))
            return;

        Scene escenaActual = gameObject.scene;

        DefeatScreenAnimator[] pantallasDerrota =
            Resources.FindObjectsOfTypeAll<DefeatScreenAnimator>();

        foreach (
            DefeatScreenAnimator pantallaDerrota
            in pantallasDerrota
        )
        {
            if (pantallaDerrota == null)
                continue;

            GameObject objetoPantalla =
                pantallaDerrota.gameObject;

            if (
                objetoPantalla.scene == escenaActual &&
                objetoPantalla.scene.isLoaded
            )
            {
                canvasDerrota = objetoPantalla;
                return;
            }
        }
    }

    private bool EsObjetoDeEscena(GameObject objeto)
    {
        return
            objeto != null &&
            objeto.scene.IsValid() &&
            objeto.scene.isLoaded;
    }
}