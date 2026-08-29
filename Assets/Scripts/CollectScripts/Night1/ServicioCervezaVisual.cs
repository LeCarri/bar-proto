using System.Collections;
using UnityEngine;

public class ServicioCervezaVisual : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject chorroCerveza;
    [SerializeField] private Transform liquidoCerveza;

    [Header("Llenado")]
    [SerializeField] private float duracionLlenado = 2.5f;

    [Header("Chorro")]

    [Range(0f, 0.5f)]
    [SerializeField] private float inicioAcortarChorro = 0.20f;

    [Range(0.9f, 1f)]
    [SerializeField] private float porcentajeCorteChorro = 0.98f;

    public bool EstaSirviendo { get; private set; }

    private Vector3 escalaLlena;

    private Transform transformChorro;
    private Vector3 escalaOriginalChorro;
    private Vector3 posicionOriginalChorro;

    // 0 = X, 1 = Y, 2 = Z
    private int ejeLargoChorro = 1;

    private void Awake()
    {
        // ==========================
        // LÍQUIDO
        // ==========================
        if (liquidoCerveza != null)
        {
            escalaLlena = liquidoCerveza.localScale;
        }

        // ==========================
        // CHORRO
        // ==========================
        if (chorroCerveza != null)
        {
            transformChorro = chorroCerveza.transform;

            escalaOriginalChorro = transformChorro.localScale;
            posicionOriginalChorro = transformChorro.localPosition;

            // Detectamos automáticamente cuál es
            // el eje más largo del cilindro.
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
    }

    public void PrepararVasoVacio()
    {
        if (liquidoCerveza != null)
        {
            Vector3 escala = escalaLlena;

            // En tu vaso, Z es el eje vertical del líquido.
            escala.z = 0.01f;

            liquidoCerveza.localScale = escala;
        }

        // Restauramos el chorro por si ya fue utilizado antes.
        if (transformChorro != null)
        {
            transformChorro.localScale = escalaOriginalChorro;
            transformChorro.localPosition = posicionOriginalChorro;
        }

        if (chorroCerveza != null)
        {
            chorroCerveza.SetActive(false);
        }
    }

    public void Servir(System.Action alTerminar)
    {
        if (EstaSirviendo)
            return;

        StartCoroutine(SecuenciaServir(alTerminar));
    }

    private IEnumerator SecuenciaServir(System.Action alTerminar)
    {
        EstaSirviendo = true;

        PrepararVasoVacio();

        if (chorroCerveza != null)
        {
            chorroCerveza.SetActive(true);
        }

        float tiempo = 0f;

        while (tiempo < duracionLlenado)
        {
            tiempo += Time.deltaTime;

            float porcentaje =
                Mathf.Clamp01(tiempo / duracionLlenado);

            // ==========================
            // LLENAR CERVEZA
            // ==========================
            if (liquidoCerveza != null)
            {
                Vector3 escala = escalaLlena;

                escala.z = Mathf.Lerp(
                    0.01f,
                    escalaLlena.z,
                    porcentaje
                );

                liquidoCerveza.localScale = escala;
            }

            // ==========================
            // ACORTAR CHORRO
            // ==========================
            if (transformChorro != null)
            {
                float progresoChorro = Mathf.InverseLerp(
                    inicioAcortarChorro,
                    porcentajeCorteChorro,
                    porcentaje
                );

                Vector3 escala = escalaOriginalChorro;

                float escalaOriginal =
                    ObtenerComponente(
                        escalaOriginalChorro,
                        ejeLargoChorro
                    );

                float nuevaEscala = Mathf.Lerp(
                    escalaOriginal,
                    escalaOriginal * 0.02f,
                    progresoChorro
                );

                AsignarComponente(
                    ref escala,
                    ejeLargoChorro,
                    nuevaEscala
                );

                transformChorro.localScale = escala;

                // El centro del cilindro se acerca
                // progresivamente a la boca de la canilla.
                transformChorro.localPosition =
                    Vector3.Lerp(
                        posicionOriginalChorro,
                        Vector3.zero,
                        progresoChorro
                    );
            }

            // Cortamos el chorro justo antes de llenarse del todo.
            if (porcentaje >= porcentajeCorteChorro &&
                chorroCerveza != null &&
                chorroCerveza.activeSelf)
            {
                chorroCerveza.SetActive(false);
            }

            yield return null;
        }

        // Aseguramos que termine completamente lleno.
        if (liquidoCerveza != null)
        {
            liquidoCerveza.localScale = escalaLlena;
        }

        if (chorroCerveza != null)
        {
            chorroCerveza.SetActive(false);
        }

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

    private void AsignarComponente(
        ref Vector3 vector,
        int eje,
        float valor
    )
    {
        if (eje == 0)
            vector.x = valor;
        else if (eje == 1)
            vector.y = valor;
        else
            vector.z = valor;
    }
}