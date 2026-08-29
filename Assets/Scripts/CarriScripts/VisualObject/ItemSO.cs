using UnityEngine;

public enum TipoItem
{
    Ninguno,
    VasoVacio,
    Cerveza,
    Honey,
    Trapo,
    Linterna
}

[CreateAssetMenu(fileName = "NuevoItem", menuName = "Juego/Item")]
public class ItemSO : ScriptableObject
{
    public string nombreItem;

    public TipoItem tipo;

    public GameObject prefab3D;

    [Header("Ajuste visual en la mano")]
    public Vector3 posicionEnMano = Vector3.zero;
    public Vector3 rotacionEnMano = Vector3.zero;
    public Vector3 escalaEnMano = Vector3.one;
}