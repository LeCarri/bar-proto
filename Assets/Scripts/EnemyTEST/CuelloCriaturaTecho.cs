
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

    [Header("Tiempos")]
    public float duracionPrimerMovimiento = 0.18f;
    public float pausaIntermedia = 0f;
    public float duracionJumpscare = 0.08f;
    public float pausaJumpscare = 0.20f;
    public float duracionRetroceso = 0.35f;

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

    public IEnumerator ReproducirJumpscare(
        Action alImpacto,
        Action alRetroceso)
    {
        if (ejecutando || !ReferenciasValidas())
            yield break;

        ejecutando = true;
        AplicarPose(poseInicial);

        // Primera aparición
        yield return MoverHaciaPose(
            poseIntermedia,
            duracionPrimerMovimiento,
            false
        );

        yield return new WaitForSeconds(
            Mathf.Max(0f, pausaIntermedia)
        );

        // Todos los efectos se disparan aquí
        alImpacto?.Invoke();

        // Extensión explosiva hacia Lucas
        yield return MoverHaciaPose(
            poseJumpscare,
            duracionJumpscare,
            true
        );

        yield return new WaitForSeconds(
            Mathf.Max(0f, pausaJumpscare)
        );

        // La linterna regresa junto con el cuello
        alRetroceso?.Invoke();

        yield return MoverHaciaPose(
            poseIntermedia,
            duracionRetroceso,
            false
        );

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
                origenPos, destinoPos, t
            );

            neck.localRotation = Quaternion.Slerp(
                origenRot, destinoRot, t
            );

            yield return null;
        }

        AplicarPose(destino);
    }

    void AplicarPose(Transform pose)
    {
        neck.localPosition = pose.localPosition;
        neck.localRotation = pose.localRotation;
    }

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

        if (ReferenciasValidas())
            AplicarPose(poseInicial);
    }
}
