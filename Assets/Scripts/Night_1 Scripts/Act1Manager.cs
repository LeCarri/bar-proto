using UnityEngine;
using TMPro; // Asegurate de tener TextMeshPro en el proyecto

public class Act1Manager : MonoBehaviour
{
    public enum ActoState { Limpieza, Servicio, ElQuiebre }
    public ActoState estadoActual = ActoState.Limpieza;

    [Header("UI")]
    public TextMeshProUGUI textoSubtitulos;

    [Header("Objetos de Escena")]
    public GameObject grupoClientes;
    public GameObject mujerMisteriosa;

    private int sillasAcomodadas = 0;
    public int totalSillas = 3;

    void Start()
    {
        // Al empezar, Lucas dice su primera frase
        MostrarDialogo("Lucas: Hay que mantener el lugar limpio para cuando ellas vuelvan...");
        grupoClientes.SetActive(false);
        mujerMisteriosa.SetActive(false);
    }

    public void SillaCompletada()
    {
        sillasAcomodadas++;
        if (sillasAcomodadas >= totalSillas && estadoActual == ActoState.Limpieza)
        {
            IniciarServicio();
        }
    }

    void IniciarServicio()
    {
        estadoActual = ActoState.Servicio;
        grupoClientes.SetActive(true);
        MostrarDialogo("Lucas: Ya entran los primeros... a trabajar.");
    }

    public void MostrarDialogo(string texto)
    {
        textoSubtitulos.text = texto;
        // Podrías agregar un Invoke para borrar el texto después de unos segundos
        CancelInvoke("BorrarTexto");
        Invoke("BorrarTexto", 4f);
    }

    void BorrarTexto() => textoSubtitulos.text = "";
}