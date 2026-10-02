using UnityEngine;
using System.Collections; // Necesario para IEnumerator

public class TriggerRegreso : MonoBehaviour
{
    private bool activado = false;

    [Header("Animación de la mujer")]
    // Nombre de la animación tal cual está en el Animator Controller
    public string nombreAnimacion = "mixamo_com";

    [Header("Objetos a desactivar durante este acto")]
    [Tooltip("Arrastrar acá el cartel o GameObject del servicio de bebidas para que no confunda al jugador.")]
    [SerializeField] private GameObject cartelServicioBebidas;

    void OnTriggerEnter(Collider other)
    {
        // 1. Verificamos que sea el Player y que el trigger no se haya activado antes
        if (other.CompareTag("Player") && !activado)
        {
            activado = true;

            // Desactivamos el cartel/pedido de servicio de bebidas para evitar confusión
            DesactivarCartelServicioBebidas();

            //StartCoroutine(AparicionMujer());
        }
    }

    private void DesactivarCartelServicioBebidas()
    {
        if (cartelServicioBebidas != null)
        {
            cartelServicioBebidas.SetActive(false);
            Debug.Log("[TriggerRegreso] Cartel de servicio de bebidas desactivado para evitar confusión con la botella especial.");
        }
        else
        {
            Debug.LogWarning("[TriggerRegreso] No se asignó el cartelServicioBebidas en el Inspector.");
        }
    }

    
}