using System.Collections;
using UnityEngine;

public class TestBebidasSetup : MonoBehaviour
{
    private IEnumerator Start()
    {
        // Esperamos un frame para que Act1Manager termine su Start()
        yield return null;

        if (Act1Manager.Instance == null)
        {
            Debug.LogError("TEST BEBIDAS: No hay Act1Manager en la escena.");
            yield break;
        }

        // Saltamos directamente al servicio
        Act1Manager.Instance.estadoActual = Act1Manager.ActoState.Servicio;

        // Permitimos probar directamente la cerveza
        Act1Manager.Instance.carlosPidioCerveza = true;

        Debug.Log("TEST BEBIDAS: Servicio habilitado.");
    }
}