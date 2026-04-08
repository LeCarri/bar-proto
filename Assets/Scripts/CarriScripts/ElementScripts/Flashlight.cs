using UnityEngine;
using UnityEngine.UI; // Si vas a usar una barra de UI para la batería

public class Flashlight : MonoBehaviour
{
    [Header("Configuración de Luz")]
    public Light flashlightLight;      // Arrastra aquí el componente Light
    public float explorationAngle = 50f;
    public float focusedAngle = 20f;
    public float explorationIntensity = 1.5f;
    public float focusedIntensity = 4f;

    [Header("Batería")]
    public float battery = 100f;
    public float maxBattery = 100f;
    public float consumptionRate = 10f; // Cuánto gasta por segundo en modo enfoque
    public float rechargeRate = 2f;    // Opcional: si querés que recupere algo sola

    [Header("Combate")]
    public float range = 10f;
    public float damagePerSecond = 40f;
    public LayerMask enemyLayer;       // Asegurate de poner a los enemigos en esta Layer

    private bool isFocused = false;

    void Start()
    {
        // Inicializar la luz en modo exploración
        if (flashlightLight == null) flashlightLight = GetComponent<Light>();
        flashlightLight.spotAngle = explorationAngle;
        flashlightLight.intensity = explorationIntensity;
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
        // Solo hacemos daño si estamos en modo enfoque
        if (isFocused)
        {
            RaycastHit hit;
            // Lanzamos el rayo desde el centro de la cámara/linterna
            if (Physics.Raycast(transform.position, transform.forward, out hit, range, enemyLayer))
            {
                // Buscamos el componente EnemyCore (el script padre de tus enemigos)
                EnemyCore enemy = hit.collider.GetComponent<EnemyCore>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerSecond);
                    // Feedback visual opcional: podrías poner chispas o luz aquí
                    Debug.DrawLine(transform.position, hit.point, Color.red); 
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
}