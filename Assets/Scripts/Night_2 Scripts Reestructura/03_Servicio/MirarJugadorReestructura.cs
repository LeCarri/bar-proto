using UnityEngine;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Glitch del cliente corrupto: "vemos a todos los clientes del bar que se
/// voltean para mirarnos". Poner este script en CADA cliente (o en su cabeza).
/// El Act2ManagerReestructura los encuentra solos.
///
/// Durante el glitch giran hacia el jugador; cuando "vuelve todo a la normalidad" regresan
/// a su rotación original.
/// </summary>
public class MirarJugadorReestructura : MonoBehaviour
{
    [Tooltip("Qué gira. Vacío = este mismo objeto. Podés poner la cabeza del modelo.")]
    public Transform parteQueGira;
    [Tooltip("Grados por segundo.")]
    public float velocidadGiro = 240f;
    [Tooltip("Si es true solo gira en el eje Y (no se inclina).")]
    public bool soloEjeY = true;
    [Tooltip("Si el modelo queda mirando de costado o de espaldas, corregilo acá (90, -90, 180).")]
    public float correccionY = 0f;

    private Quaternion rotacionOriginal;
    private Transform jugador;
    private bool mirando;
    private bool volviendo;

    void Awake()
    {
        if (parteQueGira == null) parteQueGira = transform;
        rotacionOriginal = parteQueGira.rotation;
    }

    public void MirarAlJugador()
    {
        if (jugador == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) jugador = p.transform;
        }
        rotacionOriginal = parteQueGira.rotation;
        mirando = true;
        volviendo = false;
    }

    /// <summary>Vuelve a su rotación original de a poco.</summary>
    public void VolverANormal()
    {
        mirando = false;
        volviendo = true;
    }

    /// <summary>Vuelve al instante (al final del glitch "vuelve todo a la normalidad").</summary>
    public void VolverAlInstante()
    {
        mirando = false;
        volviendo = false;
        parteQueGira.rotation = rotacionOriginal;
    }

    void Update()
    {
        if (mirando && jugador != null)
        {
            Vector3 dir = jugador.position - parteQueGira.position;
            if (soloEjeY) dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;
            Quaternion objetivo = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, correccionY, 0f);
            parteQueGira.rotation = Quaternion.RotateTowards(parteQueGira.rotation, objetivo, velocidadGiro * Time.deltaTime);
        }
        else if (volviendo)
        {
            parteQueGira.rotation = Quaternion.RotateTowards(parteQueGira.rotation, rotacionOriginal, velocidadGiro * Time.deltaTime);
            if (Quaternion.Angle(parteQueGira.rotation, rotacionOriginal) < 0.5f) volviendo = false;
        }
    }
}
