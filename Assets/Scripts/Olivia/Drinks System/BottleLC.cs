using System.Collections;
using UnityEngine;

public class BottleLC : LiquidContainer
{
    [SerializeField]
    private int transferRate = 1;

    float transferingCooldown = 0.3f;
    bool canTransfer = true;

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.GetComponent<GlassLC>() && GetComponent<Pickup>().isHolding && canTransfer) 
        {
            TransferLiquid(collision.gameObject.GetComponent<GlassLC>());
            canTransfer = false;
            StartCoroutine(TransferingCooldown(transferingCooldown));
        }
    }

    private void TransferLiquid(GlassLC glass) 
    {
        //si el liquido es el mismo o nullo, transferir liquidos Y no llegó a su límite
        if (glass.GetStoragedLiquid() < glass.GetMaxStoragedLiquid() && 
                    (glass.liquid == liquid || glass.liquid == Liquid.empty)) 
        {
            if( glass.liquid == Liquid.empty)
                glass.liquid = liquid;

            glass.AddLiquid(transferRate);
            RemoveLiquid(transferRate);

            Debug.Log(quantity);
        }
    }

    IEnumerator TransferingCooldown(float cooldown) 
    {
        yield return new WaitForSeconds(cooldown);
        canTransfer = true;
        yield break;
    }
}
