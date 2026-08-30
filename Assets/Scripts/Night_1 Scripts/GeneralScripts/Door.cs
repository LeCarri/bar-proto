using UnityEngine;

public class Door : MonoBehaviour
{
    private AudioSource audioSource;
    private Rigidbody rb;

    [SerializeField] private float minSpeed = 0.05f;            //A partir de esta velocidad se empieza a reproducir el sonido de la bisagra

    [Header("Sonido")]
    [SerializeField] private float maxVolume = 0.8f;
    [SerializeField] private float maxSpeed = 3.0f;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        float speed = rb.angularVelocity.magnitude;

        if (speed > minSpeed) 
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

            float strength = Mathf.InverseLerp(minSpeed,maxSpeed,speed);

            audioSource.volume = strength * maxVolume;          //Se ajusta el volumen según la fuerza/velocidad con la que pasemos a través de la puerta

        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

    }


}
