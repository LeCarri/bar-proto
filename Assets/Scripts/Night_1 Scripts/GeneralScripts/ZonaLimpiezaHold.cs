using UnityEngine;

public enum TipoZonaLimpieza { Mesa, Suelo }

public class ZonaLimpiezaHold : MonoBehaviour, IInteractable
{
    [Header("Tipo de Limpieza")]
    [SerializeField] private TipoZonaLimpieza tipoZona;
    [SerializeField] private float tiempoNecesario = 2.5f; // Segundos requeridos

    [Header("Visuales")]
    [Tooltip("El Renderer de la mancha o mugre.")]
    [SerializeField] private Renderer rendererSuciedad;

    private float progresoActual = 0f;
    private bool estaCompletado = false;
    private bool estaLimpiando = false;
    private Material materialInstanciado;

    private void Start()
    {
        if (rendererSuciedad != null)
        {
            materialInstanciado = rendererSuciedad.material;
        }
    }

    private void Update()
    {
        if (estaCompletado) return;

        // Si estamos interactuando y el jugador MANTIENE la tecla E presionada
        if (estaLimpiando && Input.GetKey(KeyCode.E))
        {
            progresoActual += Time.deltaTime;

            // Calculamos porcentaje de 0 a 1
            float porcentaje = Mathf.Clamp01(progresoActual / tiempoNecesario);

            // Reducimos el Alpha gradualmente
            ActualizarTransparencia(1f - porcentaje);

            // Completado al llegar al tiempo objetivo
            if (progresoActual >= tiempoNecesario)
            {
                CompletarLimpieza();
            }
        }
        else
        {
            // Si soltó la E o dejó de mirarlo, reseteamos el estado de limpieza activa
            estaLimpiando = false;
        }
    }

    public bool CanInteract() => !estaCompletado;

    public string GetDescription()
    {
        return tipoZona == TipoZonaLimpieza.Mesa ? "Limpiar mesa" : "Barrer suelo";
    }

    public void Interact()
    {
        if (estaCompletado) return;

        // 1. Validaciones en Act1Manager
        if (Act1Manager.Instance != null)
        {
            if (tipoZona == TipoZonaLimpieza.Mesa && !Act1Manager.Instance.tieneTrapo)
            {
                Act1Manager.Instance.MostrarDialogo("Necesito el trapo para limpiar la mesa.");
                return;
            }
            if (tipoZona == TipoZonaLimpieza.Suelo && !Act1Manager.Instance.tieneEscoba)
            {
                Act1Manager.Instance.MostrarDialogo("Necesito la escoba para barrer esto.");
                return;
            }
        }

        // 2. Si pasó las validaciones, habilitamos la bandera para que el Update procese el Hold
        estaLimpiando = true;
    }

    private void ActualizarTransparencia(float alfa)
    {
        if (materialInstanciado != null && materialInstanciado.HasProperty("_Color"))
        {
            Color colorActual = materialInstanciado.color;
            colorActual.a = alfa;
            materialInstanciado.color = colorActual;
        }
    }

    private void CompletarLimpieza()
    {
        estaCompletado = true;
        estaLimpiando = false;

        // 1. Ocultamos el objeto visual de la mancha o suciedad
        if (rendererSuciedad != null)
        {
            rendererSuciedad.gameObject.SetActive(false);
        }

        // 2. Apagamos el Collider para no seguir interactuando
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 3. Llamamos a los métodos que SÍ existen en tu Act1Manager
        if (Act1Manager.Instance != null)
        {
            if (tipoZona == TipoZonaLimpieza.Suelo)
            {
                Act1Manager.Instance.RegistrarZonaBarrida();
            }
            else if (tipoZona == TipoZonaLimpieza.Mesa)
            {
                Act1Manager.Instance.RegistrarMesasLimpias();
            }
        }
    }
}