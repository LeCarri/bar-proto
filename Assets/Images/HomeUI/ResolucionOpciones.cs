using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Configura un TMP_Dropdown con las resoluciones disponibles para el juego.
/// Colocar en un objeto del panel Opciones y asignar el Dropdown en Inspector.
/// </summary>
public class ResolucionOpciones : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown desplegableResolucion;
    [Tooltip("Opcional: texto original Res Text, para conservar el diseño del menú.")]
    [SerializeField] private TMP_Text textoResolucionVisual;

    private const string AnchoPrefs = "ResolucionAncho";
    private const string AltoPrefs = "ResolucionAlto";
    private readonly List<Vector2Int> resoluciones = new List<Vector2Int>();

    private void Start()
    {
        if (desplegableResolucion == null)
        {
            Debug.LogError("ResolucionOpciones: Asigná el TMP_Dropdown del panel.");
            return;
        }

        // Screen.resolutions puede devolver la misma medida con varias tasas de refresco.
        foreach (Resolution r in Screen.resolutions)
        {
            Vector2Int medida = new Vector2Int(r.width, r.height);
            if (r.width > 0 && r.height > 0 && !resoluciones.Contains(medida))
                resoluciones.Add(medida);
        }

        // Por si la plataforma o el Editor no informa resoluciones.
        Vector2Int actual = new Vector2Int(Screen.width, Screen.height);
        if (resoluciones.Count == 0)
            resoluciones.Add(actual);

        resoluciones.Sort((a, b) =>
        {
            int comparacion = ((long)b.x * b.y).CompareTo((long)a.x * a.y);
            return comparacion != 0 ? comparacion : b.x.CompareTo(a.x);
        });

        List<string> opciones = new List<string>();
        foreach (Vector2Int medida in resoluciones)
            opciones.Add($"{medida.x} x {medida.y}");

        desplegableResolucion.ClearOptions();
        desplegableResolucion.AddOptions(opciones);

        Vector2Int guardada = new Vector2Int(
            PlayerPrefs.GetInt(AnchoPrefs, actual.x),
            PlayerPrefs.GetInt(AltoPrefs, actual.y));

        int indice = resoluciones.IndexOf(guardada);
        if (indice < 0)
            indice = resoluciones.IndexOf(actual);
        if (indice < 0)
            indice = 0;

        desplegableResolucion.SetValueWithoutNotify(indice);
        desplegableResolucion.RefreshShownValue();
        ActualizarTextoVisual(indice);
        desplegableResolucion.onValueChanged.AddListener(CambiarResolucion);

        // Recupera la resolución elegida en la ejecución anterior.
        if (resoluciones.Contains(guardada) && guardada != actual)
            AplicarResolucion(guardada);
    }

    private void OnDestroy()
    {
        if (desplegableResolucion != null)
            desplegableResolucion.onValueChanged.RemoveListener(CambiarResolucion);
    }

    private void CambiarResolucion(int indice)
    {
        if (indice < 0 || indice >= resoluciones.Count)
            return;

        Vector2Int medida = resoluciones[indice];
        PlayerPrefs.SetInt(AnchoPrefs, medida.x);
        PlayerPrefs.SetInt(AltoPrefs, medida.y);
        PlayerPrefs.Save();

        ActualizarTextoVisual(indice);
        AplicarResolucion(medida);
    }

    private void ActualizarTextoVisual(int indice)
    {
        if (textoResolucionVisual != null && indice >= 0 && indice < resoluciones.Count)
        {
            Vector2Int medida = resoluciones[indice];
            textoResolucionVisual.text = $"{medida.x} x {medida.y}";
        }
    }

    private void AplicarResolucion(Vector2Int medida)
    {
#if UNITY_EDITOR
        // El tamaño real se aplica al ejecutar una build, no en la pestaña Game.
        Debug.Log($"Resolución seleccionada: {medida.x} x {medida.y}. Probá el cambio en una build.");
#else
        Screen.SetResolution(medida.x, medida.y, Screen.fullScreenMode);
#endif
    }
}
