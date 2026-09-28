using UnityEngine;

public class ManchaSangre : MonoBehaviour
{
    [Header("Configuración")]
    public float tiempoLimpieza = 3f;

    [Header("Tipo de mancha")]
    public bool esManchaPared = true;

    [Header("Evento especial")]
    public GameObject marcasUnas;

    // ANIMACIÓN DE MOPA

    [Header("Animación de mopa")]
    public GameObject visualMopa;
    public Animator animatorMopa;
    public string estadoLimpieza = "LimpiarSuelo";


    //AUDIO LIMPIEZA

    [Header("Audio de limpieza")]
    public AudioSource audioMopa;
    public AudioClip sonidoLimpiezaLargo;

    [Range(0f, 1f)]
    public float volumenLimpieza = 0.8f;


    // VARIABLES DE LIMPIEZA

    private float tiempoActual = 0f;
    private bool limpiando = false;
    private bool limpiada = false;


    // VARIABLES DE ANIMACIÓN

    private bool animacionActiva = false;
    private bool detenerAlFinalizarCiclo = false;

    private float tiempoAnimacion = 0f;
    private float duracionCiclo = 0f;


    // MATERIAL

    private Renderer rend;
    private Material material;

    private Color colorInicial;


    // START

    void Start()
    {
        rend = GetComponent<Renderer>();

        if (rend != null)
        {
            material = rend.material;
            colorInicial = material.color;
        }

        Debug.Log(
    "[SANGRE] Shader: " +
    material.shader.name
);

        if (material.HasProperty("_BaseColor"))
        {
            Debug.Log("[SANGRE] Tiene propiedad _BaseColor.");
        }

        if (material.HasProperty("_Color"))
        {
            Debug.Log("[SANGRE] Tiene propiedad _Color.");
        }

        if (material.HasProperty("_Opacity"))
        {
            Debug.Log("[SANGRE] Tiene propiedad _Opacity.");
        }

        // Las marcas empiezan ocultas
        if (marcasUnas != null)
        {
            marcasUnas.SetActive(false);
        }


        // La mopa empieza oculta
        // SOLO para manchas de piso
        if (
            !esManchaPared &&
            visualMopa != null
        )
        {
            visualMopa.SetActive(false);
        }

        if (audioMopa != null)
        {
            audioMopa.playOnAwake = false;
            audioMopa.loop = true;
        }
    }


    // UPDATE

    void Update()
    {
        if (limpiada)
            return;


        // LIMPIEZA
        if (limpiando)
        {
            // Si se suelta E
            if (!Input.GetKey(KeyCode.E))
            {
                DetenerLimpieza();
            }
            else
            {
                tiempoActual += Time.deltaTime;

                float progreso =
                    tiempoActual / tiempoLimpieza;

                progreso =
                    Mathf.Clamp01(progreso);


                // Desvanecer mancha
                if (material != null)
                {
                    Color nuevoColor =
                        colorInicial;

                    nuevoColor.a =
                        Mathf.Lerp(
                            1f,
                            0f,
                            progreso
                        );

                    material.color =
                        nuevoColor;
                }


                // Terminar limpieza
                if (
                    tiempoActual >=
                    tiempoLimpieza
                )
                {
                    TerminarLimpieza();
                }
            }
        }


        // Actualizar animación de mopa
        ActualizarAnimacionMopa();

        ActualizarAudioLimpieza();
    }


    // EMPEZAR LIMPIEZA

    public void EmpezarLimpieza()
    {
        if (limpiada)
            return;


        if (Act3Manager.Instance == null)
            return;


        if (
            !Act3Manager.Instance
                .tieneElementosLimpieza
        )
        {
            Act3Manager.Instance.MostrarDialogo(
                "Necesito buscar los elementos de limpieza primero."
            );

            return;
        }


        limpiando = true;


        // SOLO MANCHAS DE PISO
        if (!esManchaPared)
        {
            IniciarAnimacionMopa();
        }
    }


    // DETENER LIMPIEZA

    public void DetenerLimpieza()
    {
        limpiando = false;


        // No desaparece inmediatamente.
        // Primero termina el ciclo de la animación.
        if (
            !esManchaPared &&
            animacionActiva
        )
        {
            detenerAlFinalizarCiclo = true;
        }
    }


    // INICIAR ANIMACIÓN DE MOPA

    private void IniciarAnimacionMopa()
    {
        if (
            esManchaPared ||
            visualMopa == null ||
            animatorMopa == null
        )
        {
            return;
        }


        // Si ya está animándose,
        // simplemente seguimos el loop.
        if (animacionActiva)
        {
            detenerAlFinalizarCiclo = false;
            return;
        }


        visualMopa.SetActive(true);


        animatorMopa.speed = 1f;

        animatorMopa.Play(
            estadoLimpieza,
            0,
            0f
        );

        animatorMopa.Update(0f);


        AnimatorStateInfo estado =
            animatorMopa
                .GetCurrentAnimatorStateInfo(0);


        duracionCiclo =
            estado.length;


        tiempoAnimacion = 0f;

        animacionActiva = true;

        detenerAlFinalizarCiclo = false;
    }


    // ACTUALIZAR ANIMACIÓN

    private void ActualizarAnimacionMopa()
    {
        if (
            esManchaPared ||
            !animacionActiva ||
            animatorMopa == null
        )
        {
            return;
        }


        if (duracionCiclo <= 0f)
            return;


        tiempoAnimacion +=
            Time.deltaTime;


        bool mantenerLoop =
            limpiando &&
            Input.GetKey(KeyCode.E) &&
            !limpiada;


        detenerAlFinalizarCiclo =
            !mantenerLoop;


        if (
            tiempoAnimacion >=
            duracionCiclo
        )
        {
            if (detenerAlFinalizarCiclo)
            {
                FinalizarAnimacionMopa();
                return;
            }


            // Comenzar otro ciclo
            tiempoAnimacion -=
                duracionCiclo;
        }
    }


    // FINALIZAR ANIMACIÓN

    private void FinalizarAnimacionMopa()
    {
        animacionActiva = false;

        detenerAlFinalizarCiclo = false;

        tiempoAnimacion = 0f;


        if (animatorMopa != null)
        {
            animatorMopa.speed = 1f;
        }


        if (visualMopa != null)
        {
            visualMopa.SetActive(false);
        }
    }

    // AUDIO DE LIMPIEZA

    private void ActualizarAudioLimpieza()
    {
        // Las manchas de pared no usan sonido de mopa
        if (esManchaPared)
            return;

        if (
            audioMopa == null ||
            sonidoLimpiezaLargo == null
        )
        {
            return;
        }


        bool debeSonar =
            limpiando &&
            Input.GetKey(KeyCode.E) &&
            !limpiada;


        if (debeSonar)
        {
            if (!audioMopa.isPlaying)
            {
                audioMopa.clip =
                    sonidoLimpiezaLargo;

                audioMopa.volume =
                    volumenLimpieza;

                audioMopa.loop = true;

                audioMopa.Play();
            }
        }
        else
        {
            DetenerAudioLimpieza();
        }
    }


    private void DetenerAudioLimpieza()
    {
        if (
            audioMopa != null &&
            audioMopa.isPlaying
        )
        {
            audioMopa.Stop();
        }
    }


    // TERMINAR LIMPIEZA

    void TerminarLimpieza()
    {
        limpiando = false;
        limpiada = true;


        // Asegura que la mopa desaparezca
        if (!esManchaPared)
        {
            FinalizarAnimacionMopa();
        }


        // Registrar limpieza
        if (Act3Manager.Instance != null)
        {
            if (esManchaPared)
            {
                Act3Manager.Instance
                    .ManchaParedLimpiada();
            }
            else
            {
                Act3Manager.Instance
                    .ManchaPisoLimpiada();
            }
        }


        // Mostrar marcas de uñas
        if (marcasUnas != null)
        {
            marcasUnas.SetActive(true);
        }


        // Desactivar mancha
        gameObject.SetActive(false);
    }



    // AL DESACTIVARSE

    private void OnDisable()
    {
        limpiando = false;

        DetenerAudioLimpieza();

        if (!esManchaPared)
        {
            FinalizarAnimacionMopa();
        }
    }
}
