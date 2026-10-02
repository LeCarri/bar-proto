
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum TipoZonaLimpieza
{
    Mesa,
    Suelo
}

public class ZonaLimpiezaHold : MonoBehaviour, IInteractable
{
    [Header("Tipo de limpieza")]
    [SerializeField] private TipoZonaLimpieza tipoZona;
    [SerializeField] private float tiempoNecesario = 2.5f;

    [Header("Visuales de suciedad")]
    [SerializeField] private Renderer rendererSuciedad;

    [Tooltip("Para manchas individuales, como las del suelo.")]
    [SerializeField] private DecalProjector decalSuciedad;

    [Tooltip("Para conjuntos de manchas, como una mesa.")]
    [SerializeField] private DecalProjector[] decalsSuciedad;

    [Header("Animacion de mopa")]
    [SerializeField] private GameObject visualMopa;
    [SerializeField] private Animator animatorMopa;
    [SerializeField] private string estadoLimpieza = "LimpiarSuelo";

    [Header("Audio de limpieza")]
    [SerializeField] private AudioSource audioMopa;
    [SerializeField] private AudioClip sonidoLimpiezaLargo;

    [Range(0f, 1f)]
    [SerializeField] private float volumenLimpieza = 0.8f;

    private float progresoActual;
    private bool estaCompletado;
    private bool estaLimpiando;

    private bool animacionActiva;
    private bool detenerAlFinalizarCiclo;

    private float tiempoAnimacion;
    private float duracionCiclo;

    private Material materialInstanciado;
    private float alfaInicialRenderer = 1f;

    private float opacidadInicialDecal = 1f;
    private float[] opacidadesIniciales;

    // ==========================================
    // INICIALIZACION
    // ==========================================

    private void Start()
    {
        if (rendererSuciedad != null)
        {
            materialInstanciado = rendererSuciedad.material;

            if (materialInstanciado.HasProperty("_BaseColor"))
            {
                alfaInicialRenderer =
                    materialInstanciado.GetColor("_BaseColor").a;
            }
            else if (materialInstanciado.HasProperty("_Color"))
            {
                alfaInicialRenderer = materialInstanciado.color.a;
            }
        }

        if (decalSuciedad != null)
        {
            opacidadInicialDecal = decalSuciedad.fadeFactor;
        }

        // Guardar la opacidad original de cada decal del conjunto.
        if (decalsSuciedad != null)
        {
            opacidadesIniciales = new float[decalsSuciedad.Length];

            for (int i = 0; i < decalsSuciedad.Length; i++)
            {
                if (decalsSuciedad[i] != null)
                {
                    opacidadesIniciales[i] =
                        decalsSuciedad[i].fadeFactor;
                }
            }
        }

        if (tipoZona == TipoZonaLimpieza.Suelo &&
            visualMopa != null)
        {
            visualMopa.SetActive(false);
        }

        if (audioMopa != null)
        {
            audioMopa.playOnAwake = false;
            audioMopa.loop = true;
        }
    }

    private void Update()
    {
        if (!estaCompletado)
        {
            ProcesarLimpieza();
        }

        ActualizarAnimacion();
        ActualizarAudioLimpieza();
    }

    // ==========================================
    // INTERACCION
    // ==========================================

    public bool CanInteract()
    {
        return !estaCompletado;
    }

    public string GetDescription()
    {
        return tipoZona == TipoZonaLimpieza.Mesa
            ? "Limpiar mesa"
            : "Limpiar suelo";
    }

    public void Interact()
    {
        if (estaCompletado)
            return;

        if (Act1Manager.Instance != null)
        {
            if (tipoZona == TipoZonaLimpieza.Mesa &&
                !Act1Manager.Instance.tieneTrapo)
            {
                Act1Manager.Instance.MostrarDialogo(
                    "Necesito el trapo para limpiar la mesa."
                );

                return;
            }

            if (tipoZona == TipoZonaLimpieza.Suelo &&
                !Act1Manager.Instance.tieneEscoba)
            {
                Act1Manager.Instance.MostrarDialogo(
                    "Necesito la mopa para limpiar esto."
                );

                return;
            }
        }

        estaLimpiando = true;

        if (tipoZona == TipoZonaLimpieza.Suelo)
        {
            IniciarAnimacionMopa();
        }
    }

    // ==========================================
    // PROGRESO DE LIMPIEZA
    // ==========================================

    private void ProcesarLimpieza()
    {
        if (estaLimpiando && Input.GetKey(KeyCode.E))
        {
            if (tipoZona == TipoZonaLimpieza.Suelo)
            {
                IniciarAnimacionMopa();
            }

            progresoActual += Time.deltaTime;

            float porcentaje = Mathf.Clamp01(
                progresoActual / Mathf.Max(0.01f, tiempoNecesario)
            );

            ActualizarTransparencia(1f - porcentaje);

            if (progresoActual >= tiempoNecesario)
            {
                CompletarLimpieza();
            }
        }
        else
        {
            estaLimpiando = false;

            if (animacionActiva)
            {
                detenerAlFinalizarCiclo = true;
            }
        }
    }

    // ==========================================
    // ANIMACION DE MOPA
    // ==========================================

    private void IniciarAnimacionMopa()
    {
        if (visualMopa == null || animatorMopa == null)
            return;

        if (animacionActiva)
        {
            detenerAlFinalizarCiclo = false;
            return;
        }

        visualMopa.SetActive(true);

        animatorMopa.speed = 1f;
        animatorMopa.Play(estadoLimpieza, 0, 0f);
        animatorMopa.Update(0f);

        AnimatorStateInfo estado =
            animatorMopa.GetCurrentAnimatorStateInfo(0);

        duracionCiclo = estado.length;
        tiempoAnimacion = 0f;

        animacionActiva = true;
        detenerAlFinalizarCiclo = false;
    }

    private void ActualizarAnimacion()
    {
        if (!animacionActiva || animatorMopa == null)
            return;

        if (duracionCiclo <= 0f)
            return;

        tiempoAnimacion += Time.deltaTime;

        bool mantenerLoop =
            estaLimpiando &&
            Input.GetKey(KeyCode.E) &&
            !estaCompletado;

        detenerAlFinalizarCiclo = !mantenerLoop;

        if (tiempoAnimacion >= duracionCiclo)
        {
            if (detenerAlFinalizarCiclo)
            {
                FinalizarAnimacion();
                return;
            }

            tiempoAnimacion -= duracionCiclo;
        }
    }

    private void FinalizarAnimacion()
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

    // ==========================================
    // AUDIO DE LIMPIEZA
    // ==========================================

    private void ActualizarAudioLimpieza()
    {
        if (audioMopa == null || sonidoLimpiezaLargo == null)
            return;

        bool debeSonar =
            estaLimpiando &&
            Input.GetKey(KeyCode.E) &&
            !estaCompletado;

        if (debeSonar)
        {
            if (!audioMopa.isPlaying)
            {
                audioMopa.clip = sonidoLimpiezaLargo;
                audioMopa.volume = volumenLimpieza;
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
        if (audioMopa != null && audioMopa.isPlaying)
        {
            audioMopa.Stop();
        }
    }

    // ==========================================
    // TRANSPARENCIA DE SUCIEDAD
    // ==========================================

    private void ActualizarTransparencia(float factor)
    {
        // Decal individual
        if (decalSuciedad != null)
        {
            decalSuciedad.fadeFactor =
                opacidadInicialDecal * factor;
        }

        // Conjunto de decals
        if (decalsSuciedad != null &&
            opacidadesIniciales != null)
        {
            for (int i = 0; i < decalsSuciedad.Length; i++)
            {
                if (decalsSuciedad[i] != null)
                {
                    decalsSuciedad[i].fadeFactor =
                        opacidadesIniciales[i] * factor;
                }
            }
        }

        // Renderer tradicional
        if (materialInstanciado != null)
        {
            if (materialInstanciado.HasProperty("_BaseColor"))
            {
                Color color =
                    materialInstanciado.GetColor("_BaseColor");

                color.a = alfaInicialRenderer * factor;

                materialInstanciado.SetColor("_BaseColor", color);
            }
            else if (materialInstanciado.HasProperty("_Color"))
            {
                Color color = materialInstanciado.color;

                color.a = alfaInicialRenderer * factor;

                materialInstanciado.color = color;
            }
        }
    }

    // ==========================================
    // COMPLETAR LIMPIEZA
    // ==========================================

    private void CompletarLimpieza()
    {
        estaCompletado = true;
        estaLimpiando = false;

        DetenerAudioLimpieza();

        detenerAlFinalizarCiclo = true;

        // Desactivar decal individual.
        if (decalSuciedad != null)
        {
            decalSuciedad.enabled = false;
        }

        // Desactivar todos los decals del conjunto.
        if (decalsSuciedad != null)
        {
            foreach (DecalProjector decal in decalsSuciedad)
            {
                if (decal != null)
                {
                    decal.enabled = false;
                }
            }
        }

        if (rendererSuciedad != null)
        {
            rendererSuciedad.enabled = false;
        }

        Collider col = GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = false;
        }

        // Registrar UNA tarea por zona, no una por decal.
        if (Act1Manager.Instance != null)
        {
            if (tipoZona == TipoZonaLimpieza.Suelo)
            {
                Act1Manager.Instance.RegistrarZonaBarrida();
            }
            else
            {
                Act1Manager.Instance.RegistrarMesasLimpias();
            }
        }
    }

    private void OnDisable()
    {
        estaLimpiando = false;

        DetenerAudioLimpieza();

        if (tipoZona == TipoZonaLimpieza.Suelo)
        {
            FinalizarAnimacion();
        }
    }
}
