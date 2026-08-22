using UnityEngine;
using UnityEngine.UI;

public class MinijuegoPedido : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelMinijuego;
    public RectTransform vaso;
    public RectTransform zonaVerde;

    [Header("Movimiento")]
    public float velocidadVaso = 0.00005f;

    [Header("Configuración")]
    public int aciertosNecesarios = 3;

    [Header("Resultado")]
    public TMPro.TextMeshProUGUI textoAciertos;
    public TMPro.TextMeshProUGUI textoResultado;

    private bool minijuegoActivo = false;

    private int aciertos = 0;

    private float direccion = 1f;
    private float limiteIzquierdo = -350f;
    private float limiteDerecho = 350f;

    private PedidoPickup pedidoActual;

    void Start()
    {
        if (panelMinijuego != null)
        {
            panelMinijuego.SetActive(false);
        }
    }


    void Update()
    {
        if (!minijuegoActivo)
            return;

        Debug.Log("MINIJUEGO ACTIVO");

        MoverVaso();

        if (Input.GetKeyDown(KeyCode.E))
        {
            ComprobarAcierto();
        }
    }


    public void IniciarMinijuego(PedidoPickup pedido)
    {
        if (minijuegoActivo)
            return;

        pedidoActual = pedido;

        minijuegoActivo = true;

        aciertos = 0;

        if (panelMinijuego != null)
        {
            panelMinijuego.SetActive(true);
        }

        if (textoResultado != null)
        {
            textoResultado.text = "";
        }

        ActualizarTextoAciertos();

        PosicionarVasoInicial();
    }

    void MoverVaso()
    {
        if (vaso == null)
            return;

        Vector2 posicion = vaso.anchoredPosition;

        posicion.x += direccion * velocidadVaso * Time.deltaTime;

        // Límite derecho
        if (posicion.x >= limiteDerecho)
        {
            posicion.x = limiteDerecho;
            direccion = -1f;
        }

        // Límite izquierdo
        if (posicion.x <= limiteIzquierdo)
        {
            posicion.x = limiteIzquierdo;
            direccion = 1f;
        }

        vaso.anchoredPosition = posicion;
    }

    void ComprobarAcierto()
    {
        if (vaso == null || zonaVerde == null)
            return;

        float posicionVaso = vaso.anchoredPosition.x;

        float centroVerde = zonaVerde.anchoredPosition.x;

        float anchoVerde = zonaVerde.rect.width;

        float limiteVerdeIzquierdo =
            centroVerde - (anchoVerde / 2f);

        float limiteVerdeDerecho =
            centroVerde + (anchoVerde / 2f);


        bool estaEnZonaVerde =
            posicionVaso >= limiteVerdeIzquierdo &&
            posicionVaso <= limiteVerdeDerecho;


        if (estaEnZonaVerde)
        {
            Acierto();
        }
        else
        {
            Fallo();
        }
    }


    void Acierto()
    {
        aciertos++;

        ActualizarTextoAciertos();

        if (textoResultado != null)
        {
            textoResultado.text = "¡Bien!";
        }

        if (aciertos >= aciertosNecesarios)
        {
            CompletarMinijuego();
        }
        else
        {
            MoverZonaVerde();
            NuevaRonda();
        }
    }


    void Fallo()
    {
        if (textoResultado != null)
        {
            textoResultado.text = "¡Fallaste!";
        }

        MoverZonaVerde();
        NuevaRonda();
    }


    void NuevaRonda()
    {
        PosicionarVasoInicial();
    }


    void CompletarMinijuego()
    {
        minijuegoActivo = false;

        if (textoResultado != null)
        {
            textoResultado.text = "¡Pedido conseguido!";
        }

        Debug.Log("Minijuego completado. Pedido conseguido.");

        Invoke(nameof(CerrarMinijuego), 1f);
    }


    void CerrarMinijuego()
    {
        if (panelMinijuego != null)
        {
            panelMinijuego.SetActive(false);
        }

        PedidoCompletado();
    }


    void PedidoCompletado()
    {
        Debug.Log("MINIJUEGO: Pedido completado correctamente.");

        if (pedidoActual != null)
        {
            pedidoActual.PedidoCompletado();
        }

        pedidoActual = null;
    }


    void CalcularLimites()
    {
        if (vaso == null)
            return;

        RectTransform padre =
            vaso.parent as RectTransform;

        if (padre == null)
            return;

        float anchoPadre = padre.rect.width;

        float mitadVaso = vaso.rect.width / 2f;

        limiteIzquierdo =
            (-anchoPadre / 2f) + mitadVaso;

        limiteDerecho =
            (anchoPadre / 2f) - mitadVaso;
    }


    void PosicionarVasoInicial()
    {
        if (vaso == null)
            return;

        Vector2 posicion = vaso.anchoredPosition;

        posicion.x = limiteIzquierdo;

        vaso.anchoredPosition = posicion;

        direccion = 1f;
    }

    void MoverZonaVerde()
    {
        if (zonaVerde == null)
            return;

        float anchoBarra = 600f;

        float mitadZona = zonaVerde.rect.width / 2f;

        float limiteIzquierdoZona =
            -anchoBarra / 2f + mitadZona;

        float limiteDerechoZona =
            anchoBarra / 2f - mitadZona;

        float nuevaPosicionX = Random.Range(
            limiteIzquierdoZona,
            limiteDerechoZona
        );

        Vector2 posicion = zonaVerde.anchoredPosition;

        posicion.x = nuevaPosicionX;

        zonaVerde.anchoredPosition = posicion;
    }




    void ActualizarTextoAciertos()
    {
        if (textoAciertos != null)
        {
            textoAciertos.text =
                "Aciertos: " +
                aciertos +
                " / " +
                aciertosNecesarios;
        }
    }
}
