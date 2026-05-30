using UnityEngine;
using System.Collections;

/// <summary>
/// Enemigo del Acto 2. Hereda de EnemyCore para ser compatible con Flashlight y
/// Flashlight_Act2 (ambos buscan EnemyCore con GetComponent). Override de Die()
/// para notificar a Act2Manager en vez de Act1Manager.
/// En el prefab: reemplazar EnemyCore por EnemyCore_Act2.
/// </summary>
public class EnemyCore_Act2 : EnemyCore
{
    [Header("Muerte Acto 2")]
    [Tooltip("Si true, la sombra se desvanece antes de destruirse")]
    public bool desvanecerAlMorir = true;
    public float duracionDesvanecimiento = 0.8f;

    private bool muriendo = false;

    protected override void Die()
    {
        if (muriendo) return;
        muriendo = true;

        Act2Manager.Instance?.SombraDerrotada();

        SombrasCombate combate = Object.FindAnyObjectByType<SombrasCombate>();
        combate?.SombraEliminada();

        if (desvanecerAlMorir)
            StartCoroutine(DesvanecerYDestruir());
        else
            Destroy(gameObject);
    }

    IEnumerator DesvanecerYDestruir()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Color[] coloresOriginales = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            coloresOriginales[i] = renderers[i].material.color;

        float t = 0f;
        while (t < duracionDesvanecimiento)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / duracionDesvanecimiento);
            for (int i = 0; i < renderers.Length; i++)
            {
                Color c = coloresOriginales[i];
                c.a = alpha;
                renderers[i].material.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}
