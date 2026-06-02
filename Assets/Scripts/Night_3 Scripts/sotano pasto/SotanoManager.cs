using System.Collections;
using TMPro;
using UnityEngine;

public class SotanoManager : MonoBehaviour
{
    public static SotanoManager Instance;

    [Header("Objetivos")]
    public TextMeshProUGUI textoObjetivo;

    [Header("UI")]
    public TextMeshProUGUI textoSubtitulos;

    public GameObject pantallaNegra;
    public GameObject paranoia;

    public GameObject panelInteraccion;
    public TextMeshProUGUI textoInteraccion;

    private Coroutine dialogoActual;

    public int interaccionesCompletadas = 0;


    public void ActualizarObjetivo(string nuevoObjetivo)
    {
        if (textoObjetivo != null)
        {
            textoObjetivo.text = "- " + nuevoObjetivo;
        }
    }


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ParanoiaSystem.Instance.AddParanoia(0f);

        StartCoroutine(MantenerParanoiaMinima());

        StartCoroutine(InicioSotano());
    }

    void Update()
    {
        Ray ray = new Ray(
            Camera.main.transform.position,
            Camera.main.transform.forward
        );

        RaycastHit hit;

        bool mirandoAlgo = false;

        if (Physics.Raycast(ray, out hit, 10f))
        {
            SimpleInteract interactuable =
                hit.collider.GetComponentInParent<SimpleInteract>();

            if (interactuable != null)
            {
                mirandoAlgo = true;

                panelInteraccion.SetActive(true);
                textoInteraccion.text = "Hablar";
            }
        }

        if (!mirandoAlgo)
        {
            panelInteraccion.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Physics.Raycast(ray, out hit, 10f))
            {
                SimpleInteract interactuable =
                    hit.collider.GetComponentInParent<SimpleInteract>();

                if (interactuable != null)
                {
                    interactuable.Interact();
                    return;
                }
            }
        }
    }


    // DIÁLOGOS


    public void MostrarDialogo(string mensaje)
    {
        if (textoSubtitulos == null)
            return;

        if (dialogoActual != null)
            StopCoroutine(dialogoActual);

        textoSubtitulos.text = mensaje;

        dialogoActual = StartCoroutine(LimpiarDialogo());
    }

    IEnumerator LimpiarDialogo()
    {
        yield return new WaitForSeconds(4f);

        textoSubtitulos.text = "";
    }

   
    // INICIO DEL SÓTANO
    

    IEnumerator InicioSotano()
    {
        yield return new WaitForSeconds(1f);

        ActualizarObjetivo("vuelve con ellas.");

        MostrarDialogo(
            "solo un poco mas."
        );
    }

        
    // PARANOIA
    

    IEnumerator MantenerParanoiaMinima()
    {
        while (true)
        {
            if (ParanoiaSystem.Instance != null)
            {
                ParanoiaSystem.Instance.AddParanoia(1f);
            }

            yield return new WaitForSeconds(5f);
        }
    }

    public void InteraccionCompletada()
    {
        interaccionesCompletadas++;

        Debug.Log("Interacciones: " + interaccionesCompletadas);

        if (interaccionesCompletadas == 1)
        {
            StartCoroutine(AvanzarSotano());
        }
    }

    IEnumerator AvanzarSotano()
    {
        MostrarDialogo("Perdón por la tardanza... el turno se hizo eterno. Ya podemos irnos");

        yield return new WaitForSeconds(4f);

        pantallaNegra.SetActive(true);

        paranoia.SetActive(false);
    }
}