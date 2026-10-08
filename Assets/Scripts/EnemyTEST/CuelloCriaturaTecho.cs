
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
    public float duracionPrimerMovimiento = 0.35f;
    public float pausaIntermedia = 0.25f;
    public float duracionJumpscare = 0.10f;
    public float pausaJumpscare = 0.35f;
    public float duracionRetroceso = 0.4f;

    private bool ejecutando;

    void Start()
    {
        if (neck == null || poseInicial == null ||
            poseIntermedia == null || poseJumpscare == null)
        {
            Debug.LogError("Faltan referencias del cuello.");
            enabled = false;
            return;
        }

        AplicarPose(poseInicial);
    }

    void AplicarPose(Transform pose)
    {
        neck.localPosition = pose.localPosition;
        neck.localRotation = pose.localRotation;
    }

    public void IniciarJumpscare()
    {
        if (ejecutando || !enabled)
            return;

        StartCoroutine(SecuenciaJumpscare());
    }

    IEnumerator SecuenciaJumpscare()
    {
        ejecutando = true;

        // 1. Movimiento inquietante
        yield return MoverHaciaPose(
            poseIntermedia,
            duracionPrimerMovimiento,
            false
        );

        // 2. Pausa antes del susto
        yield return new WaitForSeconds(pausaIntermedia);

        // 3. Movimiento explosivo hacia la cámara
        yield return MoverHaciaPose(
            poseJumpscare,
            duracionJumpscare,
            true
        );

        // 4. Mantener la cara cerca
        yield return new WaitForSeconds(pausaJumpscare);

        // 5. Retroceso para comenzar el combate
        yield return MoverHaciaPose(
            poseIntermedia,
            duracionRetroceso,
            false
        );

        Debug.Log("Jumpscare terminado. Criatura lista para combate.");
    }

    IEnumerator MoverHaciaPose(
        Transform destino,
        float duracion,
        bool movimientoBrusco
    )
    {
        Vector3 posicionOrigen = neck.localPosition;
        Quaternion rotacionOrigen = neck.localRotation;

        Vector3 posicionDestino = destino.localPosition;
        Quaternion rotacionDestino = destino.localRotation;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(
                tiempo / Mathf.Max(0.001f, duracion)
            );

            if (movimientoBrusco)
            {
                t = 1f - Mathf.Pow(1f - t, 4f);
            }
            else
            {
                t = t * t * (3f - 2f * t);
            }

            neck.localPosition = Vector3.Lerp(
                posicionOrigen,
                posicionDestino,
                t
            );

            neck.localRotation = Quaternion.Slerp(
                rotacionOrigen,
                rotacionDestino,
                t
            );

            yield return null;
        }

        AplicarPose(destino);
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

        if (neck != null && poseInicial != null)
            AplicarPose(poseInicial);
    }
}
