using UnityEngine;
using System.Collections;

/// <summary>
/// NOCHE 2 — "En un punto nos quedamos sin pilas y la linterna no combate más, por lo que hay
/// que llegar a la puerta mientras nos persiguen."
///
/// Funciona con la linterna que ya usa la escena (Flashlight.cs, la de la Noche 1) SIN modificarla:
/// la hace parpadear, la apaga y deshabilita el componente, así la [F] y el clic derecho dejan de andar.
/// También es compatible con Flashlight_Act2 si algún día la usan.
///
/// SETUP: poner este script en el mismo objeto que tiene Flashlight (o en cualquier lado:
/// si los campos quedan vacíos busca la linterna sola). El Act2Manager lo llama solo.
/// </summary>
public class LinternaSinPilasAct2 : MonoBehaviour
{
    [Header("Referencias (se buscan solas si quedan vacías)")]
    public Flashlight linterna;
    public Flashlight_Act2 linternaAct2;
    public Light luz;

    [Header("Efecto")]
    public float duracionParpadeo = 2f;
    public AudioSource sonidoFallo;

    public bool SinPilas { get; private set; }

    void Start()
    {
        if (linterna == null) linterna = GetComponent<Flashlight>();
        if (linterna == null) linterna = Object.FindAnyObjectByType<Flashlight>();
        if (linternaAct2 == null) linternaAct2 = Object.FindAnyObjectByType<Flashlight_Act2>();

        if (luz == null)
        {
            if (linterna != null) luz = linterna.spotLight != null ? linterna.spotLight : linterna.GetComponentInChildren<Light>();
            else if (linternaAct2 != null) luz = linternaAct2.flashlightLight;
        }
    }

    public void AgotarPilas()
    {
        if (SinPilas) return;
        SinPilas = true;
        StartCoroutine(Agotar());
    }

    IEnumerator Agotar()
    {
        if (sonidoFallo != null) sonidoFallo.Play();

        // Parpadeo solo si la linterna estaba prendida
        if (luz != null && luz.enabled)
        {
            float original = luz.intensity;
            float t = 0f;
            while (t < duracionParpadeo)
            {
                float paso = Random.Range(0.04f, 0.14f);
                luz.intensity = Random.Range(0f, original);
                t += paso;
                yield return new WaitForSeconds(paso);
            }
            luz.intensity = original;
        }

        // Ya no combate ni prende
        if (linterna != null)
        {
            linterna.enabled = false;
            if (linterna.lightBeamMesh != null) linterna.lightBeamMesh.gameObject.SetActive(false);
            if (linterna.audioSourceLoop != null) linterna.audioSourceLoop.Stop();
            if (linterna.sparksParticles != null) linterna.sparksParticles.Stop();
        }
        if (linternaAct2 != null) linternaAct2.enabled = false;
        if (luz != null) luz.enabled = false;

        Debug.Log("[LinternaSinPilasAct2] La linterna se quedó sin pilas.");
    }
}
