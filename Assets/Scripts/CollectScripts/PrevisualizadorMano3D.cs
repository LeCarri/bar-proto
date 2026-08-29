using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
public class PrevisualizadorMano3D : MonoBehaviour
{
    [Header("Preview")]
    [SerializeField] private Transform puntoMano;
    [SerializeField] private ItemSO itemPrevisualizar;

    private GameObject objetoPreview;
    private ItemSO ultimoItem;

    private void Update()
    {
        // Esto es solamente para editar.
        // Durante Play no hace absolutamente nada.
        if (Application.isPlaying)
            return;

        if (puntoMano == null)
            return;

        // Si cambiamos de ItemSO, recreamos el modelo.
        if (itemPrevisualizar != ultimoItem)
        {
            CrearPreview();
            ultimoItem = itemPrevisualizar;
        }

        if (objetoPreview == null && itemPrevisualizar != null)
        {
            CrearPreview();
        }

        // Actualizamos en vivo posición, rotación y escala.
        if (objetoPreview != null && itemPrevisualizar != null)
        {
            objetoPreview.transform.localPosition =
                itemPrevisualizar.posicionEnMano;

            objetoPreview.transform.localRotation =
                Quaternion.Euler(itemPrevisualizar.rotacionEnMano);

            objetoPreview.transform.localScale =
                itemPrevisualizar.escalaEnMano;
        }
    }

    private void CrearPreview()
    {
        BorrarPreview();

        if (itemPrevisualizar == null ||
            itemPrevisualizar.prefab3D == null ||
            puntoMano == null)
        {
            return;
        }

        objetoPreview = Instantiate(
            itemPrevisualizar.prefab3D,
            puntoMano
        );

        objetoPreview.name = "PREVIEW_MANO_NO_BORRAR";

        objetoPreview.hideFlags =
            HideFlags.DontSaveInEditor |
            HideFlags.DontSaveInBuild;
    }

    private void OnDisable()
    {
        if (!Application.isPlaying)
        {
            BorrarPreview();
        }
    }

    private void BorrarPreview()
    {
        if (objetoPreview != null)
        {
            DestroyImmediate(objetoPreview);
            objetoPreview = null;
        }
    }
}
#endif