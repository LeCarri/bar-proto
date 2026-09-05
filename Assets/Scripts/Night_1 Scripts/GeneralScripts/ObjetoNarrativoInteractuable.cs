using UnityEngine;
using System.Collections;

public class ObjetoNarrativoInteractuable : MonoBehaviour, IInteractable
{
    [Header("UI & Inspección")]
    [SerializeField] private string textoAccion = "Inspeccionar dibujo";
    [SerializeField] private GameObject imagenDibujoEnPantalla; // La Image del Canvas que muestra el dibujo centrado
    [SerializeField] private float tiempoEnPantalla = 3.5f;

    [Header("Lore / Narrativa")]
    [TextArea(3, 5)]
    [SerializeField] private string descripcionLore = "Un dibujo infantil... me lo guardo en el bolsillo.";

    private bool yaFueTomado = false;

    public bool CanInteract()
    {
        return !yaFueTomado;
    }

    public string GetDescription()
    {
        return textoAccion;
    }

   public void Interact()
    {
        if (yaFueTomado) return;
        yaFueTomado = true;

        // 1. Apagamos el Collider para no reinteractuar
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 2. Apagamos solo la parte visual 3D (Renderer), NO el GameObject
        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.enabled = false;

        // Desactivamos hijos 3D si los tiene
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        // 3. Lanzamos el diálogo
        if (Act1Manager.Instance != null)
        {
            Act1Manager.Instance.MostrarDialogo(descripcionLore);
        }

        // 4. Arrancamos la muestra en UI
        StartCoroutine(SecuenciaSostenerDibujo());
    }

    private IEnumerator SecuenciaSostenerDibujo()
    {
        if (imagenDibujoEnPantalla == null)
        {
            Debug.LogError($"[ObjetoNarrativo] Falta asignar 'imagenDibujoEnPantalla' en {gameObject.name}");
            yield break;
        }

        // Encender la UI del dibujo
        imagenDibujoEnPantalla.SetActive(true);

        yield return new WaitForSeconds(tiempoEnPantalla);

        // Ocultar la UI del dibujo
        imagenDibujoEnPantalla.SetActive(false);

        // RECIÉN ACÁ apagamos el GameObject completo
        gameObject.SetActive(false);
    }
}