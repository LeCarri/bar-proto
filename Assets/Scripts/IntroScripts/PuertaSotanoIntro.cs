using UnityEngine;

/// <summary>
/// Maneja la apertura opuesta de una puerta doble de sótano en el piso.
/// </summary>
public class PuertaSotanoIntro : MonoBehaviour
{
    public enum EjeRotacion { EjeX, EjeZ }

    [Header("Referencias Hojas de Puerta")]
    [SerializeField] private Transform hojaIzquierda;
    [SerializeField] private Transform hojaDerecha;

    [Header("Ajustes de Eje y Apertura")]
    [Tooltip("Seleccioná el eje sobre el que se levantan las trampillas (probá X o Z)")]
    [SerializeField] private EjeRotacion ejeApertura = EjeRotacion.EjeX;

    [Tooltip("Ángulo de apertura hacia arriba (ej: 80 u 85 grados)")]
    [SerializeField] private float anguloApertura = 85f;

    [Tooltip("Velocidad de apertura (grados por segundo)")]
    [SerializeField] private float velocidadApertura = 50f;

    [Header("Invertir Dirección")]
    [Tooltip("Si las puertas abren hacia abajo en lugar de levantarse hacia arriba")]
    [SerializeField] private bool invertirSentido = false;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSourcePuerta;
    [SerializeField] private AudioClip clipAbrirPuerta;

    private bool abriendo = false;
    private Quaternion rotacionObjetivoIzq;
    private Quaternion rotacionObjetivoDer;

    private void Update()
    {
        if (!abriendo) return;

        // Mover hoja izquierda
        if (hojaIzquierda != null)
        {
            hojaIzquierda.localRotation = Quaternion.RotateTowards(
                hojaIzquierda.localRotation,
                rotacionObjetivoIzq,
                velocidadApertura * Time.deltaTime
            );
        }

        // Mover hoja derecha
        if (hojaDerecha != null)
        {
            hojaDerecha.localRotation = Quaternion.RotateTowards(
                hojaDerecha.localRotation,
                rotacionObjetivoDer,
                velocidadApertura * Time.deltaTime
            );
        }

        // Verificar si ambas terminaron de rotar
        bool izqLista = (hojaIzquierda == null) || (Quaternion.Angle(hojaIzquierda.localRotation, rotacionObjetivoIzq) < 0.1f);
        bool derLista = (hojaDerecha == null) || (Quaternion.Angle(hojaDerecha.localRotation, rotacionObjetivoDer) < 0.1f);

        if (izqLista && derLista)
        {
            abriendo = false;
        }
    }

    public void AbrirPuertas()
    {
        if (abriendo) return;

        float angulo = invertirSentido ? -anguloApertura : anguloApertura;

        // IMPORTANTE: Un lado recibe +ángulo y el otro -ángulo para abrir hacia afuera
        Vector3 rotIzq = (ejeApertura == EjeRotacion.EjeX) ? new Vector3(angulo, 0f, 0f) : new Vector3(0f, 0f, angulo);
        Vector3 rotDer = (ejeApertura == EjeRotacion.EjeX) ? new Vector3(-angulo, 0f, 0f) : new Vector3(0f, 0f, -angulo);

        if (hojaIzquierda != null)
        {
            rotacionObjetivoIzq = hojaIzquierda.localRotation * Quaternion.Euler(rotIzq);
        }

        if (hojaDerecha != null)
        {
            rotacionObjetivoDer = hojaDerecha.localRotation * Quaternion.Euler(rotDer);
        }

        if (audioSourcePuerta != null && clipAbrirPuerta != null)
        {
            audioSourcePuerta.PlayOneShot(clipAbrirPuerta);
        }
        else if (audioSourcePuerta != null)
        {
            // Si el clip ya está asignado directamente dentro del componente AudioSource
            audioSourcePuerta.Play();
        }

        abriendo = true;
    }
}