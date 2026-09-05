using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject panelDialogo;
    [SerializeField] private TextMeshProUGUI textoDialogo;

    private bool estaAbierto = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CerrarDialogo();
    }

    private void Update()
    {
        // Si el diálogo está abierto y apretamos E, Espacio o Click, lo cerramos
        if (estaAbierto && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            CerrarDialogo();
        }
    }

    public void MostrarTexto(string mensaje)
    {
        textoDialogo.text = mensaje;
        panelDialogo.SetActive(true);
        estaAbierto = true;

        // Opcional: Pausar el tiempo o desbloquear cursor si lo necesitás
        Time.timeScale = 0f;
    }

    public void CerrarDialogo()
    {
        panelDialogo.SetActive(false);
        estaAbierto = false;

        // Reanudamos el juego
        Time.timeScale = 1f;
    }
}
