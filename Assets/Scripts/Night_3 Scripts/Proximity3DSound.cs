using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Proximity3DSound : MonoBehaviour
{
    [Header("Jugador")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;

    [Header("Distancias")]
    [SerializeField] private float minDistance = 1.5f;
    [SerializeField] private float maxDistance = 8f;

    [Header("Volumen")]
    [SerializeField] private float maxVolume = 0.25f;
    [SerializeField] private float fadeSpeed = 3f;

    [Header("Opciones")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;

    private float targetVolume = 0f;

    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = loop;
            audioSource.spatialBlend = 1f; // 3D completo
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.dopplerLevel = 0f;
            audioSource.volume = 0f;

            if (clip != null)
            {
                audioSource.clip = clip;
            }
        }
    }

    void Start()
    {
        if (player == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag(playerTag);

            if (playerGO != null)
            {
                player = playerGO.transform;
            }
            else
            {
                Debug.LogWarning("[Proximity3DSound] No se encontró un objeto con tag Player.");
            }
        }

        if (audioSource != null && audioSource.clip != null && playOnStart)
        {
            audioSource.Play();
        }
    }

    void Update()
    {
        if (player == null || audioSource == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance >= maxDistance)
        {
            targetVolume = 0f;
        }
        else if (distance <= minDistance)
        {
            targetVolume = maxVolume;
        }
        else
        {
            float t = Mathf.InverseLerp(maxDistance, minDistance, distance);
            targetVolume = Mathf.Lerp(0f, maxVolume, t);
        }

        audioSource.volume = Mathf.Lerp(
            audioSource.volume,
            targetVolume,
            Time.deltaTime * fadeSpeed
        );

        if (!audioSource.isPlaying && audioSource.clip != null && audioSource.volume > 0.01f)
        {
            audioSource.Play();
        }
    }
}