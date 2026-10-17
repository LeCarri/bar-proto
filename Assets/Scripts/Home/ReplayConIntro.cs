
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ReplayConIntro : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string escenaHome = "Home";

    [Header("Solo asignar en Home")]
    [SerializeField] private Button startBtnHome;

    private static bool iniciarIntroAutomaticamente = false;

    // Llamado desde el botón Replay de los créditos.
    public void JugarDeNuevo()
    {
        iniciarIntroAutomaticamente = true;

        Time.timeScale = 1f;

        SceneManager.LoadScene(escenaHome);
    }

    private IEnumerator Start()
    {
        // Solo ejecutar si venimos desde Replay.
        if (!iniciarIntroAutomaticamente)
            yield break;

        if (SceneManager.GetActiveScene().name != escenaHome)
            yield break;

        iniciarIntroAutomaticamente = false;

        // Esperar a que Home termine de inicializarse.
        yield return null;

        if (startBtnHome != null)
        {
            // Ejecutar exactamente el mismo OnClick
            // que utiliza el botón Start original.
            startBtnHome.onClick.Invoke();
        }
        else
        {
            Debug.LogWarning(
                "[ReplayConIntro] Falta asignar StartBtn de Home."
            );
        }
    }
}
