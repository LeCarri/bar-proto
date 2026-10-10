
using System;
using System.Collections;
using UnityEngine;

public class ServicioCervezaVisual : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject chorroCerveza;
    [SerializeField] private Transform liquidoCerveza;

    [Header("Shader de llenado")]
    [SerializeField] private Renderer[] renderersLiquido;

    [SerializeField] private float alturaMin = 0f;
    [SerializeField] private float alturaMax = 1f;

    [Header("Llenado")]
    [SerializeField] private float duracionLlenado = 2.5f;

    [Header("Chorro")]
    [Range(0f, 0.5f)]
    [SerializeField] private float inicioAcortarChorro = 0.20f;

    [Range(0.9f, 1f)]
    [SerializeField] private float porcentajeCorteChorro = 0.98f;

    [Header("Espuma")]
    [SerializeField] private GameObject espumaCerveza;

    [Header("Aparición de espuma")]
    [SerializeField] private float duracionAparicionEspuma = 0.35f;

    public bool EstaSirviendo { get; private set; }

    private Transform transformChorro;
    private Vector3 escalaOriginalChorro;
    private Vector3 posicionOriginalChorro;

    private Vector3 escalaOriginalLiquido;
    private MaterialPropertyBlock bloquePropiedades;

    private static readonly int FillAmountID =
        Shader.PropertyToID("_FillAmount");

    private static readonly int AlturaMinID =
        Shader.PropertyToID("_AlturaMin");

    private static readonly int AlturaMaxID =
        Shader.PropertyToID("_AlturaMax");

    private int ejeLargoChorro = 1;

    private void Awake()
    {
        bloquePropiedades = new MaterialPropertyBlock();

        if (liquidoCerveza != null)
        {
            escalaOriginalLiquido = liquidoCerveza.localScale;
        }

        if (chorroCerveza != null)
        {
            transformChorro = chorroCerveza.transform;

            escalaOriginalChorro = transformChorro.localScale;
            posicionOriginalChorro = transformChorro.localPosition;

            if (escalaOriginalChorro == Vector3.zero)
                escalaOriginalChorro = Vector3.one;

            float x = Mathf.Abs(escalaOriginalChorro.x);
            float y = Mathf.Abs(escalaOriginalChorro.y);
            float z = Mathf.Abs(escalaOriginalChorro.z);

            if (x >= y && x >= z)
                ejeLargoChorro = 0;
            else if (y >= x && y >= z)
                ejeLargoChorro = 1;
            else
                ejeLargoChorro = 2;

            chorroCerveza.SetActive(false);
        }

        PrepararVasoVacio();
    }

    private void ActualizarNivel(float porcentaje)
    {
        if (renderersLiquido == null)
            return;

        foreach (Renderer rendererLiquido in renderersLiquido)
        {
            if (rendererLiquido == null)
                continue;

            rendererLiquido.GetPropertyBlock(bloquePropiedades);

            bloquePropiedades.SetFloat(
                FillAmountID, Mathf.Clamp01(porcentaje)
            );

            bloquePropiedades.SetFloat(
                AlturaMinID, alturaMin
            );

            bloquePropiedades.SetFloat(
                AlturaMaxID, alturaMax
            );

            rendererLiquido.SetPropertyBlock(
                bloquePropiedades
            );
        }
    }

    public void PrepararVasoVacio()
    {
        if (liquidoCerveza != null)
        {
            liquidoCerveza.localScale = escalaOriginalLiquido;
        }

        ActualizarNivel(0f);

        if (transformChorro != null)
        {
            transformChorro.localScale = escalaOriginalChorro;
            transformChorro.localPosition = posicionOriginalChorro;
        }

        if (chorroCerveza != null)
        {
            chorroCerveza.SetActive(false);
        }

        if (espumaCerveza != null)
        {
            espumaCerveza.SetActive(false);
        }
    }

    public void Servir(Action alTerminar)
    {
        if (EstaSirviendo)
            return;

        StartCoroutine(SecuenciaServir(alTerminar));
    }

    private IEnumerator SecuenciaServir(Action alTerminar)
    {
        EstaSirviendo = true;

        PrepararVasoVacio();

        if (chorroCerveza != null)
            chorroCerveza.SetActive(true);

        float tiempo = 0f;

        while (tiempo < duracionLlenado)
        {
            tiempo += Time.deltaTime;

            float porcentaje = duracionLlenado > 0f
                ? Mathf.Clamp01(tiempo / duracionLlenado)
                : 1f;

            // Llenar mediante el shader
            ActualizarNivel(porcentaje);

            // Acortar el chorro (logica original)
            if (transformChorro != null)
            {
                float progresoChorro = Mathf.InverseLerp(
                    inicioAcortarChorro,
                    porcentajeCorteChorro,
                    porcentaje
                );

                Vector3 escala = escalaOriginalChorro;

                float escalaOriginal = ObtenerComponente(
                    escalaOriginalChorro, ejeLargoChorro
                );

                float nuevaEscala = Mathf.Lerp(
                    escalaOriginal,
                    escalaOriginal * 0.02f,
                    progresoChorro
                );

                AsignarComponente(
                    ref escala, ejeLargoChorro, nuevaEscala
                );

                transformChorro.localScale = escala;

                transformChorro.localPosition = Vector3.Lerp(
                    posicionOriginalChorro,
                    Vector3.zero,
                    progresoChorro
                );
            }

            if (porcentaje >= porcentajeCorteChorro &&
                chorroCerveza != null &&
                chorroCerveza.activeSelf)
            {
                chorroCerveza.SetActive(false);
            }

            yield return null;
        }

        ActualizarNivel(1f);

        // Mostrar espuma cuando el vaso termina de llenarse
        if (espumaCerveza != null)
        {
            espumaCerveza.SetActive(true);
        }

        if (chorroCerveza != null)
            chorroCerveza.SetActive(false);

        yield return new WaitForSeconds(0.3f);

        EstaSirviendo = false;
        alTerminar?.Invoke();
    }

    private float ObtenerComponente(Vector3 vector, int eje)
    {
        if (eje == 0) return vector.x;
        if (eje == 1) return vector.y;
        return vector.z;
    }

    private void AsignarComponente(ref Vector3 vector, int eje, float valor)
    {
        if (eje == 0)
            vector.x = valor;
        else if (eje == 1)
            vector.y = valor;
        else
            vector.z = valor;
    }

}
