using UnityEngine;

public class SillaInteractuable : MonoBehaviour, IInteractable
{
    [Header("Referencias de Posición")]
    [Tooltip("Arrastrá aquí el objeto vacío que marca dónde debe quedar la silla.")]
    public Transform posicionCorrecta; 
    
    [Header("Ajustes de Movimiento")]
    public float velocidad = 5f;
    
    private bool estaAcomodada = false;
    private bool moviendo = false;

    // Este es el método que llama el Raycast del Player
    public void Interact()
    {
        // Solo actuamos si la silla no está ya en su lugar y no se está moviendo
        if (!estaAcomodada && !moviendo)
        {
            if (posicionCorrecta == null)
            {
                Debug.LogError("¡Lucas! Te olvidaste de asignar la 'Posicion Correcta' en la silla: " + gameObject.name);
                return;
            }

            moviendo = true;
            Debug.Log("Acomodando silla...");

            // Avisamos al Act1Manager del progreso
            Act1Manager manager = FindObjectOfType<Act1Manager>();
            if (manager != null)
            {
                manager.SillaCompletada();
            }
        }
    }

    void Update()
    {
        if (moviendo)
        {
            // Movemos la silla hacia el objetivo de forma fluida
            transform.position = Vector3.MoveTowards(transform.position, posicionCorrecta.position, velocidad * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, posicionCorrecta.rotation, velocidad * 50f * Time.deltaTime);

            // Si llegamos a la posición final, detenemos todo
            if (Vector3.Distance(transform.position, posicionCorrecta.position) < 0.001f)
            {
                transform.position = posicionCorrecta.position;
                transform.rotation = posicionCorrecta.rotation;
                estaAcomodada = true;
                moviendo = false;
                Debug.Log("Silla en su sitio.");
            }
        }
    }

    // Texto que aparecería en la UI si la implementás
    public string GetDescription()
    {
        if (estaAcomodada) return "";
        return "Presiona [E] para acomodar silla";
    }
}