using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        // Hace que el objeto siempre mire a la cámara principal
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                         Camera.main.transform.rotation * Vector3.up);
    }

    public float amplitud = 0.2f; // Qué tanto sube y baja
    public float frecuencia = 2f; // Qué tan rápido lo hace
    Vector3 posInicial;

    void Start() => posInicial = transform.localPosition;

    void Update()
    {
        // Efecto de flotación senoidal
        float nuevaY = posInicial.y + Mathf.Sin(Time.time * frecuencia) * amplitud;
        transform.localPosition = new Vector3(posInicial.x, nuevaY, posInicial.z);
    }
}