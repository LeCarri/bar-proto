using UnityEngine;

public class LucasCinematicMovement : MonoBehaviour
{
    private Animator animator;
    
    public Transform[] targets;

    [Header("Velocidad")]
    public float speed = 1f;
    public float rotationSpeed = 2f;
    public float stopAtDistance = 0.05f;

    [Header("Audio")]
    public AudioSource audioSteps;

    private bool isWalking = false;
    private int currentTarget = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isWalking)
        {
            GoToTarget();

            if (!audioSteps.isPlaying)
            {
                audioSteps.Play();
            }
        }
        else
        {
            if (audioSteps.isPlaying)
            {
                audioSteps.Stop();
            }
        }
    }

    public void StartWalking()
    {
        if (targets.Length == 0)
        {
            Debug.LogWarning("Lucas no tiene un Target asignado.");
            return;
        }

        isWalking = true;
        animator.SetBool("isWalking", true);

        
    }

    void GoToTarget()
    {
        Transform target = targets[currentTarget];

        if (target == null)
        {
            Debug.LogWarning("No hay Target asignado.");
            return;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;    // Mantener la posición en el eje Y
        
        float distance = direction.magnitude;

        if (distance <= stopAtDistance)
        {
            currentTarget++;

            if (currentTarget >= targets.Length)
            {
                isWalking = false;
                animator.SetBool("isWalking", false);
                return;
            }

            return;
        }

        direction.Normalize();

        transform.position += direction * speed * Time.deltaTime;

        Quaternion toRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

    }
    
}
