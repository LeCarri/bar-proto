using UnityEngine;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Copia de Act2DebugHelper.cs adaptada al Act2ManagerReestructura.
/// (El original sigue intacto para la demo.)
///
/// Muestra la fase actual en pantalla y permite saltar a cualquier fase con el teclado.
/// Solo funciona en el Editor y en Development Builds: en la build final no hace nada.
///
/// SETUP: agregar a cualquier GameObject de la escena (por ejemplo al mismo del Act2ManagerReestructura).
///
/// TECLAS:
///   F1  — Mostrar / ocultar el panel
///   1 Tareas · 2 Servicio · 3 Cliente corrupto · 4 Pasillo · 5 Puerta del sótano
///   6 Regreso al baño · 7 Vigilante · 8 Combate · 9 Cierre
///   + / -  — Subir / bajar paranoia 20
/// </summary>
public class Act2DebugHelperReestructura : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD

    [Tooltip("Arranca con el panel visible")]
    public bool mostrarAlInicio = true;

    private bool hudVisible;
    private GUIStyle estilo;

    void Start()
    {
        hudVisible = mostrarAlInicio;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) hudVisible = !hudVisible;
        if (!hudVisible) return;

        Act2ManagerReestructura m = Act2ManagerReestructura.Instance;
        if (m == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) m.DebugSaltarA(FaseDebugReestructura.Tareas);
        if (Input.GetKeyDown(KeyCode.Alpha2)) m.DebugSaltarA(FaseDebugReestructura.Servicio);
        if (Input.GetKeyDown(KeyCode.Alpha3)) m.DebugSaltarA(FaseDebugReestructura.ClienteCorrupto);
        if (Input.GetKeyDown(KeyCode.Alpha4)) m.DebugSaltarA(FaseDebugReestructura.Pasillo);
        if (Input.GetKeyDown(KeyCode.Alpha5)) m.DebugSaltarA(FaseDebugReestructura.PuertaSotano);
        if (Input.GetKeyDown(KeyCode.Alpha6)) m.DebugSaltarA(FaseDebugReestructura.RegresoBano);
        if (Input.GetKeyDown(KeyCode.Alpha7)) m.DebugSaltarA(FaseDebugReestructura.Vigilante);
        if (Input.GetKeyDown(KeyCode.Alpha8)) m.DebugSaltarA(FaseDebugReestructura.Combate);
        if (Input.GetKeyDown(KeyCode.Alpha9)) m.DebugSaltarA(FaseDebugReestructura.Cierre);

        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Equals))
            m.SumarParanoia(20f);
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            m.SumarParanoia(-20f);
    }

    void OnGUI()
    {
        if (!hudVisible) return;

        if (estilo == null)
        {
            estilo = new GUIStyle { fontSize = 14, fontStyle = FontStyle.Bold, richText = true };
            estilo.normal.textColor = Color.white;
        }

        Act2ManagerReestructura m = Act2ManagerReestructura.Instance;
        float paranoia = ParanoiaSystem.Instance != null ? ParanoiaSystem.Instance.paranoiaActual : -1f;

        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.DrawTexture(new Rect(8, 8, 240, 250), Texture2D.whiteTexture);
        GUI.color = Color.white;

        string texto =
            "<b><color=#FF6B6B>NOCHE 2 (REESTRUCTURA)</color></b> [F1]\n" +
            (m == null ? "<color=#FF6B6B>¡No hay Act2ManagerReestructura!</color>\n"
                       : $"Fase: <color=#FFD93D>{m.estadoActual}</color>\n") +
            (paranoia >= 0f ? $"Paranoia: {paranoia:F0}/100\n" : "") +
            $"Llave: {(m != null && m.TieneLlave() ? "<color=#6BCB77>SÍ</color>" : "<color=#FF6B6B>NO</color>")}\n" +
            "──────────────────\n" +
            "<color=#AAAAAA>1 Tareas  2 Servicio\n3 Corrupto  4 Pasillo\n5 Pta.Sótano  6 Baño\n7 Vigilante  8 Combate\n9 Cierre\n" +
            "[+/-] Paranoia ±20</color>";

        GUI.Label(new Rect(14, 12, 230, 250), texto, estilo);
    }

#endif
}
