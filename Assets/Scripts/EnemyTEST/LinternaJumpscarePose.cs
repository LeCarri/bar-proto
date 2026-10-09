
using System.Collections;
using UnityEngine;

public class LinternaJumpscarePose : MonoBehaviour
{
    [Header("Referencias")]
    public Transform puntoLinterna;
    public Transform poseJumpscare;

    [Header("Movimiento")]
    public float duracionEntrada = 0.12f;
    public float duracionSalida = 0.3f;

    private Vector3 posicionNormal;
    private Quaternion rotacionNormal;
    private Coroutine movimiento;

    void Start()
    {
        posicionNormal = puntoLinterna.localPosition;
        rotacionNormal = puntoLinterna.localRotation;
    }

    public void ActivarPoseJumpscare()
    {
        if (movimiento != null)
            StopCoroutine(movimiento);

        movimiento = StartCoroutine(Mover(
            poseJumpscare.localPosition,
            poseJumpscare.localRotation,
            duracionEntrada
        ));
    }

    public void RestaurarPoseNormal()
    {
        if (movimiento != null)
            StopCoroutine(movimiento);

        movimiento = StartCoroutine(Mover(
            posicionNormal,
            rotacionNormal,
            duracionSalida
        ));
    }

    IEnumerator Mover(
        Vector3 destinoPos,
        Quaternion destinoRot,
        float duracion
    )
    {
        Vector3 origenPos = puntoLinterna.localPosition;
        Quaternion origenRot = puntoLinterna.localRotation;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(
                tiempo / Mathf.Max(0.001f, duracion)
            );

            t = t * t * (3f - 2f * t);

            puntoLinterna.localPosition = Vector3.Lerp(
                origenPos,
                destinoPos,
                t
            );

            puntoLinterna.localRotation = Quaternion.Slerp(
                origenRot,
                destinoRot,
                t
            );

            yield return null;
        }

        puntoLinterna.localPosition = destinoPos;
        puntoLinterna.localRotation = destinoRot;
        movimiento = null;
    }

    [ContextMenu("Probar Pose Jumpscare")]
    public void ProbarPoseJumpscare()
    {
        ActivarPoseJumpscare();
    }

    [ContextMenu("Restaurar Linterna")]
    public void ProbarRestauracion()
    {
        RestaurarPoseNormal();
    }
}
