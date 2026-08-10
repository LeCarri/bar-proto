using UnityEngine;

public enum TipoItem { Ninguno, VasoVacio, Cerveza, Honey, Trapo, Linterna }

[CreateAssetMenu(fileName = "NuevoItem", menuName = "Juego/Item")]
public class ItemSO : ScriptableObject
{
    public string nombreItem;
    public TipoItem tipo;
    public GameObject prefab3D; // El modelo que se instanciará o mostrará en la mano
}