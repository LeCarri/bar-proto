using UnityEngine;

public class CulpaAI : EnemyCore
{
    [Header("Configuración de La Culpa")]
    public AudioSource soundEffect; // Sonido específico (ej. un lamento o estática)
    public float detectionThreshold = 0.1f; // Sensibilidad al movimiento
    public float damagePenalty = 20f; // Daño si te movés

    private CharacterController playerController; 
    private bool isPlayerMoving = false;

    void Start()
    {
        // Buscamos el controlador del jugador para saber si se mueve
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerController = player.GetComponent<CharacterController>();
        
        // Al aparecer, suena el aviso
        if (soundEffect != null) soundEffect.Play();
    }

    void Update()
    {
        CheckPlayerMovement();

        if (isPlayerMoving)
        {
            // Si el jugador se mueve mientras La Culpa está activa, recibe castigo
            AttackPlayer();
        }
    }

    void CheckPlayerMovement()
    {
        if (playerController != null)
        {
            // Verificamos la velocidad del CharacterController o los Inputs
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Si los inputs son mayores al umbral, se considera movimiento
            isPlayerMoving = (Mathf.Abs(horizontal) > detectionThreshold || Mathf.Abs(vertical) > detectionThreshold);
        }
    }

     void AttackPlayer() //consecuenias de no correr
    {
        // 1. Daño a la Vida
        // Aca llamamos al script de la salud del Player
        // playerHealth.TakeDamage(damagePenalty * Time.deltaTime);

        // 2. Daño a la Paranoia (Efecto Visual)
        // Aca podemos modificar el fov y agregar un efecto visual de estatica o algo 
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 80f, Time.deltaTime * 5f);
    
        // 3. Feedback Auditivo
        // esto no se por si tenemos efectos de audio corte latidos de corazon o algo
        Debug.Log("La Culpa te quema: Daño físico + Aumento de Paranoia");
    }
}