
using System;
using System.Collections;
using UnityEngine;

public class CuelloCriaturaTecho : MonoBehaviour
{
    [Header("Hueso")]
    public Transform neck;

    [Header("Poses")]
    public Transform poseInicial;
    public Transform poseIntermedia;
    public Transform poseJumpscare;

    [Header("Tiempos - Primer Jumpscare")]
    public float duracionPrimerMovimiento = 0.18f;
    public float pausaIntermedia = 0f;
    public float duracionJumpscare = 0.08f;
    public float pausaJumpscare = 0.20f;
    public float duracionRetroceso = 0.35f;

    [Header("Seguimiento de Lucas")]
    public Transform objetivoMirada;
    public Transform puntoRostro;

    public float velocidadGiro = 6f;

    [Range(0f, 180f)]
    public float anguloMaximoGiro = 180f;

    [Header("Nuevo ataque hacia la camara")]
    public Transform puntoAtaqueCamara;

    [Min(0.01f)]
    public float duracionEmbestida = 0.25f;

    [Min(0f)]
    public float pausaImpacto = 0.15f;

    [Min(0.01f)]
    public float duracionRetrocesoAtaque = 0.35f;

    [Range(0f, 1f)]
    public float seguimientoDuranteAtaque = 1f;

    private bool seguimientoActivo;
    private bool cuelloOcupado;
    private bool ejecutando;

    void Start()
    {
        if (ReferenciasValidas())
            AplicarPose(poseInicial);
    }

    bool ReferenciasValidas()
    {
        return neck != null &&
               poseInicial != null &&
               poseIntermedia != null &&
               poseJumpscare != null;
    }

    public void IniciarJumpscare()
    {
        if (!ejecutando && ReferenciasValidas())
            StartCoroutine(ReproducirJumpscare(null, null));
    }

    // =====================================
    // PRIMER JUMPSCARE ORIGINAL
    // =====================================

    public IEnumerator ReproducirJumpscare(
        Action alImpacto,
        Action alRetroceso)
    {
        if (ejecutando || !ReferenciasValidas())
            yield break;

        ejecutando = true;
        cuelloOcupado = true;

        AplicarPose(poseInicial);

        yield return MoverHaciaPose(
            poseIntermedia,
            duracionPrimerMovimiento,
            false
        );

        yield return new WaitForSeconds(
            Mathf.Max(0f, pausaIntermedia)
        );

        alImpacto?.Invoke();

        yield return MoverHaciaPose(
            poseJumpscare,
            duracionJumpscare,
            true
        );

        yield return new WaitForSeconds(
            Mathf.Max(0f, pausaJumpscare)
        );

        alRetroceso?.Invoke();

        yield return MoverHaciaPose(
            poseIntermedia,
            duracionRetroceso,
            false
        );

        cuelloOcupado = false;
        ejecutando = false;
    }

    IEnumerator MoverHaciaPose(
        Transform destino,
        float duracion,
        bool brusco)
    {
        Vector3 origenPos = neck.localPosition;
        Quaternion origenRot = neck.localRotation;

        Vector3 destinoPos = destino.localPosition;
        Quaternion destinoRot = destino.localRotation;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(
                tiempo / Mathf.Max(0.001f, duracion)
            );

            if (brusco)
                t = 1f - Mathf.Pow(1f - t, 5f);
            else
                t = t * t * (3f - 2f * t);

            neck.localPosition = Vector3.Lerp(
                origenPos,
                destinoPos,
                t
            );

            neck.localRotation = Quaternion.Slerp(
                origenRot,
                destinoRot,
                t
            );

            yield return null;
        }

        AplicarPose(destino);
    }

    void AplicarPose(Transform pose)
    {
        if (neck == null || pose == null)
            return;

        neck.localPosition = pose.localPosition;
        neck.localRotation = pose.localRotation;
    }

    // =====================================
    // NUEVO ATAQUE DINAMICO
    // =====================================

    public IEnumerator EjecutarAtaqueCuello(
        Action alGolpear = null)
    {
        if (neck == null ||
            poseIntermedia == null ||
            puntoRostro == null ||
            puntoAtaqueCamara == null ||
            ejecutando)
        {
            Debug.LogWarning(
                "[Criatura3] No se puede ejecutar el ataque. " +
                "Revisar referencias del cuello y camara."
            );

            yield break;
        }

        ejecutando = true;
        cuelloOcupado = true;

        Vector3 posicionInicial = neck.position;
        Quaternion rotacionInicial = neck.rotation;

        float tiempo = 0f;

        float duracion = Mathf.Max(
            0.01f,
            duracionEmbestida
        );

        // La cabeza avanza hacia la camara
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / duracion);

            // Movimiento agresivo
            float avance =
                1f - Mathf.Pow(1f - t, 4f);

            // Primero orientamos la cara
            Vector3 direccion =
                puntoAtaqueCamara.position -
                puntoRostro.position;

            if (direccion.sqrMagnitude > 0.0001f)
            {
                Quaternion rotacionRostro =
                    Quaternion.LookRotation(
                        direccion.normalized,
                        Vector3.up
                    );

                Quaternion ajuste =
                    rotacionRostro *
                    Quaternion.Inverse(
                        puntoRostro.rotation
                    );

                Quaternion rotacionObjetivo =
                    ajuste * neck.rotation;

                neck.rotation = Quaternion.Slerp(
                    rotacionInicial,
                    rotacionObjetivo,
                    avance * seguimientoDuranteAtaque
                );
            }

            // Calculamos el desplazamiento necesario
            // para acercar el rostro al objetivo.
            Vector3 desplazamiento =
                puntoAtaqueCamara.position -
                puntoRostro.position;

            // Solo desplazamos parte de la distancia
            // correspondiente al avance del ataque.
            neck.position +=
                desplazamiento *
                avance *
                Mathf.Clamp01(seguimientoDuranteAtaque);

            yield return null;
        }

        // Ajuste final para que el rostro alcance
        // exactamente el punto de ataque.
        neck.position +=
            puntoAtaqueCamara.position -
            puntoRostro.position;

        // Momento del impacto
        alGolpear?.Invoke();

        yield return new WaitForSeconds(
            Mathf.Max(0f, pausaImpacto)
        );

        // Retroceso hacia la pose intermedia
        yield return MoverHaciaPose(
            poseIntermedia,
            duracionRetrocesoAtaque,
            false
        );

        cuelloOcupado = false;
        ejecutando = false;
    }

    // =====================================
    // SEGUIMIENTO MIENTRAS ACECHA
    // =====================================

    private void LateUpdate()
    {
        if (!seguimientoActivo ||
            cuelloOcupado ||
            objetivoMirada == null ||
            puntoRostro == null ||
            neck == null ||
            poseIntermedia == null)
            return;

        // Mantener posicion de acecho.
        neck.localPosition =
            poseIntermedia.localPosition;

        Vector3 direccion =
            objetivoMirada.position -
            puntoRostro.position;

        if (direccion.sqrMagnitude < 0.001f)
            return;

        Quaternion rotacionDeseadaRostro =
            Quaternion.LookRotation(
                direccion.normalized,
                Vector3.up
            );

        Quaternion ajuste =
            rotacionDeseadaRostro *
            Quaternion.Inverse(puntoRostro.rotation);

        Quaternion rotacionDeseadaCuello =
            ajuste * neck.rotation;

        Quaternion baseMundo =
            neck.parent.rotation *
            poseIntermedia.localRotation;

        rotacionDeseadaCuello =
            Quaternion.RotateTowards(
                baseMundo,
                rotacionDeseadaCuello,
                anguloMaximoGiro
            );

        neck.rotation = Quaternion.Slerp(
            neck.rotation,
            rotacionDeseadaCuello,
            Mathf.Clamp01(
                velocidadGiro * Time.deltaTime
            )
        );
    }

    public void ActivarSeguimiento()
    {
        seguimientoActivo = true;
    }

    public void DesactivarSeguimiento()
    {
        seguimientoActivo = false;
    }

    // =====================================
    // PRUEBAS
    // =====================================

    [ContextMenu("Probar Jumpscare")]
    public void ProbarJumpscare()
    {
        IniciarJumpscare();
    }

    [ContextMenu("Reiniciar Cuello")]
    public void ReiniciarCuello()
    {
        StopAllCoroutines();

        ejecutando = false;
        cuelloOcupado = false;
        seguimientoActivo = false;

        if (ReferenciasValidas())
            AplicarPose(poseInicial);
    }
}
