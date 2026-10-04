using System.Collections;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [Header("Componentes Base")]
    public Light spotLight;
    public Transform lightBeamMesh;
    public Camera mainCamera;
    public Transform cameraShakePivot;

    [Header("Apuntado al Crosshair")]
    public float maxAimDistance = 50f;
    public float rotationSpeed = 20f;
    public LayerMask aimIgnoreLayers;

    [Header("Ajustes de Luz (Exploración vs Ataque)")]
    public float normalAngle = 55f;
    public float focusAngle = 14f;           
    public float normalIntensity = 3.5f;
    public float focusIntensity = 10f;       
    public float burstIntensity = 25f;       
    public float smoothSpeed = 16f;

    [Header("Combate y Daño")]
    public float attackRange = 18f;
    public float damagePerSecond = 50f;
    public LayerMask enemyLayer;

    [Header("Impacto de Cámara / Zoom (Juice)")]
    public float defaultFOV = 60f;
    public float focusFOV = 50f;             
    public float continuousShakeIntensity = 0.05f; 
    public float burstShakeIntensity = 0.25f;       
    public float burstShakeDuration = 0.15f;

    [Header("Sonidos")]
    public AudioSource audioSourceToggle;   
    public AudioSource audioSourceLoop;     
    public AudioClip toggleSound;           
    public AudioClip focusStartSound;        
    public AudioClip focusLoopSound;         

    [Header("Partículas de Ambiente y Lente")]
    public ParticleSystem dustParticles;        // Polvo ambiental (activo cuando la linterna está encendida)
    public ParticleSystem sparksParticles;      // Chispas de sobrecarga al iniciar el ataque (Clic Derecho)
    public ParticleSystem lensSmokeParticles;   // Humo/Vapor al soltar el ataque

    [Header("Partículas de Impacto / Hit Enemigo")]
    public GameObject impactParticlesPrefab;    // Prefab o GameObject de partículas de hit
    private ParticleSystem particlesInstance;

    private bool isOn = false;
    private bool isAttacking = false;
    private float currentTargetIntensity;
    
    private Vector3 originalCameraPos;
    private float burstShakeTimer = 0f;

    void Start()
    {
        if (spotLight == null) spotLight = GetComponentInChildren<Light>();
        if (mainCamera == null) mainCamera = Camera.main;
        
        if (mainCamera != null)
        {
            defaultFOV = mainCamera.fieldOfView;
            if (cameraShakePivot == null) cameraShakePivot = mainCamera.transform;
            originalCameraPos = cameraShakePivot.localPosition;
        }

        // Configuración inicial: LINTERNA APAGADA
        spotLight.spotAngle = normalAngle;
        spotLight.intensity = normalIntensity;
        spotLight.enabled = false; 
        currentTargetIntensity = normalIntensity;

        if (lightBeamMesh != null)
        {
            lightBeamMesh.gameObject.SetActive(false);
        }

        // Apagar partículas al inicio
        if (dustParticles != null) dustParticles.Stop();
        if (sparksParticles != null) sparksParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (lensSmokeParticles != null) lensSmokeParticles.Stop();

        // Inicialización del sistema de partículas de Hit
        if (impactParticlesPrefab != null)
        {
            if (impactParticlesPrefab.scene.rootCount == 0)
            {
                // Si es un Prefab del Project, lo instanciamos
                GameObject obj = Instantiate(impactParticlesPrefab, transform);
                particlesInstance = obj.GetComponent<ParticleSystem>();
            }
            else
            {
                // Si ya es un GameObject en la Escena
                particlesInstance = impactParticlesPrefab.GetComponent<ParticleSystem>();
            }

            if (particlesInstance != null)
            {
                particlesInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particlesInstance.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        HandleInput();
        AimToCrosshair();
        HandleCombat();
        UpdateVisuals();
        HandleAudio();
        HandleContinuousCameraShake();
    }

    void HandleInput()
    {
        // --- ENCENDER / APAGAR CON TECLA F ---
        if (Input.GetKeyDown(KeyCode.F))
        {
            isOn = !isOn;
            spotLight.enabled = isOn;

            if (audioSourceToggle != null && toggleSound != null)
            {
                audioSourceToggle.PlayOneShot(toggleSound);
            }

            // Polvo de ambiente
            if (dustParticles != null)
            {
                if (isOn) dustParticles.Play();
                else dustParticles.Stop();
            }

            if (!isOn)
            {
                isAttacking = false;
                StopImpactParticles();
                ResetCameraPosition();
                
                if (lensSmokeParticles != null) lensSmokeParticles.Stop();
                if (sparksParticles != null) sparksParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                if (audioSourceLoop != null && audioSourceLoop.isPlaying)
                {
                    audioSourceLoop.Stop();
                }
                return;
            }
        }

        if (!isOn)
        {
            isAttacking = false;
            ResetCameraPosition();
            return;
        }

        // --- GOLPE INICIAL AL APRETAR CLIC DERECHO ---
        if (Input.GetMouseButtonDown(1))
        {
            spotLight.intensity = burstIntensity;
            
            if (lightBeamMesh != null)
            {
                lightBeamMesh.localScale = new Vector3(1.2f, 15f, 1.2f);
            }

            if (audioSourceToggle != null && focusStartSound != null)
            {
                audioSourceToggle.PlayOneShot(focusStartSound);
            }

            // Ráfaga de chispas
            if (sparksParticles != null)
            {
                sparksParticles.Play();
            }

            burstShakeTimer = burstShakeDuration;
        }

        // --- MANTENER/SOLTAR CLIC DERECHO ---
        if (Input.GetMouseButton(1))
        {
            isAttacking = true;
            currentTargetIntensity = focusIntensity;
        }
        else
        {
            if (isAttacking)
            {
                // Cortar chispas de golpe al soltar el botón
                if (sparksParticles != null)
                {
                    sparksParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                // Disparar humo del lente al enfriarse
                if (lensSmokeParticles != null)
                {
                    lensSmokeParticles.Play();
                }
            }

            isAttacking = false;
            currentTargetIntensity = normalIntensity;
            StopImpactParticles();
            ResetCameraPosition();
        }
    }

    void AimToCrosshair()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, ~aimIgnoreLayers) 
            ? hit.point 
            : ray.GetPoint(maxAimDistance);

        Vector3 targetDirection = targetPoint - transform.position;
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void HandleCombat()
    {
        if (!isOn || !isAttacking || mainCamera == null)
        {
            StopImpactParticles();
            return;
        }

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange, enemyLayer))
        {
            EnemyCore enemy = hit.collider.GetComponentInParent<EnemyCore>();
            if (enemy != null)
            {
                enemy.TakeDamage(damagePerSecond * Time.deltaTime);
            }

            PlayImpactParticles(hit);
        }
        else
        {
            StopImpactParticles();
        }
    }

    void PlayImpactParticles(RaycastHit hit)
    {
        if (particlesInstance == null) return;

        if (!particlesInstance.gameObject.activeSelf)
            particlesInstance.gameObject.SetActive(true);

        // Posicionar el efecto justo en el punto de contacto del Raycast y orientarlo hacia afuera según la normal
        particlesInstance.transform.position = hit.point;
        particlesInstance.transform.forward = hit.normal;

        if (!particlesInstance.isPlaying)
        {
            particlesInstance.Play();
        }
    }

    void StopImpactParticles()
    {
        if (particlesInstance != null && particlesInstance.gameObject.activeSelf)
        {
            particlesInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particlesInstance.gameObject.SetActive(false);
        }
    }

    void HandleAudio()
    {
        if (audioSourceLoop == null || focusLoopSound == null) return;

        if (isOn && isAttacking)
        {
            if (!audioSourceLoop.isPlaying)
            {
                audioSourceLoop.clip = focusLoopSound;
                audioSourceLoop.loop = true;
                audioSourceLoop.Play();
            }
        }
        else
        {
            if (audioSourceLoop.isPlaying)
            {
                audioSourceLoop.Stop();
            }
        }
    }

    void UpdateVisuals()
    {
        if (!isOn)
        {
            if (lightBeamMesh != null && lightBeamMesh.gameObject.activeSelf)
            {
                lightBeamMesh.gameObject.SetActive(false);
            }
            if (mainCamera != null)
            {
                mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, defaultFOV, Time.deltaTime * 12f);
            }
            return;
        }

        float targetAngle = isAttacking ? focusAngle : normalAngle;
        float targetFOV = isAttacking ? focusFOV : defaultFOV;

        spotLight.spotAngle = Mathf.Lerp(spotLight.spotAngle, targetAngle, Time.deltaTime * smoothSpeed);
        spotLight.intensity = Mathf.Lerp(spotLight.intensity, currentTargetIntensity, Time.deltaTime * smoothSpeed);

        if (mainCamera != null)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * 12f);
        }

        if (lightBeamMesh != null)
        {
            if (isAttacking)
            {
                if (!lightBeamMesh.gameObject.activeSelf) 
                    lightBeamMesh.gameObject.SetActive(true);

                Vector3 targetScale = new Vector3(0.6f, 15f, 0.6f);
                lightBeamMesh.localScale = Vector3.Lerp(lightBeamMesh.localScale, targetScale, Time.deltaTime * 15f);
            }
            else
            {
                if (lightBeamMesh.gameObject.activeSelf) 
                    lightBeamMesh.gameObject.SetActive(false);
            }
        }
    }

    void HandleContinuousCameraShake()
    {
        if (cameraShakePivot == null) return;

        if (isOn && isAttacking)
        {
            float currentIntensity = continuousShakeIntensity;

            if (burstShakeTimer > 0)
            {
                burstShakeTimer -= Time.deltaTime;
                currentIntensity = burstShakeIntensity;
            }

            float x = Random.Range(-1f, 1f) * currentIntensity;
            float y = Random.Range(-1f, 1f) * currentIntensity;

            cameraShakePivot.localPosition = originalCameraPos + new Vector3(x, y, 0f);
        }
        else
        {
            ResetCameraPosition();
        }
    }

    void ResetCameraPosition()
    {
        if (cameraShakePivot != null && cameraShakePivot.localPosition != originalCameraPos)
        {
            cameraShakePivot.localPosition = originalCameraPos;
        }
    }
}