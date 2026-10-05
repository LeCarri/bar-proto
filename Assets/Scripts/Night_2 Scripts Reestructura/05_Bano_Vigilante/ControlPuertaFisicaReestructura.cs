using UnityEngine;
using System.Collections;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Controla una puerta de cubículo que funciona con FÍSICA
/// (Rigidbody + Hinge Joint + Door.cs) SIN modificar la puerta.
///
/// IMPORTANTE: este script NO va en la puerta. Va en un GameObject vacío aparte y solo APUNTA a la
/// puerta (campo "Puerta"). En el editor la puerta queda exactamente como está. Durante el juego el
/// script la traba (Rigidbody kinematic), la abre con el motor de la bisagra o la cierra de golpe, y
/// después le devuelve su configuración original. Como la puerta se mueve por física, Door.cs hace
/// sonar la bisagra solo.
///
///  - FueraDeServicio: en las tareas queda entreabierta y FIJA (el jugador no la puede empujar).
///                     Después de leer la nota "cede" y se abre lentamente.
///  - SeCierraDeGolpe: al volver al baño (después de la nota) se abre y queda trabada abierta.
///                     Al agarrar la llave se cierra de golpe.
/// </summary>
public class ControlPuertaFisicaReestructura : MonoBehaviour
{
    [Header("Puerta (NO se modifica en el editor)")]
    public RolPuertaReestructura rol = RolPuertaReestructura.FueraDeServicio;
    [Tooltip("Arrastrá acá la puerta (el objeto que tiene Rigidbody + Hinge Joint + Door).")]
    public Rigidbody puerta;
    [Tooltip("+1 o -1: hacia qué lado se abre. Si la puerta choca contra la pared al abrirse, cambiá el signo.")]
    public int direccionApertura = 1;
    [Tooltip("Ángulo de 'abierta' (grados). Tiene que estar dentro de los límites del Hinge Joint (±90).")]
    public float anguloAbierta = 85f;

    [Header("Fuera de servicio")]
    [Tooltip("Grados que queda entreabierta durante las tareas (0 = cerrada).")]
    public float anguloEntreabierta = 15f;
    [Tooltip("Grados por segundo cuando 'cede' y se abre lentamente.")]
    public float velocidadAperturaLenta = 25f;
    [Tooltip("Si está marcado, después de abrirse se le apaga el resorte para que no vuelva a cerrarse sola.")]
    public bool quedarAbiertaDespues = true;
    [Tooltip("Cartelito \"FUERA DE SERVICIO\" (opcional). Se muestra al empezar.")]
    public GameObject cartelFueraDeServicio;
    [Tooltip("Chirrido al ceder (opcional).")]
    public AudioSource sonidoChirrido;

    [Header("Se cierra de golpe")]
    [Tooltip("Grados por segundo del portazo.")]
    public float velocidadPortazo = 900f;
    [Tooltip("Fuerza del motor de la bisagra al cerrar de golpe.")]
    public float fuerzaPortazo = 2000f;
    [Tooltip("Sonido extra de portazo (opcional: Door.cs ya hace sonar la bisagra).")]
    public AudioSource sonidoPortazo;

    [Header("Motor")]
    [Tooltip("Fuerza del motor al abrir (la puerta pesa 5 y tiene Angular Damping 6).")]
    public float fuerzaApertura = 300f;

    private HingeJoint bisagra;
    private bool origKinematic, origUseSpring, origUseMotor;
    private JointMotor origMotor;
    private bool listo;

    void Start()
    {
        if (puerta == null) { Debug.LogError($"[ControlPuertaFisicaReestructura] '{name}': falta asignar la puerta."); return; }
        bisagra = puerta.GetComponent<HingeJoint>();
        if (bisagra == null) { Debug.LogError($"[ControlPuertaFisicaReestructura] '{name}': la puerta '{puerta.name}' no tiene Hinge Joint."); return; }

        origKinematic = puerta.isKinematic;
        origUseSpring = bisagra.useSpring;
        origUseMotor = bisagra.useMotor;
        origMotor = bisagra.motor;
        listo = true;

        // Fuera de servicio: entreabierta y fija desde el principio
        if (rol == RolPuertaReestructura.FueraDeServicio)
        {
            if (cartelFueraDeServicio != null) cartelFueraDeServicio.SetActive(true);
            if (anguloEntreabierta != 0f)
                StartCoroutine(MoverConMotor(anguloEntreabierta * Signo(), 60f, fuerzaApertura, 3f, true, false));
            else
                puerta.isKinematic = true;
        }
    }

    // ─────────────────────────────────────────────────────────
    //  API para el Act2ManagerReestructura
    // ─────────────────────────────────────────────────────────

    /// <summary>FueraDeServicio: "la puerta cede. Se abre lentamente."</summary>
    public void Habilitar()
    {
        if (!listo) return;
        StopAllCoroutines();
        if (sonidoChirrido != null) sonidoChirrido.Play();
        StartCoroutine(MoverConMotor(anguloAbierta * Signo(), velocidadAperturaLenta, fuerzaApertura, 8f, false, quedarAbiertaDespues));
    }

    /// <summary>SeCierraDeGolpe: la deja abierta y trabada (los dos cubículos abiertos del regreso al baño).</summary>
    public void AbrirYTrabar()
    {
        if (!listo) return;
        StopAllCoroutines();
        StartCoroutine(MoverConMotor(anguloAbierta * Signo(), 120f, fuerzaApertura, 4f, true, false));
    }

    /// <summary>SeCierraDeGolpe: portazo al agarrar la llave.</summary>
    public void CerrarDeGolpe()
    {
        if (!listo) return;
        StopAllCoroutines();
        if (sonidoPortazo != null) sonidoPortazo.Play();
        StartCoroutine(MoverConMotor(0f, velocidadPortazo, fuerzaPortazo, 0.8f, false, false));
    }

    // ─────────────────────────────────────────────────────────
    IEnumerator MoverConMotor(float anguloObjetivo, float velocidad, float fuerza, float tiempoMax, bool trabarAlFinal, bool sinResorteAlFinal)
    {
        puerta.isKinematic = false;
        bisagra.useSpring = false;          // el resorte la tiraría de vuelta a cerrada
        puerta.WakeUp();

        JointMotor m = bisagra.motor;
        m.force = fuerza;
        m.freeSpin = false;

        float t = 0f;
        while (t < tiempoMax && Mathf.Abs(bisagra.angle - anguloObjetivo) > 2f)
        {
            m.targetVelocity = Mathf.Sign(anguloObjetivo - bisagra.angle) * velocidad;
            bisagra.motor = m;
            bisagra.useMotor = true;
            t += Time.deltaTime;
            yield return null;
        }

        // Devolver la bisagra como estaba
        bisagra.useMotor = origUseMotor;
        bisagra.motor = origMotor;
        bisagra.useSpring = sinResorteAlFinal ? false : origUseSpring;

        puerta.isKinematic = trabarAlFinal ? true : origKinematic;
    }

    int Signo() => direccionApertura >= 0 ? 1 : -1;
}
