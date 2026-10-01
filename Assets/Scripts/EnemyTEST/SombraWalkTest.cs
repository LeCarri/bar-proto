using UnityEngine;

public class SombraWalkTest : MonoBehaviour
{
    public float velocidad = 1.2f;

    void Update()
    {
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);
    }
}