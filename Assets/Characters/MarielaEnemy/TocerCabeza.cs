using UnityEngine;

public class TocerCabeza : MonoBehaviour
{
    [Header("Hueso de la cabeza")]
    public Transform headBone;

    [Header("Temblor muy sutil")]
    public bool usarTemblor = true;
    public float shakeAmount = 0.2f;
    public float shakeSpeed = 3f;

    [Header("Torcedura automática")]
    public bool torcerAutomaticamente = true;
    public float tiempoEntreTorceduras = 5f;

    [Header("Torcedura lateral")]
    public float tiltAngle = 25f;
    public float tiltDuration = 2f;
    public float holdDuration = 2.5f;
    public float returnDuration = 1.5f;

    [Header("Dirección de la torcedura")]
    public bool alternarLados = true;
    public bool torcerHaciaLaDerecha = true;

    [Header("Sonido de torcedura")]
    public AudioSource audioSource;
    public AudioClip sonidoTorcedura;
    public float volumenSonido = 1f;
    public bool variarPitch = true;
    public float pitchMin = 0.85f;
    public float pitchMax = 1.15f;

    private Quaternion baseLocalRotation;
    private Quaternion targetRotation;

    private float timer;
    private float autoTimer;
    private int state = 0;
    private int ladoActual = 1;

    void Start()
    {
        if (headBone == null)
        {
            Debug.LogWarning("Falta asignar el hueso de la cabeza.");
            enabled = false;
            return;
        }

        baseLocalRotation = headBone.localRotation;
        targetRotation = baseLocalRotation;

        autoTimer = tiempoEntreTorceduras;
        ladoActual = torcerHaciaLaDerecha ? 1 : -1;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void LateUpdate()
    {
        if (headBone == null) return;

        if (torcerAutomaticamente && state == 0)
        {
            autoTimer -= Time.deltaTime;

            if (autoTimer <= 0f)
            {
                TorcerCabeza();
                autoTimer = tiempoEntreTorceduras;
            }
        }

        Quaternion shake = Quaternion.identity;

        if (usarTemblor)
        {
            float shakeZ = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;

            shake = Quaternion.Euler(
                0f,
                0f,
                shakeZ
            );
        }

        if (state == 0)
        {
            headBone.localRotation = baseLocalRotation * shake;
        }
        else if (state == 1)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / tiltDuration);

            t = SuavizarMovimiento(t);

            headBone.localRotation = Quaternion.Slerp(
                baseLocalRotation,
                targetRotation,
                t
            ) * shake;

            if (t >= 1f)
            {
                state = 2;
                timer = 0f;
            }
        }
        else if (state == 2)
        {
            timer += Time.deltaTime;

            headBone.localRotation = targetRotation * shake;

            if (timer >= holdDuration)
            {
                state = 3;
                timer = 0f;
            }
        }
        else if (state == 3)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / returnDuration);

            t = SuavizarMovimiento(t);

            headBone.localRotation = Quaternion.Slerp(
                targetRotation,
                baseLocalRotation,
                t
            ) * shake;

            if (t >= 1f)
            {
                state = 0;
                timer = 0f;
            }
        }
    }

    public void TorcerCabeza()
    {
        if (headBone == null) return;

        timer = 0f;
        state = 1;

        if (alternarLados)
        {
            ladoActual *= -1;
        }
        else
        {
            ladoActual = torcerHaciaLaDerecha ? 1 : -1;
        }

        targetRotation = baseLocalRotation * Quaternion.Euler(
            0f,
            0f,
            tiltAngle * ladoActual
        );

        ReproducirSonidoTorcedura();
    }

    public void ResetearCabeza()
    {
        state = 0;
        timer = 0f;
        autoTimer = tiempoEntreTorceduras;
        headBone.localRotation = baseLocalRotation;
    }

    private void ReproducirSonidoTorcedura()
    {
        if (audioSource == null) return;
        if (sonidoTorcedura == null) return;

        if (variarPitch)
        {
            audioSource.pitch = Random.Range(pitchMin, pitchMax);
        }
        else
        {
            audioSource.pitch = 1f;
        }

        audioSource.PlayOneShot(sonidoTorcedura, volumenSonido);
    }

    private float SuavizarMovimiento(float t)
    {
        return t * t * (3f - 2f * t);
    }
}