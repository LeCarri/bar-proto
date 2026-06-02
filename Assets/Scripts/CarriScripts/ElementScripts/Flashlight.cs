using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Si vas a usar una barra de UI para la batería

public class Flashlight : MonoBehaviour
{
    [Header("Configuración de Luz")]
    public Light flashlightLight;      // Arrastrar aca el componente Light
    public float explorationAngle = 50f;
    public float focusedAngle = 20f;
    public float explorationIntensity = 1.5f;
    public float focusedIntensity = 4f;

    [Header("Batería")]
    public float battery = 10000f;
    public float maxBattery = 10000f;
    public float consumptionRate = 10f; // Cuánto gasta por segundo en modo enfoque
    public float rechargeRate = 2f;    // Opcional: si queremos que se recupere sola un poco

    [Header("Combate")]
    public float range = 10f;
    public float damagePerSecond = 40f;
    public LayerMask enemyLayer;       // Asegurate de poner a los enemigos en esta Layer

    [Header ("Puuntos De Origen")]
    public Transform firePoint; //se colocca el objeto FirePoint

    [Header("Particulas")]
    [InspectorName("Particle System")]
    [SerializeField] GameObject particlesObj;
    private ParticleSystem particlesSys;

    [SerializeField] private float particlesCooldown = 0.5f;
    private float currParticleCooldown = 0;

    private bool isFocused = false;

    void Start()
    {
        // Inicializar la luz en modo exploración
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

        if (currParticleCooldown < particlesCooldown)
            currParticleCooldown += Time.deltaTime;
        else
            RestartParticles();
    }

    void HandleInput()
    {
        // Click derecho para enfocar, solo si tenemos batería
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
        else if (battery < maxBattery)
        {
            // Opcional: recarga muy lenta si no se usa
            // battery += rechargeRate * Time.deltaTime; 
        }
    }

   void HandleCombat()
    {
        // Agregamos la verificación de que firePoint no sea nulo
        if (isFocused && firePoint != null)
        {
            RaycastHit hit;

            // 🔥 CONFIGURACIÓN REALISTA: Todo atado al objeto físico de la linterna
            // El origen es la punta física (firePoint)
            Vector3 rayOrigin = firePoint.position;
            
            // La dirección es HACIA DONDE APUNTA el firePoint (su eje azul Z)
            Vector3 rayDirection = firePoint.forward; 

            // Visualización del gizmo en la pestaña Escena (Línea amarilla)
            Debug.DrawRay(rayOrigin, rayDirection * range, Color.yellow);

            // Ejecutamos el Raycast siguiendo la orientación de la linterna
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, range, enemyLayer))
            {
                // Línea roja indicando el impacto real en el enemigo
                Debug.DrawLine(rayOrigin, hit.point, Color.red);
            
                EnemyCore enemy = hit.collider.GetComponent<EnemyCore>();
                if (enemy != null)
                {
                    if (particlesObj != null)
                        HandleParticles(hit);

                    enemy.TakeDamage(damagePerSecond);
                }
            }
        }
    }

    void UpdateLightVisuals()
    {
        // Suavizamos el cambio entre los dos modos de luz
        float targetAngle = isFocused ? focusedAngle : explorationAngle;
        float targetIntensity = isFocused ? focusedIntensity : explorationIntensity;

        flashlightLight.spotAngle = Mathf.Lerp(flashlightLight.spotAngle, targetAngle, Time.deltaTime * 10f);
        flashlightLight.intensity = Mathf.Lerp(flashlightLight.intensity, targetIntensity, Time.deltaTime * 10f);

        // Si se queda sin batería, la luz parpadea o se apaga
        if (battery <= 0 && isFocused)
        {
            flashlightLight.intensity = 0;
        }
    }

    // Método público para usar con items de inventario (pilas)
    public void Recharge(float amount)
    {
        battery = Mathf.Clamp(battery + amount, 0, maxBattery);
    }

    private void HandleParticles(RaycastHit rayHit) 
    {
        if (particlesObj == null) return;
        particlesObj.SetActive(true);
        particlesSys.Play();

        currParticleCooldown = 0;
        particlesObj.transform.parent = null;
        particlesObj.transform.position = rayHit.point;
        particlesObj.transform.localScale = Vector3.one;
        particlesObj.transform.LookAt(gameObject.transform.parent.transform);
    }

    private void RestartParticles()
    {

        if (particlesObj == null) return;
        particlesObj.transform.parent = null;
        particlesSys.Stop();
        particlesObj.SetActive(false);
    }
}