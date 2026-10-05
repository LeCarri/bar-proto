using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SegundaCriaturaPared : MonoBehaviour
{
    [Header("Ruta por pared")]
    public Transform[] puntosRuta;

    [Header("DEBUG - Preview de ruta")]
    [Range(0, 20)]
    public int puntoPreview = 0;

    public bool actualizarPreview = false;

    [Header("Movimiento pared")]
    public float velocidad = 2.2f;
    public float velocidadRotacion = 7f;
    public float distanciaLlegada = 0.05f;

    [Header("Persecución")]
    public NavMeshAgent navMeshAgent;
    public SombraWalkTest persecucion;

    [Header("Animación")]
    public Animator animator;
    public string parametroMovimiento = "Walk";

    [Header("Transición al suelo")]
    public float radioBusquedaNavMesh = 1.5f;

    private bool secuenciaIniciada;

    private void Awake()
    {
        if (navMeshAgent != null)
            navMeshAgent.enabled = false;

        if (persecucion != null)
            persecucion.enabled = false;
    }

    public void IniciarSecuencia()
    {
        if (secuenciaIniciada)
            return;

        secuenciaIniciada = true;

        StartCoroutine(RecorrerPared());
    }

    private IEnumerator RecorrerPared()
    {
        if (puntosRuta == null ||
            puntosRuta.Length < 2)
        {
            Debug.LogError(
                "[SegundaCriaturaPared] Faltan puntos de ruta."
            );

            yield break;
        }


        // ==========================================
        // POSICIÓN INICIAL
        // ==========================================

        transform.position =
            puntosRuta[0].position;

        transform.rotation =
            puntosRuta[0].rotation;


        if (animator != null &&
            !string.IsNullOrEmpty(parametroMovimiento))
        {
            animator.SetBool(
                parametroMovimiento,
                true
            );
        }


        // ==========================================
        // RECORRER LA PARED
        // ==========================================

        for (int i = 1; i < puntosRuta.Length; i++)
        {
            Transform destino =
                puntosRuta[i];

            while (
                Vector3.Distance(
                    transform.position,
                    destino.position
                ) > distanciaLlegada
            )
            {
                transform.position =
                    Vector3.MoveTowards(
                        transform.position,
                        destino.position,
                        velocidad *
                        Time.deltaTime
                    );


                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        destino.rotation,
                        velocidadRotacion *
                        Time.deltaTime
                    );

                yield return null;
            }


            transform.position =
                destino.position;

            transform.rotation =
                destino.rotation;
        }


        // ==========================================
        // LLEGÓ AL PISO
        // ==========================================

        yield return StartCoroutine(
            PasarAPersecucion()
        );
    }


    private IEnumerator PasarAPersecucion()
    {
        // Frenamos brevemente la animación
        // mientras hacemos el cambio de sistema.

        if (animator != null &&
            !string.IsNullOrEmpty(parametroMovimiento))
        {
            animator.SetBool(
                parametroMovimiento,
                false
            );
        }


        // Buscar un punto válido del NavMesh
        // cerca de la posición final.

        NavMeshHit hit;

        bool encontroNavMesh =
            NavMesh.SamplePosition(
                transform.position,
                out hit,
                radioBusquedaNavMesh,
                NavMesh.AllAreas
            );


        if (!encontroNavMesh)
        {
            Debug.LogError(
                "[SegundaCriaturaPared] " +
                "No encontró NavMesh al bajar al suelo."
            );

            yield break;
        }


        // ==========================================
        // PONERLA DERECHA
        // ==========================================

        Vector3 euler =
            transform.eulerAngles;

        transform.rotation =
            Quaternion.Euler(
                0f,
                euler.y,
                0f
            );


        transform.position =
            hit.position;


        yield return null;


        // ==========================================
        // ACTIVAR NAVMESH
        // ==========================================

        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;

            navMeshAgent.Warp(
                hit.position
            );
        }


        // ==========================================
        // EMPEZAR PERSECUCIÓN
        // ==========================================

        if (persecucion != null)
        {
            persecucion.enabled = true;
        }


        Debug.Log(
            "[SegundaCriaturaPared] " +
            "Transición a persecución completada."
        );
    }

    private void OnValidate()
    {
        if (!actualizarPreview)
            return;

        if (puntosRuta == null ||
            puntosRuta.Length == 0)
            return;

        int indice =
            Mathf.Clamp(
                puntoPreview,
                0,
                puntosRuta.Length - 1
            );

        if (puntosRuta[indice] == null)
            return;

        transform.position =
            puntosRuta[indice].position;

        transform.rotation =
            puntosRuta[indice].rotation;
    }
}