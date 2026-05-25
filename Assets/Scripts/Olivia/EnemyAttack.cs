using UnityEngine;
using UnityEngine.Rendering;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField]
    [InspectorName("Daño")]
    private float damage = 5f;

    private void OnCollisionEnter(Collision collision)
    {
        if ( PlayerHealth.Instance != null && collision.transform.gameObject.CompareTag("Player"))
            PlayerHealth.Instance.RecibirDanio(damage);
    }
}
