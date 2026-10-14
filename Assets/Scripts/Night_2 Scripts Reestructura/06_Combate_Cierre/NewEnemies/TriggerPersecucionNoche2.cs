using UnityEngine;

/// <summary>
/// Trigger independiente del evento de Pilar: arranca la persecucion tras 1 segundo
/// (el retraso se configura en EnemigoNoche2AI).
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class TriggerPersecucionNoche2 : MonoBehaviour
{
    [SerializeField] private EnemigoNoche2AI enemigo;
    private bool usado;

    private void OnTriggerEnter(Collider otro)
    {
        if (usado || enemigo == null) return;

        PlayerController jugador = otro.GetComponentInParent<PlayerController>();
        if (jugador == null) return;

        
        if (Act2ManagerDemo.Instance == null ||
            !Act2ManagerDemo.Instance.TieneLlave() ||
            Act2ManagerDemo.Instance.estadoActual != Act2ManagerDemo.Act2State.Psicosis)
        {
            return;
        }


        usado = true;
        enemigo.ActivarPersecucion(jugador.transform);
        Debug.Log("[EnemigoNoche2] Trigger cruzado: persecucion solicitada.", this);
    }

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }
}
