using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;

public class Act3Manager : MonoBehaviour
{
    public static Act3Manager Instance;


    // ESCENA


    [Header("Escena")]
    public GameObject clientesActo3;
    public GameObject enemigos;
    public GameObject vigilante;



    // INTERACCIÓN


    [Header("Interacción")]
    public GameObject panelInteraccion;
    public TextMeshProUGUI textoInteraccion;

    [Header("Textos de Interacción")]
    public string textoCliente = "Interactuar";
    public string textoPedido = "Recoger";
    public string textoPuerta = "Abrir";



    // UI Y DIÁLOGOS


    [Header("UI y Diálogos")]
    public TextMeshProUGUI textoSubtitulos;

    private Coroutine dialogoActual;



    // EFECTOS


    [Header("Efectos")]
    public EffectoParpadeo effectoParpadeo;



    // ILUMINACIÓN


    [Header("Sistemas de Iluminación")]
    public GameObject lucesNormales;
    public GameObject lucesServicio;
    public GameObject lucesCombate;



    // TEXTO OBJETIVOS


    [Header("Objetivos")]
    public TextMeshProUGUI textoObjetivo;



    // PROGRESO


    [Header("Progreso")]
    public bool enSotano = false;
    public int clientesAtendidos = 0;



    // LIMPIEZA INICIAL


    [Header("Limpieza Inicial")]
    public GameObject ElementosLimpieza;

    public bool tieneElementosLimpieza = false;

    public int manchasParedLimpiadas = 0;
    public int manchasPisoLimpiadas = 0;

    public int totalManchasPared = 5;
    public int totalManchasPiso = 5;

    private bool limpiezaTerminada = false;



    // PEDIDOS


    [Header("Pedidos")]
    public string pedidoActual = "";
    public bool tienePedido = false;
    public bool tienePedidoBuscado = false;



    // OBJETO ESPECIAL


    [Header("Objeto especial")]
    public GameObject objetoEspecial;
    public string pedidoQueLoActiva = "Llave";



    // AWAKE


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }



    // START


    void Start()
    {
        if (ParanoiaSystem.Instance != null)
        {
            ParanoiaSystem.Instance.AddParanoia(50f);
        }

        if (objetoEspecial != null)
        {
            objetoEspecial.SetActive(false);
        }

        if (clientesActo3 != null)
        {
            clientesActo3.SetActive(false);
        }

        tieneElementosLimpieza = false;

        manchasParedLimpiadas = 0;
        manchasPisoLimpiadas = 0;

        limpiezaTerminada = false;

        ActualizarObjetivo("Busca los elementos de limpieza");

        StartCoroutine(MantenerParanoiaMinima());
    }



    // UPDATE / INTERACCIONES


    void Update()
    {
        if (Camera.main == null)
            return;

        Ray ray = new Ray(
            Camera.main.transform.position,
            Camera.main.transform.forward
        );

        RaycastHit hit;

        bool mirandoAlgo = false;


        // MOSTRAR TEXTO DE INTERACCIÓN


        if (Physics.Raycast(
            ray,
            out hit,
            10f,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide
        ))
        {
            // CLIENTE
            SimpleInteract cliente =
                hit.collider.GetComponentInParent<SimpleInteract>();

            if (cliente != null)
            {
                mirandoAlgo = true;

                if (panelInteraccion != null)
                    panelInteraccion.SetActive(true);

                if (textoInteraccion != null)
                    textoInteraccion.text = textoCliente;
            }

            // MANCHA DE SANGRE
            ManchaSangre mancha =
                hit.collider.GetComponentInParent<ManchaSangre>();

            if (mancha != null)
            {
                mirandoAlgo = true;

                if (panelInteraccion != null)
                    panelInteraccion.SetActive(true);

                if (textoInteraccion != null)
                    textoInteraccion.text = "Mantener E para limpiar";
            }


            // PEDIDO
            PedidoPickup pedido =
                hit.collider.GetComponentInParent<PedidoPickup>();

            if (pedido != null)
            {
                mirandoAlgo = true;

                if (panelInteraccion != null)
                    panelInteraccion.SetActive(true);

                if (textoInteraccion != null)
                    textoInteraccion.text = textoPedido;
            }


            // PUERTA
            PuertaSotano puerta =
                hit.collider.GetComponentInParent<PuertaSotano>();

            if (puerta != null)
            {
                mirandoAlgo = true;

                if (panelInteraccion != null)
                    panelInteraccion.SetActive(true);

                if (textoInteraccion != null)
                    textoInteraccion.text = textoPuerta;
            }


            // ELEMENTOS DE LIMPIEZA
            ElementosLimpieza limpieza =
                hit.collider.GetComponentInParent<ElementosLimpieza>();

            if (limpieza != null)
            {
                mirandoAlgo = true;

                if (panelInteraccion != null)
                    panelInteraccion.SetActive(true);

                if (textoInteraccion != null)
                    textoInteraccion.text = "Recoger";
            }


            // OBJETO ESPECIAL
            ObjetosEspeciales objeto =
                hit.collider.GetComponentInParent<ObjetosEspeciales>();

            if (objeto != null)
            {
                mirandoAlgo = true;

                if (panelInteraccion != null)
                    panelInteraccion.SetActive(true);

                if (textoInteraccion != null)
                    textoInteraccion.text = "Investigar";
            }
        }


        // OCULTAR TEXTO


        if (!mirandoAlgo)
        {
            if (panelInteraccion != null)
            {
                panelInteraccion.SetActive(false);
            }
        }



        // INTERACTUAR CON E


        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Physics.Raycast(
                ray,
                out hit,
                10f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide
            ))
            {

                // ELEMENTOS DE LIMPIEZA
                ElementosLimpieza limpieza =
                    hit.collider.GetComponentInParent<ElementosLimpieza>();

                if (limpieza != null)
                {
                    limpieza.Interact();
                    return;
                }


                // PUERTA

                PuertaSotano puerta =
                    hit.collider.GetComponentInParent<PuertaSotano>();

                if (puerta != null)
                {
                    puerta.Interact();
                    return;
                }


                // MANCHA DE SANGRE
                ManchaSangre mancha =
                    hit.collider.GetComponentInParent<ManchaSangre>();

                if (mancha != null)
                {
                    if (Input.GetKey(KeyCode.E))
                    {
                        mancha.EmpezarLimpieza();
                    }
                    else
                    {
                        mancha.DetenerLimpieza();
                    }

                    return;
                }


                // OBJETO ESPECIAL

                ObjetosEspeciales objeto =
                    hit.collider.GetComponentInParent<ObjetosEspeciales>();

                if (objeto != null)
                {
                    objeto.Interactuar();
                    return;
                }


                // CLIENTE

                SimpleInteract cliente =
                    hit.collider.GetComponentInParent<SimpleInteract>();

                if (cliente != null)
                {
                    cliente.Interact();
                    return;
                }



                // PEDIDO


                PedidoPickup pedido =
                    hit.collider.GetComponentInParent<PedidoPickup>();

                if (pedido != null)
                {
                    pedido.Interact();
                    return;
                }

            }
        }
    }



    // DIÁLOGOS


    public void MostrarDialogo(string mensaje)
    {
        if (textoSubtitulos == null)
            return;

        if (dialogoActual != null)
        {
            StopCoroutine(dialogoActual);
        }

        textoSubtitulos.text = mensaje;

        dialogoActual = StartCoroutine(LimpiarTextoCoroutine());
    }


    IEnumerator LimpiarTextoCoroutine()
    {
        yield return new WaitForSeconds(4f);

        if (textoSubtitulos != null)
        {
            textoSubtitulos.text = "";
        }

        dialogoActual = null;
    }



    // ILUMINACIÓN


    public void CambiarIluminacion(string estado)
    {
        if (lucesNormales != null)
            lucesNormales.SetActive(false);

        if (lucesServicio != null)
            lucesServicio.SetActive(false);

        if (lucesCombate != null)
            lucesCombate.SetActive(false);


        switch (estado)
        {
            case "Normal":

                if (lucesNormales != null)
                    lucesNormales.SetActive(true);

                break;


            case "Servicio":

                if (lucesServicio != null)
                    lucesServicio.SetActive(true);

                break;


            case "Combate":

                if (lucesCombate != null)
                    lucesCombate.SetActive(true);

                break;


            case "Apagado":

                break;
        }
    }



    // OBJETIVOS


    public void ActualizarObjetivo(string nuevoObjetivo)
    {
        if (textoObjetivo != null)
        {
            textoObjetivo.text = "- " + nuevoObjetivo;
        }
    }



    // LIMPIEZA INICIAL


    public void RecogerElementosLimpieza()
    {
        if (tieneElementosLimpieza)
            return;

        tieneElementosLimpieza = true;

        Debug.Log("Elementos de limpieza recogidos.");

        MostrarDialogo(
            "Bien... será mejor limpiar todo esto antes de empezar."
        );

        ActualizarObjetivoLimpieza();
    }


    public void ManchaParedLimpiada()
    {
        if (!tieneElementosLimpieza)
        {
            Debug.Log("NO SE PUEDE LIMPIAR: todavía no tiene los elementos de limpieza.");
            MostrarDialogo("Necesito buscar los elementos de limpieza primero.");
            return;
        }

        if (limpiezaTerminada)
            return;

        manchasParedLimpiadas++;

        if (manchasParedLimpiadas > totalManchasPared)
        {
            manchasParedLimpiadas = totalManchasPared;
        }

        Debug.Log(
            "MANCHA DE PARED LIMPIADA | " +
            "Contador: " +
            manchasParedLimpiadas +
            "/" +
            totalManchasPared
        );

        ActualizarObjetivoLimpieza();

        ComprobarLimpieza();
    }


    public void ManchaPisoLimpiada()
    {
        if (!tieneElementosLimpieza)
        {
            MostrarDialogo(
                "Necesito buscar los elementos de limpieza primero."
            );

            return;
        }

        if (limpiezaTerminada)
            return;

        manchasPisoLimpiadas++;

        if (manchasPisoLimpiadas > totalManchasPiso)
        {
            manchasPisoLimpiadas = totalManchasPiso;
        }

        Debug.Log(
            "Manchas de piso: " +
            manchasPisoLimpiadas +
            "/" +
            totalManchasPiso
        );

        ActualizarObjetivoLimpieza();

        ComprobarLimpieza();
    }


    void ActualizarObjetivoLimpieza()
    {
        if (textoObjetivo == null)
            return;

        textoObjetivo.text =
            "- Manchas de pared: " +
            manchasParedLimpiadas +
            "/" +
            totalManchasPared +
            "\n" +
            "- Manchas de piso: " +
            manchasPisoLimpiadas +
            "/" +
            totalManchasPiso;
    }


    void ComprobarLimpieza()
    {
        if (limpiezaTerminada)
            return;

        bool paredTerminada =
            manchasParedLimpiadas >= totalManchasPared;

        bool pisoTerminado =
            manchasPisoLimpiadas >= totalManchasPiso;


        if (paredTerminada && pisoTerminado)
        {
            limpiezaTerminada = true;

            Debug.Log("Limpieza terminada.");

            StartCoroutine(FinalizarLimpieza());
        }
    }


    IEnumerator FinalizarLimpieza()
    {
        ActualizarObjetivo("Limpieza completada");

        MostrarDialogo(
            "Listo... ya está todo limpio."
        );

        yield return new WaitForSeconds(3f);

        StartCoroutine(SecuenciaInicio());
    }



    // INICIO DEL ACTO 3


    IEnumerator SecuenciaInicio()
    {
        if (effectoParpadeo != null)
        {
            effectoParpadeo.IniciarParpadeo();
        }

        yield return new WaitForSeconds(1.5f);


        MostrarDialogo(
            "Ya casi... una ronda mas y bajo a buscarlas. Tienen que estar por despertar"
        );

        yield return new WaitForSeconds(3f);


        CambiarIluminacion("Servicio");


        if (effectoParpadeo != null)
        {
            effectoParpadeo.IniciarParpadeo();
        }


        if (clientesActo3 != null)
        {
            clientesActo3.SetActive(true);
        }


        ActualizarObjetivo(
            "Atiende a las entidades de la barra (0/2)"
        );
    }



    // PEDIDOS


    public bool TienePedidoEntregable()
    {
        return tienePedido && tienePedidoBuscado;
    }


    public void TomarPedido(string pedido)
    {
        pedidoActual = pedido;
        tienePedido = true;
        tienePedidoBuscado = false;



        // PEDIDO ESPECIAL


        if (objetoEspecial != null)
        {
            if (
                pedidoActual.Trim().ToLower() ==
                pedidoQueLoActiva.Trim().ToLower()
            )
            {
                objetoEspecial.SetActive(true);
            }
            else
            {
                objetoEspecial.SetActive(false);
            }
        }


        ActualizarObjetivo(
            "Pedido: " + pedidoActual
        );


        Debug.Log(
            "Pedido tomado: " + pedidoActual
        );
    }


    public void RecogerPedido(string objeto)
    {
        Debug.Log(
            "Objeto recogido: " + objeto
        );

        Debug.Log(
            "Pedido actual: " + pedidoActual
        );


        if (
            tienePedido &&
            objeto.Trim().ToLower() ==
            pedidoActual.Trim().ToLower()
        )
        {
            tienePedidoBuscado = true;

            if (objetoEspecial != null)
            {
                objetoEspecial.SetActive(false);
            }

            Debug.Log("Pedido correcto.");


            ActualizarObjetivo(
                "Entregar pedido: " + pedidoActual
            );
        }
    }


    public void EntregarPedido()
    {
        tienePedido = false;
        tienePedidoBuscado = false;
        pedidoActual = "";

        ActualizarObjetivo(
            "Atiende a las entidades de la barra (" +
            clientesAtendidos +
            "/2)"
        );
    }



    // CONTEO DE CLIENTES


    public void ClienteCompletado()
    {
        clientesAtendidos++;


        ActualizarObjetivo(
            "Atiende a las entidades de la barra (" +
            clientesAtendidos +
            "/2)"
        );


        Debug.Log(
            "Entidades completas: " +
            clientesAtendidos
        );


        if (clientesAtendidos == 2)
        {
            StartCoroutine(AvanzarNoche());
        }
    }



    // AVANZAR NOCHE


    IEnumerator AvanzarNoche()
    {
        Debug.Log(
            "Los dos clientes fueron atendidos"
        );


        yield return new WaitForSeconds(4f);


        if (effectoParpadeo != null)
        {
            effectoParpadeo.IniciarParpadeo();
        }


        if (clientesActo3 != null)
        {
            clientesActo3.SetActive(false);
        }


        yield return new WaitForSeconds(3f);


        MostrarDialogo(
            "Listo... voy a buscarlas."
        );


        ActualizarObjetivo(
            "Ve al sótano"
        );


        yield return new WaitForSeconds(5f);


        CambiarIluminacion("Apagado");


        yield return new WaitForSeconds(1.5f);


        if (enemigos != null)
        {
            enemigos.SetActive(true);
        }



        // APARICIÓN DEL VIGILANTE


        if (vigilante != null && Camera.main != null)
        {
            Transform cam =
                Camera.main.transform;


            Vector3 posicion =
                cam.position +
                cam.forward * 5f;


            posicion.y =
                vigilante.transform.position.y;


            vigilante.transform.position =
                posicion;


            Vector3 direccion =
                cam.position -
                vigilante.transform.position;


            direccion.y = 0f;


            vigilante.transform.rotation =
                Quaternion.LookRotation(direccion);


            vigilante.transform.Rotate(
                0,
                90,
                0
            );


            vigilante.SetActive(true);
        }
    }



    // PARANOIA


    IEnumerator MantenerParanoiaMinima()
    {
        while (true)
        {
            if (ParanoiaSystem.Instance != null)
            {
                ParanoiaSystem.Instance.AddParanoia(1f);
            }

            yield return new WaitForSeconds(5f);
        }
    }



    // SÓTANO


    public void IrASotano()
    {
        enSotano = true;

        SceneManager.LoadScene(
            "Basement (pasto)"
        );
    }

}