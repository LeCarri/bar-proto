using UnityEngine;
using System.Collections;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — "6. EL VIGILANTE". Una aparición = un trigger con este script.
///
/// Guion:
///  Aparición 0: al salir del cubículo, el Vigilante está en la puerta de entrada del baño mirándonos.
///               2-3 segundos y se va rápidamente hacia afuera del baño.
///  Aparición 1: vamos a la puerta para salir y lo vemos dentro del baño de mujeres mirándonos.
///               Un par de segundos y sale disparado hacia los cubículos. Si lo seguimos no hay nada.
///  → Al terminar la ÚLTIMA aparición empieza el combate con las Sombras en el salón.
///
/// SETUP (por cada aparición):
///  - GameObject vacío + Box Collider "Is Trigger" + este script.
///    Aparición 0: el trigger va en la salida del cubículo fuera de servicio.
///    Aparición 1: el trigger va en la puerta del baño de hombres.
///  - modeloVigilante: el MISMO modelo del Vigilante para las dos (empieza desactivado).
///  - puntoAparicion: dónde se para. puntoHuida: adónde sale corriendo (ahí desaparece).
///  - ordenAparicion: 0 para la primera, 1 para la segunda. Marcar "esLaUltima" en la última.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AparicionVigilanteReestructura : MonoBehaviour
{
    [Header("Orden")]
    public int ordenAparicion = 0;
    public bool esLaUltima = false;

    [Header("Vigilante")]
    public GameObject modeloVigilante;
    public Transform puntoAparicion;
    public Transform puntoHuida;
    [Tooltip("Si el modelo queda de costado o de espaldas, corregilo acá (90, -90, 180).")]
    public float correccionY = 0f;

    [Header("Tiempos")]
    public float segundosMirando = 2.5f;
    public float velocidadHuida = 9f;

    [Header("Audio / Paranoia")]
    public AudioSource sonidoAparicion;
    public AudioSource sonidoHuida;
    public float paranoia = 10f;

    private bool usado;
    private Transform jugador;

    void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.transform;
    }

    void OnTriggerEnter(Collider other) => Probar(other);
    void OnTriggerStay(Collider other) => Probar(other);

    void Probar(Collider other)
    {
        if (usado || !other.CompareTag("Player")) return;
        Act2ManagerReestructura m = Act2ManagerReestructura.Instance;
        if (m == null || !m.PuedeAparecerVigilante(ordenAparicion)) return;

        usado = true;
        StartCoroutine(Aparicion(m));
    }

    IEnumerator Aparicion(Act2ManagerReestructura m)
    {
        if (modeloVigilante == null || puntoAparicion == null)
        {
            Debug.LogError($"[AparicionVigilanteReestructura #{ordenAparicion}] '{name}': falta 'Modelo Vigilante' o 'Punto Aparicion'.");
            m.AparicionVigilanteTerminada(ordenAparicion, esLaUltima);
            yield break;
        }

        Transform v = modeloVigilante.transform;
        v.position = puntoAparicion.position;
        MirarHacia(v, jugador != null ? jugador.position : puntoAparicion.position + puntoAparicion.forward);
        modeloVigilante.SetActive(true);

        if (sonidoAparicion != null) sonidoAparicion.Play();
        m.SumarParanoia(paranoia);

        // Nos mira
        float t = 0f;
        while (t < segundosMirando)
        {
            t += Time.deltaTime;
            if (jugador != null) MirarHacia(v, jugador.position);
            yield return null;
        }

        // Sale disparado
        if (sonidoHuida != null) sonidoHuida.Play();
        if (puntoHuida != null)
        {
            MirarHacia(v, puntoHuida.position);
            while (Vector3.Distance(v.position, puntoHuida.position) > 0.1f)
            {
                v.position = Vector3.MoveTowards(v.position, puntoHuida.position, velocidadHuida * Time.deltaTime);
                yield return null;
            }
        }

        modeloVigilante.SetActive(false);
        m.AparicionVigilanteTerminada(ordenAparicion, esLaUltima);
    }

    void MirarHacia(Transform v, Vector3 punto)
    {
        Vector3 dir = punto - v.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        v.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, correccionY, 0f);
    }
}
