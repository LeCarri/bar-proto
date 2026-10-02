using UnityEngine;

public class LevitacionEnemigo : MonoBehaviour
{
    [Header("Ajustes de Levitación")]
    public float amplitud = 0.08f; // Distancia que sube/baja
    public float velocidad = 1.2f;  // Velocidad del bamboleo

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        // Flota suavemente en el eje Y
        float nuevoY = posicionInicial.y + Mathf.Sin(Time.time * velocidad) * amplitud;
        transform.position = new Vector3(posicionInicial.x, nuevoY, posicionInicial.z);
    }
}