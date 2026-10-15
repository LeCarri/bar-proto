using UnityEngine;
using System.Collections;

public class NotaPuerta : MonoBehaviour, IInteractable
{
    [Header("Estado")]
    private bool yaLeida = false;
    private bool estaAbierta = false;
    private bool puedeCerrar = false; 
    private Coroutine timerCierreCoroutine;

    [Header("Audio")]
    public AudioSource sonidoPapel;

    [Header("UI / Canvas de la Nota")]
    public GameObject canvasNotaUI;

    [Header("Objeto Físico en la Puerta")]
    public GameObject objetoNotaPuerta;

    void Start()
    {
        if (canvasNotaUI != null)
        {
            canvasNotaUI.SetActive(false);
        }
    }

    void Update()
    {
        if (estaAbierta && puedeCerrar)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Cerrando nota por tecla del usuario.");
                CerrarNota();
            }
        }
    }

    public void Interact()
    {
        if (estaAbierta) return;

        Debug.Log("¡Interact() llamado con éxito en NotaPuerta!");

        if (!yaLeida)
        {
            yaLeida = true;
            ReaccionLucas();
        }

        AbrirNota();
    }

    private void AbrirNota()
    {
        estaAbierta = true;
        puedeCerrar = false; 

        if (sonidoPapel != null) sonidoPapel.Play();

        if (canvasNotaUI != null)
        {
            canvasNotaUI.SetActive(true);
            
            Canvas canvasComp = canvasNotaUI.GetComponent<Canvas>();
            if (canvasComp == null) canvasComp = canvasNotaUI.GetComponentInChildren<Canvas>();
            if (canvasComp != null) canvasComp.sortingOrder = 999;

            Debug.Log("-> canvasNotaUI.SetActive(true) ejecutado.");
        }

        if (objetoNotaPuerta != null)
        {
            Renderer rend = objetoNotaPuerta.GetComponent<Renderer>();
            if (rend != null) rend.enabled = false;
            
            Collider col = objetoNotaPuerta.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        StartCoroutine(HabilitarCierre());

        if (timerCierreCoroutine != null) StopCoroutine(timerCierreCoroutine);
        timerCierreCoroutine = StartCoroutine(EsperarYCerrar());
    }

    private IEnumerator HabilitarCierre()
    {
        yield return null; 
        puedeCerrar = true;
    }

    private IEnumerator EsperarYCerrar()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("Temporizador de 3 segundos finalizado. Cerrando nota.");
        CerrarNota();
    }

    private void CerrarNota()
    {
        if (!estaAbierta) return;

        if (timerCierreCoroutine != null)
        {
            StopCoroutine(timerCierreCoroutine);
            timerCierreCoroutine = null;
        }

        estaAbierta = false;
        puedeCerrar = false;

        if (sonidoPapel != null) sonidoPapel.Play();

        if (canvasNotaUI != null)
        {
            canvasNotaUI.SetActive(false);
            Debug.Log("-> canvasNotaUI.SetActive(false) ejecutado.");
        }
    }

    void ReaccionLucas()
    {
        if (Act2ManagerDemo.Instance != null)
        {
            Act2ManagerDemo.Instance.NotaLeida();
        }
    }

    public bool CanInteract()
    {
        return !estaAbierta;
    }

    public string GetDescription()
    {
        if (estaAbierta) return "Leyendo nota...";
        return "Presiona [E] para leer la nota";
    }
}