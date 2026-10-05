using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;

public class Act3Manager : MonoBehaviour
{
    public static Act3Manager Instance;

    public DistorsionPasilloAct3 triggerDistorsion;

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


    //LINTERNA EN MANO

    [Header("Linterna")]
    public GameObject linternaAct3;

    private bool limpiandoMancha = false;


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


    [Header("DEBUG")]
    public bool saltarLimpiezaAlIniciar = false;

    [Header("Limpieza Inicial")]
    public GameObject ElementosLimpieza;

    public bool tieneElementosLimpieza = false;

    public int manchasParedLimpiadas = 0;
    public int manchasPisoLimpiadas = 0;

    public int totalManchasPared = 5;
    public int totalManchasPiso = 5;

    private bool limpiezaTerminada = false;

    public bool sangreLimpiada = false;
    public bool elementosGuardados = false;

    private bool elementosYaGuardados = false;

    // PEDIDOS


    [Header("Pedidos")]
    public string pedidoActual = "";
    public bool tienePedido = false;
    public bool tienePedidoBuscado = false;

    [Header("Configuración pedidos")]
    public string nombrePedidoCerveza = "Cerveza";



    // SERVICIO DE CERVEZA


    [Header("Servicio de cerveza")]
    public ItemSO itemCerveza;
    public ItemSO itemVasoVacio;
    public ItemSO itemVasoPilar;

    public GameObject vasoServicio;
    public ServicioCervezaVisual servicioCervezaVisual;

    [HideInInspector]
    public bool vasoEnCanilla = false;

    private bool sirviendoCerveza = false;
    private bool servicioBebidasActivo = false;

    [Header("Audio servicio de cerveza")]
    public AudioSource audioServicioCerveza;
    public AudioClip sonidoServirCerveza;


    // OBJETO ESPECIAL


    [Header("Objeto especial")]
    public GameObject objetoEspecial;
    public string pedidoQueLoActiva = "Llave";


    [Header("Combate Final")]
    public GameObject laberintoCombate;
    public GameObject InterriorObject;
    public GameObject triggerInicioCombate;

    public LlegadaSalonAct3 llegadaSalon;

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

        if (triggerInicioCombate != null)
        {
            triggerInicioCombate.SetActive(false);
        }

        tieneElementosLimpieza = false;

        manchasParedLimpiadas = 0;
        manchasPisoLimpiadas = 0;

        limpiezaTerminada = false;

        StartCoroutine(MantenerParanoiaMinima());


        // DEBUG: saltar toda la limpieza
        if (saltarLimpiezaAlIniciar)
        {
            SaltarLimpiezaDebug();
            return;
        }


        ActualizarObjetivo("Busca los elementos de limpieza");
    }


    //OCULTAR Y MOSTRAR LINTERNA 

    public void OcultarLinterna()
    {
        if (linternaAct3 != null)
        {
            linternaAct3.SetActive(false);
        }
    }

    public void MostrarLinterna()
    {
        if (linternaAct3 != null)
        {
            linternaAct3.SetActive(true);
        }
    }

    public void SetLimpiandoMancha(bool limpiando)
    {
        limpiandoMancha = limpiando;
        ActualizarEstadoLinterna();
    }

    private void ActualizarEstadoLinterna()
    {
        if (linternaAct3 == null)
            return;

        bool tieneObjetoEnMano = false;

        if (ControladorMano3D.Instance != null)
        {
            tieneObjetoEnMano =
                ControladorMano3D.Instance.ObtenerItemActual() != null;
        }

        if (limpiandoMancha || tieneObjetoEnMano)
        {
            linternaAct3.SetActive(false);
        }
        else
        {
            linternaAct3.SetActive(true);
        }
    }


    // SERVICIO DE BEBIDAS


    public bool PuedeUsarServicioBebidas()
    {
        return servicioBebidasActivo;
    }


    public void ColocarVasoEnCanilla()
    {
        if (!PuedeUsarServicioBebidas())
        {
            MostrarDialogo(
                "Lucas: Ahora no es momento de preparar bebidas."
            );
            return;
        }

        if (ControladorMano3D.Instance == null)
            return;

        if (
            ControladorMano3D.Instance.ObtenerItemActual()
            != itemVasoVacio
        )
        {
            MostrarDialogo(
                "Lucas: Necesito un vaso primero."
            );
            return;
        }

        // Sacamos el vaso vacío de la mano
        ControladorMano3D.Instance.VaciarMano();

        // Dejamos el líquido vacío antes de mostrar el vaso
        if (servicioCervezaVisual != null)
        {
            servicioCervezaVisual.PrepararVasoVacio();
        }

        // Aparece el vaso debajo de la canilla
        if (vasoServicio != null)
        {
            vasoServicio.SetActive(true);
        }

        vasoEnCanilla = true;

        // Empieza automáticamente
        ServirCerveza();
    }


    public void ServirCerveza()
    {
        if (!vasoEnCanilla)
            return;

        if (sirviendoCerveza)
            return;

        if (servicioCervezaVisual == null)
        {
            Debug.LogError(
                "[Act3Manager] Falta asignar ServicioCervezaVisual."
            );
            return;
        }

        sirviendoCerveza = true;

        if (audioServicioCerveza != null &&
            sonidoServirCerveza != null)
        {
            audioServicioCerveza.clip = sonidoServirCerveza;
            audioServicioCerveza.Play();
        }

        servicioCervezaVisual.Servir(() =>
        {
            if (audioServicioCerveza != null)
            {
                audioServicioCerveza.Stop();
            }

            sirviendoCerveza = false;
            vasoEnCanilla = false;

            if (vasoServicio != null)
            {
                vasoServicio.SetActive(false);
            }

            if (ControladorMano3D.Instance != null)
            {
                ControladorMano3D.Instance.EquiparItem(
                    itemCerveza
                );
            }

            RecogerPedido(nombrePedidoCerveza);

            Debug.Log(
                "[Act3Manager] Cerveza servida. " +
                "Pedido actual: " +
                pedidoActual +
                " | Pedido conseguido: " +
                tienePedidoBuscado
            );
        });
    }



    // UPDATE / INTERACCIONES


    void Update()
    {
       
       ActualizarEstadoLinterna();

       if (Camera.main == null)
       return;


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


            // ROCOLA
            Rocola rocola =
                hit.collider.GetComponent<Rocola>();

            if (rocola != null)
            {
                mirandoAlgo = true;

                if (panelInteraccion != null)
                    panelInteraccion.SetActive(true);

                if (textoInteraccion != null)
                    textoInteraccion.text = "usar rocola";
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
                    textoInteraccion.text =
                        "Mantener E para limpiar";
            }


            // PUNTO DE VASOS
            PuntoVasosAct3 puntoVasos =
                hit.collider.GetComponentInParent<PuntoVasosAct3>();

            if (
                puntoVasos != null &&
                puntoVasos.PuedeInteractuar()
            )
            {
                mirandoAlgo = true;

                if (panelInteraccion != null)
                    panelInteraccion.SetActive(true);

                if (textoInteraccion != null)
                    textoInteraccion.text = "Tomar vaso";
            }


            // SERVICIO DE CERVEZA
            PuntoSuministroAct3 suministroCerveza =
                hit.collider.GetComponentInParent<
                    PuntoSuministroAct3
                >();

            if (
                suministroCerveza != null &&
                suministroCerveza.PuedeInteractuar()
            )
            {
                mirandoAlgo = true;

                if (panelInteraccion != null)
                    panelInteraccion.SetActive(true);

                if (textoInteraccion != null)
                    textoInteraccion.text =
                        "Servir cerveza";
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
                hit.collider.GetComponentInParent<
                    ElementosLimpieza
                >();

            if (limpieza != null)
            {
                mirandoAlgo = true;

                if (panelInteraccion != null)
                    panelInteraccion.SetActive(true);

                if (textoInteraccion != null)
                    textoInteraccion.text = "Recoger";

                if (elementosYaGuardados)
                {
                    panelInteraccion.SetActive(false);
                    return;
                }
            }

            // ARMARIO ELEMENTOS DE LIMPIEZA
            ArmarioElementosLimpieza armario =
                hit.collider.GetComponentInParent<
                    ArmarioElementosLimpieza
                >();

            if (armario != null)
            {
                mirandoAlgo = true;

                if (panelInteraccion != null)
                    panelInteraccion.SetActive(true);

                if (textoInteraccion != null)
                    textoInteraccion.text = "Guardar elementos";

                if (elementosYaGuardados)
                {
                    panelInteraccion.SetActive(false);
                    return;
                }
            }

            // OBJETO ESPECIAL
            ObjetosEspeciales objeto =
                hit.collider.GetComponentInParent<
                    ObjetosEspeciales
                >();

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
                // ROCOLA
                Rocola rocola =
                    hit.collider.GetComponentInParent<Rocola>();

                if (rocola != null)
                {
                    rocola.Interact();
                    return;
                }


                // ELEMENTOS DE LIMPIEZA
                ElementosLimpieza limpieza =
                    hit.collider.GetComponentInParent<
                        ElementosLimpieza
                    >();

                if (limpieza != null)
                {
                    limpieza.Interact();
                    return;
                }


                // PUERTA
                PuertaSotano puerta =
                    hit.collider.GetComponentInParent<
                        PuertaSotano
                    >();

                if (puerta != null)
                {
                    puerta.Interact();
                    return;
                }


                // MANCHA DE SANGRE
                ManchaSangre mancha =
                    hit.collider.GetComponentInParent<
                        ManchaSangre
                    >();

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

                // ARMARIO DE ELEMENTOS DE LIMPIEZA
                ArmarioElementosLimpieza armario =
                    hit.collider.GetComponentInParent<
                        ArmarioElementosLimpieza
                    >();

                if (armario != null)
                {
                    armario.Interact();
                    return;
                }

                // OBJETO ESPECIAL
                ObjetosEspeciales objeto =
                    hit.collider.GetComponentInParent<
                        ObjetosEspeciales
                    >();

                if (objeto != null)
                {
                    objeto.Interactuar();
                    return;
                }


                // CLIENTE
                SimpleInteract cliente =
                    hit.collider.GetComponentInParent<
                        SimpleInteract
                    >();

                if (cliente != null)
                {
                    cliente.Interact();
                    return;
                }


                // PUNTO DE VASOS
                PuntoVasosAct3 puntoVasos =
                    hit.collider.GetComponentInParent<
                        PuntoVasosAct3
                    >();

                if (puntoVasos != null)
                {
                    puntoVasos.Interact();
                    return;
                }


                // SERVICIO DE CERVEZA
                PuntoSuministroAct3 suministroCerveza =
                    hit.collider.GetComponentInParent<
                        PuntoSuministroAct3
                    >();

                if (suministroCerveza != null)
                {
                    suministroCerveza.Interact();
                    return;
                }


                // PEDIDO
                PedidoPickup pedido =
                    hit.collider.GetComponentInParent<
                        PedidoPickup
                    >();

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

        dialogoActual =
            StartCoroutine(LimpiarTextoCoroutine());
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


    public void ActualizarObjetivo(
        string nuevoObjetivo
    )
    {
        if (textoObjetivo != null)
        {
            textoObjetivo.text =
                "- " + nuevoObjetivo;
        }
    }



    // LIMPIEZA INICIAL


    public void SaltarLimpiezaDebug()
    {
        Debug.Log(
            "[DEBUG] Saltando limpieza inicial."
        );

        tieneElementosLimpieza = true;

        manchasParedLimpiadas =
            totalManchasPared;

        manchasPisoLimpiadas =
            totalManchasPiso;

        limpiezaTerminada = true;

        if (ElementosLimpieza != null)
        {
            ElementosLimpieza.SetActive(false);
        }

        ActualizarObjetivo(
            "Limpieza completada"
        );

        StartCoroutine(
            SecuenciaInicio()
        );
    }


    public void RecogerElementosLimpieza()
    {
        if (tieneElementosLimpieza)
            return;

        tieneElementosLimpieza = true;

        Debug.Log(
            "Elementos de limpieza recogidos."
        );

        MostrarDialogo(
            "Bueno... mejor limpio antes de arrancar."
        );

        ActualizarObjetivoLimpieza();
    }


    public void ManchaParedLimpiada()
    {
        if (!tieneElementosLimpieza)
        {
            Debug.Log(
                "NO SE PUEDE LIMPIAR: todavía no tiene los elementos de limpieza."
            );

            MostrarDialogo(
                "Necesito buscar los elementos de limpieza primero."
            );

            return;
        }

        if (limpiezaTerminada)
            return;

        manchasParedLimpiadas++;

        if (
            manchasParedLimpiadas >
            totalManchasPared
        )
        {
            manchasParedLimpiadas =
                totalManchasPared;
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

        if (
            manchasPisoLimpiadas >
            totalManchasPiso
        )
        {
            manchasPisoLimpiadas =
                totalManchasPiso;
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
            manchasParedLimpiadas >=
            totalManchasPared;

        bool pisoTerminado =
            manchasPisoLimpiadas >=
            totalManchasPiso;


        if (
            paredTerminada &&
            pisoTerminado
        )
        {
            limpiezaTerminada = true;

            Debug.Log(
                "Limpieza terminada."
            );

            StartCoroutine(
                FinalizarLimpieza()
            );
        }
    }


    IEnumerator FinalizarLimpieza()
    {
        sangreLimpiada = true;

        ActualizarObjetivo(
            "Ve al depósito y deja los elementos de limpieza en el armario"
        );

        MostrarDialogo(
            "Listo... ya está todo limpio. guardo lo que use y ya estoy."
        );

        yield return
            new WaitForSeconds(3f);
    }


    //guardar elementos de limpieza
    public void GuardarElementosLimpieza()
    {
        Debug.Log("INTENTANDO GUARDAR ELEMENTOS");

        if (elementosGuardados)
        {
            Debug.Log("[GUARDAR] Ya estaban guardados.");
            return;
        }

        if (!sangreLimpiada)
        {
            Debug.Log("[GUARDAR] Todavía NO terminó de limpiar la sangre.");
            return;
        }

        if (!tieneElementosLimpieza)
        {
            Debug.Log("[GUARDAR] NO tiene los elementos de limpieza.");
            return;
        }

        elementosGuardados = true;
        tieneElementosLimpieza = false;

        if (ElementosLimpieza != null)
        {
            ElementosLimpieza.SetActive(true);
        }

        Debug.Log("[GUARDAR] ¡ELEMENTOS GUARDADOS CORRECTAMENTE!");

        if (triggerDistorsion != null)
        {
            Debug.Log("[GUARDAR] triggerDistorsion está asignado.");
            triggerDistorsion.ActivarTrigger();
        }
        else
        {
            Debug.LogError("[GUARDAR] triggerDistorsion NO está asignado en el Inspector.");
        }

        elementosYaGuardados = true;
        tieneElementosLimpieza = false;

        if (ElementosLimpieza != null)
        {
            ElementosLimpieza.SetActive(true);
        }

        ActualizarObjetivo("Vuelve al salón");

        MostrarDialogo(
            "Listo. Ahora sí, puedo arrancar"
        );
    }


    // INICIAR SECUENCIA LLEGADA AL SALÓN
    public void IniciarSecuenciaSalon()
    {
        StartCoroutine(SecuenciaInicio());
    }


    // SECUENCIA INICIO 

    IEnumerator SecuenciaInicio()
    {
        if (effectoParpadeo != null)
        {
            effectoParpadeo.IniciarParpadeo();
        }

        yield return
            new WaitForSeconds(1.5f);

        MostrarDialogo(
            "Ya casi... una ronda mas y bajo a buscarlas. Tienen que estar por despertar"
        );

        yield return
            new WaitForSeconds(3f);

        CambiarIluminacion(
            "Servicio"
        );

        if (effectoParpadeo != null)
        {
            effectoParpadeo.IniciarParpadeo();
        }

        servicioBebidasActivo = true;

        ActualizarObjetivo(
            "Atiende a las entidades de la barra (0/2)"
        );
    }



    // PEDIDOS


    public bool TienePedidoEntregable()
    {
        return
            tienePedido &&
            tienePedidoBuscado;
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
            "Pedido: " +
            pedidoActual
        );


        Debug.Log(
            "Pedido tomado: " +
            pedidoActual
        );
    }

    public void RecogerPedido(string objeto)
{
    Debug.Log(
        "Objeto recogido: " +
        objeto
    );

    Debug.Log(
        "Pedido actual: " +
        pedidoActual
    );

    if (
        tienePedido &&
        objeto.Trim().ToLower() ==
        pedidoActual.Trim().ToLower()
    )
    {
        tienePedidoBuscado = true;

        // Si es el pedido especial de Pilar
        if (
            pedidoActual.Trim().ToLower() ==
            pedidoQueLoActiva.Trim().ToLower()
        )
        {
            if (
                ControladorMano3D.Instance != null &&
                itemVasoPilar != null
            )
            {
                ControladorMano3D.Instance.EquiparItem(
                    itemVasoPilar
                );
            }
        }

        if (objetoEspecial != null)
        {
            objetoEspecial.SetActive(false);
        }

        Debug.Log(
            "Pedido correcto."
        );

        ActualizarObjetivo(
            "Entregar pedido: " +
            pedidoActual
        );
    }
}
    
    


    public void EntregarPedido()
{
    if (ControladorMano3D.Instance != null)
    {
        ControladorMano3D.Instance.VaciarMano();
    }

    tienePedido = false;
    tienePedidoBuscado = false;
    pedidoActual = "";

    ActualizarObjetivo(
        "Atiende a las entidades de la barra (" +
        clientesAtendidos +
        "/2)"
    );

    Debug.Log("[Act3Manager] Pedido entregado. Mano vaciada.");
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
            StartCoroutine(
                AvanzarNoche()
            );
        }
    }


    //INICIO COMBATE FINAL
    public void ActivarTriggerInicioCombate()
    {
        if (triggerInicioCombate != null)
        {
            triggerInicioCombate.SetActive(true);

            Debug.Log("[COMBATE ACT3] Trigger de inicio de combate activado.");
        }
        else
        {
            Debug.LogWarning(
                "[COMBATE ACT3] No hay trigger de inicio de combate asignado."
            );
        }
    }

    public void IniciarCombateFinal()
    {
        Debug.Log("[COMBATE ACT3] INICIANDO COMBATE FINAL");

        if (laberintoCombate != null)
        {
            laberintoCombate.SetActive(true);

            Debug.Log(
                "[COMBATE ACT3] Laberinto activado."
            );
        }

        if (InterriorObject != null)
        {
            InterriorObject.SetActive(false);

            Debug.Log(
                "[COMBATE ACT3] Objeto desactivado."
            );
        }

        // REACTIVAR SALIDA DE LA BARRA

        if (llegadaSalon != null)
        {
            llegadaSalon.ReactivarSalida();

            Debug.Log(
                "[COMBATE ACT3] Salida de la barra reactivada."
            );
        }
        else
        {
            Debug.LogWarning(
                "[COMBATE ACT3] No hay LlegadaSalonAct3 asignado."
            );
        }

        ActualizarObjetivo("Llega al sotano");
    }


    // AVANZAR NOCHE
    IEnumerator AvanzarNoche()
    {
        servicioBebidasActivo = false;

        Debug.Log(
            "Los dos clientes fueron atendidos"
        );

       
        yield return new WaitForSeconds(4f);


        // DIÁLOGO DE PILAR

        MostrarDialogo(
            "Pilar: Gracias Pa!... ¿Te puedo pedir algo más?"
        );

        yield return new WaitForSeconds(3f);


        // DIÁLOGO DE LUCAS

        MostrarDialogo(
            "Lucas: ¿Qué?"
        );

        yield return new WaitForSeconds(2f);


        // DIÁLOGO DE PILAR

        MostrarDialogo(
            "Pilar: Irnos."
        );

        yield return new WaitForSeconds(1.5f);

        // FLASHES NEGROS

        if (effectoParpadeo != null)
        {
            effectoParpadeo.IniciarParpadeo();
        }
       

        // DESAPARECEN LOS CLIENTES

        if (clientesActo3 != null)
        {
            clientesActo3.SetActive(false);
        }

        if (effectoParpadeo != null)
        {
            effectoParpadeo.IniciarParpadeo();
        }

        yield return new WaitForSeconds(1f);

        // ACTIVO EL TRIGGER
        ActivarTriggerInicioCombate();
    }



    // PARANOIA

    IEnumerator MantenerParanoiaMinima()
    {
        while (true)
        {
            if (ParanoiaSystem.Instance != null)
            {
                ParanoiaSystem.Instance.AddParanoia(
                    1f
                );
            }

            yield return
                new WaitForSeconds(5f);
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