using UnityEngine;

public class LlegadaSalonAct3 : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject jugador;

    [Header("Siluetas")]
    public GameObject clientesActo3;

    private bool activado = false;


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(
            "[SALON] ENTRÓ AL TRIGGER: " +
            other.gameObject.name
        );

        // Evita que se active más de una vez
        if (activado)
            return;


        // Verifica que haya entrado Lucas
        if (jugador != null && other.gameObject != jugador)
            return;


        // Verifica que exista el Act3Manager
        if (Act3Manager.Instance == null)
            return;


        if (!Act3Manager.Instance.elementosGuardados)
            return;


        activado = true;


        Debug.Log(
            "[Act3] Lucas llegó al salón después de guardar los elementos."
        );


        // Activa las siluetas
        if (clientesActo3 != null)
        {
            clientesActo3.SetActive(true);
        }


        // Continúa la secuencia de la noche
        Act3Manager.Instance.IniciarSecuenciaSalon();
    }
}