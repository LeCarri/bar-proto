using UnityEngine;

public class LiquidContainer : MonoBehaviour
{
    public string containerName;

    [SerializeField]
    public Liquid liquid;

    [SerializeField]
    protected int quantity, maxQuantity;

    public void AddLiquid(int quantityToAdd) 
    {
        if (quantity + quantityToAdd <= maxQuantity)
            quantity += Mathf.RoundToInt(quantityToAdd);
    }

    public void RemoveLiquid(float quantityToRemove)
    {
        if (quantity - quantityToRemove >= 0)
            quantity -= Mathf.RoundToInt(quantityToRemove);
    }

    public float GetStoragedLiquid() { return quantity; }

    public float GetMaxStoragedLiquid() { return maxQuantity; }

    public string GetLiquidType() { return liquid.ToString(); }
}

public enum Liquid{ empty, water, soda, beer, wine }
