using System.Collections;
using UnityEngine;

public class MarielaPeekPasillo : MonoBehaviour
{
    [Header("Hueso real")]
    public Transform headBone;

    [Header("Poses")]
    public Transform poseAsomo1;
    public Transform poseAsomo2;
    public Transform poseOculta;

    [Header("Tiempos")]
    public float duracionAsomo1 = 0.18f;
    public float duracionAsomo2 = 0.20f;
    public float duracionOcultar = 0.07f;
    public float esperaAntesDesaparecer = 0.03f;

    [Header("Opcional")]
    public Animator animatorMariela;

    private Coroutine rutina;

    private void OnEnable()
    {
        // Si el Animator pisa la cabeza, lo desactivamos
        if (animatorMariela != null)
            animatorMariela.enabled = false;

        if (headBone != null && poseOculta != null)
        {
            headBone.localPosition = poseOculta.localPosition;
            headBone.localRotation = poseOculta.localRotation;
        }
    }

    public void MostrarAsomoInicial()
    {
        IrAPose(poseAsomo1, duracionAsomo1, false);
    }

    public void MostrarMasCabeza()
    {
        IrAPose(poseAsomo2, duracionAsomo2, false);
    }

    public void OcultarYDesaparecer()
    {
        IrAPose(poseOculta, duracionOcultar, true);
    }

    private void IrAPose(Transform poseDestino, float duracion, bool desactivarAlFinal)
    {
        if (poseDestino == null || headBone == null)
            return;

        if (rutina != null)
            StopCoroutine(rutina);

        rutina = StartCoroutine(AnimarPose(poseDestino, duracion, desactivarAlFinal));
    }

    private IEnumerator AnimarPose(Transform poseDestino, float duracion, bool desactivarAlFinal)
    {
        Vector3 posInicial = headBone.localPosition;
        Quaternion rotInicial = headBone.localRotation;

        float t = 0f;
        duracion = Mathf.Max(0.01f, duracion);

        while (t < duracion)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duracion);

            // Suavizado
            k = k * k * (3f - 2f * k);

            headBone.localPosition = Vector3.Lerp(
                posInicial,
                poseDestino.localPosition,
                k
            );

            headBone.localRotation = Quaternion.Slerp(
                rotInicial,
                poseDestino.localRotation,
                k
            );

            yield return null;
        }

        headBone.localPosition = poseDestino.localPosition;
        headBone.localRotation = poseDestino.localRotation;

        if (desactivarAlFinal)
        {
            yield return new WaitForSeconds(esperaAntesDesaparecer);
            gameObject.SetActive(false);
        }

        rutina = null;
    }
}