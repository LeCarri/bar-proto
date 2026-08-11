using UnityEngine;

public class ManchaSangre : MonoBehaviour
{
    [Header("Configuración")]
    public float tiempoLimpieza = 3f;

    [Header("Tipo de mancha")]
    public bool esManchaPared = true;

    private float tiempoActual = 0f;
    private bool limpiando = false;
    private bool limpiada = false;

    private Renderer rend;
    private Material material;

    private Color colorInicial;

    void Start()
    {
        rend = GetComponent<Renderer>();

        if (rend != null)
        {
            material = rend.material;
            colorInicial = material.color;
        }
    }

    void Update()
    {
        if (limpiada)
            return;

        if (limpiando)
        {
            tiempoActual += Time.deltaTime;

            float progreso = tiempoActual / tiempoLimpieza;

            progreso = Mathf.Clamp01(progreso);

            if (material != null)
            {
                Color nuevoColor = colorInicial;
                nuevoColor.a = Mathf.Lerp(1f, 0f, progreso);

                material.color = nuevoColor;
            }

            if (tiempoActual >= tiempoLimpieza)
            {
                TerminarLimpieza();
            }
        }
    }

    public void EmpezarLimpieza()
    {
        if (limpiada)
            return;

        if (Act3Manager.Instance == null)
            return;

        if (!Act3Manager.Instance.tieneElementosLimpieza)
        {
            Act3Manager.Instance.MostrarDialogo(
                "Necesito buscar los elementos de limpieza primero."
            );

            return;
        }

        limpiando = true;
    }

    public void DetenerLimpieza()
    {
        limpiando = false;
    }

    void TerminarLimpieza()
    {
        limpiando = false;
        limpiada = true;

        if (Act3Manager.Instance != null)
        {
            if (esManchaPared)
            {
                Act3Manager.Instance.MostrarDialogo(
                    "Mancha de pared limpiada."
                );

                Act3Manager.Instance.ManchaParedLimpiada();
            }
            else
            {
                Act3Manager.Instance.ManchaPisoLimpiada();
            }
        }

        gameObject.SetActive(false);
    }
}
