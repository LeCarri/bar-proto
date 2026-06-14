using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Flashlight : MonoBehaviour
{
    [Header("Configuración de Luz")]
    public Light flashlightLight;      
    public float explorationAngle = 50f;
    public float focusedAngle = 20f;
    public float explorationIntensity = 1.5f;
    public float focusedIntensity = 4f;

    [Header("Batería")]
    public float battery = 10000f;
    public float maxBattery = 10000f;
    public float consumptionRate = 10f; 
    public float rechargeRate = 2f;    

    [Header("Combate")]
    public float range = 10f;
    public float damagePerSecond = 40f;
    public LayerMask enemyLayer;       

    [Header("Puntos De Origen")]
    public Transform firePoint; 

    [Header("Partículas")]
    [SerializeField] GameObject particlesObj;
    private ParticleSystem particlesSys;

    private bool isFocused = false;

    void Start()
    {
        if (flashlightLight == null) flashlightLight = GetComponent<Light>();
        flashlightLight.spotAngle = explorationAngle;
        flashlightLight.intensity = explorationIntensity;
    
        if (particlesObj != null)
        {
            particlesSys = particlesObj.GetComponent<ParticleSystem>();
            particlesObj.SetActive(false);
        }
    }

    void Update()
    {
        HandleInput();
        HandleBattery();
        HandleCombat();
        UpdateLightVisuals();
    }

    void HandleInput()
    {
        if (Input.GetMouseButton(1) && battery > 0)
        {
            isFocused = true;
        }
        else
        {
            isFocused = false;
        }
    }

    void HandleBattery()
    {
        if (isFocused)
        {
            battery -= consumptionRate * Time.deltaTime;
            battery = Mathf.Clamp(battery, 0, maxBattery);
        }
    }

    void HandleCombat()
    {
        if (isFocused)
        {
            RaycastHit hit;
            Camera mainCam = Camera.main; 
            if (mainCam == null) return;

            // Raycast desde el centro exacto de la pantalla
            Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out hit, range, enemyLayer))
            {
                EnemyCore enemy = hit.collider.GetComponentInParent<EnemyCore>();
                
                if (enemy != null)
                {
                    // Inflige daño por segundo de manera fluida
                    enemy.TakeDamage(damagePerSecond * Time.deltaTime);

                    // Activa y posiciona las partículas solo si golpea al enemigo
                    if (particlesObj != null)
                    {
                        ProcessParticles(hit);
                    }
                }
                else 
                {
                    StopParticles();
                }
            }
            else
            {
                StopParticles();
            }
        }
        else
        {
            StopParticles();
        }
    }

   private void ProcessParticles(RaycastHit rayHit) 
{
    if (particlesObj == null) return;
    
    // Si estaba apagado, lo prendemos
    if (!particlesObj.activeSelf) particlesObj.SetActive(true);
    
    // Posicionamos y orientamos
    particlesObj.transform.position = rayHit.point;
    particlesObj.transform.forward = rayHit.normal; 

    // Si no estaba reproduciendo, le damos Play
    if (!particlesSys.isPlaying) particlesSys.Play();
}

private void StopParticles()
{
    if (particlesObj != null)
    {
        if (particlesSys != null)
        {
            particlesSys.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // 🔥 Fuerza el frenado y limpia el buffer
            particlesSys.Clear(); // Doble limpieza por seguridad
        }
        
        // ¡LA CLAVE! Apagamos el objeto de raíz. Si está apagado, no puede haber humo flotando.
        particlesObj.SetActive(false); 
    }
}

    void UpdateLightVisuals()
    {
        float targetAngle = isFocused ? focusedAngle : explorationAngle;
        float targetIntensity = isFocused ? focusedIntensity : explorationIntensity;

        flashlightLight.spotAngle = Mathf.Lerp(flashlightLight.spotAngle, targetAngle, Time.deltaTime * 10f);
        flashlightLight.intensity = Mathf.Lerp(flashlightLight.intensity, targetIntensity, Time.deltaTime * 10f);

        if (battery <= 0 && isFocused)
        {
            flashlightLight.intensity = 0;
            StopParticles();
        }
    }

    public void Recharge(float amount)
    {
        battery = Mathf.Clamp(battery + amount, 0, maxBattery);
    }
}