using System.Collections;
using TMPro;
using UnityEngine;

public class SotanoManager : MonoBehaviour
{
    public static SotanoManager Instance;

    [Header("Objetivos")]
    public TextMeshProUGUI textoObjetivo;

    [Header("Subtítulos")]
    public TextMeshProUGUI textoSubtitulos;

    [Header("Final del sótano")]
    public GameObject pantallaNegra;
    public GameObject paranoia;

    [Header("Interacción")]
    public GameObject panelInteraccion;
    public TextMeshProUGUI textoInteraccion;

    private Coroutine dialogoActual;

    public int interaccionesCompletadas = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Camera.main == null)
            return;

        Ray ray = new Ray(
            Camera.main.transform.position,
            Camera.main.transform.forward
        );

        bool mirandoAlgo = false;

        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            SimpleInteract interactuable =
                hit.collider.GetComponentInParent<SimpleInteract>();

            if (interactuable != null)
            {
                mirandoAlgo = true;

                if (panelInteraccion != null)
                    panelInteraccion.SetActive(true);

                if (textoInteraccion != null)
                    textoInteraccion.text = "Interactuar";

                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactuable.Interact();
                    return;
                }
            }
        }

        if (!mirandoAlgo && panelInteraccion != null)
            panelInteraccion.SetActive(false);
    }

    public void ActualizarObjetivo(string nuevoObjetivo)
    {
        if (textoObjetivo != null)
            textoObjetivo.text = "- " + nuevoObjetivo;
    }

    public void MostrarDialogo(string mensaje)
    {
        if (textoSubtitulos == null)
            return;

        if (dialogoActual != null)
            StopCoroutine(dialogoActual);

        textoSubtitulos.text = mensaje;
        dialogoActual = StartCoroutine(LimpiarDialogo());
    }

    private IEnumerator LimpiarDialogo()
    {
        yield return new WaitForSeconds(4f);

        if (textoSubtitulos != null)
            textoSubtitulos.text = "";

        dialogoActual = null;
    }

    public void InteraccionCompletada()
    {
        interaccionesCompletadas++;

        Debug.Log("Interacciones: " + interaccionesCompletadas);

        if (interaccionesCompletadas == 1)
            StartCoroutine(AvanzarSotano());
    }

    private IEnumerator AvanzarSotano()
    {
        MostrarDialogo(
            "Perdon por la tardanza... el turno se hizo eterno. Ya podemos irnos"
        );

        yield return new WaitForSeconds(4f);

        if (pantallaNegra != null)
            pantallaNegra.SetActive(true);

        if (paranoia != null)
            paranoia.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}