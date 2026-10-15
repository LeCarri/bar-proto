
using UnityEngine;

public class IndicadorMenu : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RectTransform indicador;
    [SerializeField] private RectTransform botonInicial;

    [Header("Movimiento")]
    [SerializeField] private float velocidad = 12f;
    [SerializeField] private bool volverAlSalir = true;

    private RectTransform objetivoActual;

    private void Awake()
    {
        if (indicador == null)
            indicador = GetComponent<RectTransform>();

        objetivoActual = botonInicial;
    }

    private void Start()
    {
        if (indicador != null && botonInicial != null)
        {
            Vector3 posicion = indicador.position;
            posicion.y = ObtenerCentroY(botonInicial);
            indicador.position = posicion;
        }
    }

    private void Update()
    {
        if (indicador == null || objetivoActual == null)
            return;

        float destinoY = ObtenerCentroY(objetivoActual);

        Vector3 posicion = indicador.position;

        float suavizado = 1f - Mathf.Exp(
            -velocidad * Time.unscaledDeltaTime
        );

        posicion.y = Mathf.Lerp(
            posicion.y,
            destinoY,
            suavizado
        );

        indicador.position = posicion;
    }

    private float ObtenerCentroY(RectTransform boton)
    {
        return boton.TransformPoint(boton.rect.center).y;
    }

    public void MoverA(RectTransform boton)
    {
        if (boton != null)
            objetivoActual = boton;
    }

    public void SalirDe(RectTransform boton)
    {
        if (volverAlSalir && objetivoActual == boton)
            objetivoActual = botonInicial;
    }
}
