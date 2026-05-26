using UnityEngine;
using TMPro;

/// <summary>
/// Herramienta de debug para el Acto 2. Muestra el estado actual en pantalla
/// y permite saltar a cualquier fase con teclas de acceso rápido.
///
/// SETUP: Agregar a cualquier GameObject en la escena Night_2.
/// Activar/desactivar el HUD en runtime con la tecla F1.
/// Solo funciona en el Editor y en Development Builds (desaparece en la build final).
///
/// TECLAS (solo con el HUD activo):
///   F1  — Toggle del HUD
///   1   — Saltar a Inicio
///   2   — Saltar a Servicio
///   3   — Saltar a Pasillo
///   4   — Saltar a Sótano
///   5   — Saltar a Baño
///   6   — Saltar a Psicosis
///   7   — Saltar a Cierre
///   +   — Subir paranoia 20
///   -   — Bajar paranoia 20
/// </summary>
public class Act2DebugHelper : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD

    [Header("Configuración")]
    [Tooltip("Arranca con el HUD visible")]
    public bool mostrarAlInicio = true;

    private bool hudVisible;
    private GUIStyle estilo;
    private GUIStyle estiloFondo;

    void Start()
    {
        hudVisible = mostrarAlInicio;
        ConfigurarEstilos();
    }

    void ConfigurarEstilos()
    {
        estilo = new GUIStyle
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            richText = true
        };
        estilo.normal.textColor = Color.white;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            hudVisible = !hudVisible;

        if (!hudVisible) return;

        Act2Manager m = Act2Manager.Instance;
        if (m == null) return;

        // Saltar estados
        if (Input.GetKeyDown(KeyCode.Alpha1)) SaltarA(Act2Manager.Act2State.Inicio);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SaltarA(Act2Manager.Act2State.Servicio);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SaltarA(Act2Manager.Act2State.Pasillo);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SaltarA(Act2Manager.Act2State.Sotano);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SaltarA(Act2Manager.Act2State.Bano);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SaltarA(Act2Manager.Act2State.Psicosis);
        if (Input.GetKeyDown(KeyCode.Alpha7)) SaltarA(Act2Manager.Act2State.Cierre);

        // Paranoia
        if (Input.GetKeyDown(KeyCode.Plus)  || Input.GetKeyDown(KeyCode.KeypadPlus))
            ParanoiaSystem.Instance?.AddParanoia(20f);
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            ParanoiaSystem.Instance?.AddParanoia(-20f);
    }

    void SaltarA(Act2Manager.Act2State estado)
    {
        Act2Manager m = Act2Manager.Instance;
        if (m == null) return;

        m.StopAllCoroutines();

        switch (estado)
        {
            case Act2Manager.Act2State.Inicio:
                m.estadoActual = Act2Manager.Act2State.Inicio;
                m.StartCoroutine(m.GetType()
                    .GetMethod("SecuenciaInicio", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(m, null) as System.Collections.IEnumerator);
                break;
            case Act2Manager.Act2State.Servicio:
                m.estadoActual = Act2Manager.Act2State.Inicio; // para que SecuenciaServicio no falle
                m.StartCoroutine(m.GetType()
                    .GetMethod("SecuenciaServicio", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(m, null) as System.Collections.IEnumerator);
                break;
            case Act2Manager.Act2State.Pasillo:
                m.IrACocina();
                break;
            case Act2Manager.Act2State.Sotano:
                m.estadoActual = Act2Manager.Act2State.Pasillo;
                m.ZapatosEncontrados();
                break;
            case Act2Manager.Act2State.Bano:
                m.NotaLeida();
                break;
            case Act2Manager.Act2State.Psicosis:
                m.estadoActual = Act2Manager.Act2State.Bano;
                m.llaveTenida = true;
                m.StartCoroutine(m.GetType()
                    .GetMethod("SecuenciaPsicosis", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(m, null) as System.Collections.IEnumerator);
                break;
            case Act2Manager.Act2State.Cierre:
                m.llaveTenida = true;
                m.UsarLlave();
                break;
        }
        Debug.Log($"[Debug] Saltando a estado: {estado}");
    }

    void OnGUI()
    {
        if (!hudVisible) return;

        Act2Manager m = Act2Manager.Instance;
        float paranoia = ParanoiaSystem.Instance?.GetParanoia() ?? -1f;

        // Fondo semitransparente
        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.DrawTexture(new Rect(8, 8, 230, 230), Texture2D.whiteTexture);
        GUI.color = Color.white;

        string texto =
            "<b><color=#FF6B6B>ACT2 DEBUG</color></b> [F1 ocultar]\n" +
            $"Estado: <color=#FFD93D>{m?.estadoActual}</color>\n" +
            $"Paranoia: <color=#FF6B6B>{paranoia:F0}/100</color>\n" +
            $"Llave: {(m != null && m.TieneLlave() ? "<color=#6BCB77>SÍ</color>" : "<color=#FF6B6B>NO</color>")}\n" +
            "──────────────────\n" +
            "<color=#AAAAAA>[1-7] Saltar estado\n" +
            "[+/-] Paranoia ±20</color>";

        if (estilo == null) ConfigurarEstilos();
        GUI.Label(new Rect(14, 12, 220, 230), texto, estilo);
    }

#endif
}
