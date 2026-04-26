// ============================================================
// COPIA MODIFICADA PARA EL ACTO 2
// Original: Assets/Scripts/CarriScripts/ElementScripts/Flashlight.cs
// Cambios:
//   - consumptionRate por defecto: 20f (doble) → representa el pánico del Acto 2
//   - rechargeRate por defecto: 0f → no se recarga sola durante la psicosis
//   - EfectoPsicosis puede multiplicar consumptionRate en tiempo de ejecución
//   - Añade evento OnBatteryEmpty para notificar sistemas externos
// Colocar en: Assets/Scripts/Night_2 Scripts/  (o donde organicen el Acto 2)
// ============================================================
using UnityEngine;
using UnityEngine.UI;
using System;

public class Flashlight_Act2 : MonoBehaviour
{
    [Header("Configuración de Luz")]
    public Light flashlightLight;
    public float explorationAngle = 50f;
    public float focusedAngle = 20f;
    public float explorationIntensity = 1.5f;
    public float focusedIntensity = 4f;

    [Header("Batería — Acto 2 (agotamiento doble por pánico)")]
    public float battery = 100f;
    public float maxBattery = 100f;
    [Tooltip("20 en Acto 2 vs 10 en Acto 1 — representa el pánico del protagonista")]
    public float consumptionRate = 20f;
    [Tooltip("0 en Acto 2: la linterna NO se recarga sola durante la psicosis")]
    public float rechargeRate = 0f;

    [Header("UI (opcional)")]
    [Tooltip("Barra de UI para mostrar la batería. Puede dejarse vacía.")]
    public Image batteryBarUI;

    [Header("Combate")]
    public float range = 10f;
    public float damagePerSecond = 40f;
    public LayerMask enemyLayer;
    public Transform firePoint;

    [Header("Eventos")]
    [Tooltip("Callback cuando la batería llega a 0")]
    public Action OnBatteryEmpty;

    private bool isFocused = false;
    private bool batteryEmptyEventFired = false;
    private bool equipada = true;

    void Start()
    {
        if (flashlightLight == null) flashlightLight = GetComponent<Light>();
        if (flashlightLight != null)
        {
            flashlightLight.spotAngle = explorationAngle;
            flashlightLight.intensity = explorationIntensity;
        }
        else
        {
            Debug.LogError("[Flashlight_Act2] No se encontró un componente Light en este GameObject. " +
                           "Asigná la luz en el campo 'Flashlight Light' del Inspector.");
        }
    }

    void Update()
    {
        HandleToggleEquip();
        if (!equipada) return;

        HandleInput();
        HandleBattery();
        HandleCombat();
        UpdateLightVisuals();
        UpdateBatteryUI();
    }

    void HandleToggleEquip()
    {
        if (!Input.GetKeyDown(KeyCode.F)) return;

        if (flashlightLight == null)
        {
            Debug.LogError("[Flashlight_Act2] No se puede equipar/desequipar: falta el componente Light. " +
                           "Verificá que haya un Light en el mismo GameObject o asignalo en el Inspector.");
            return;
        }

        equipada = !equipada;
        flashlightLight.enabled = equipada;
        isFocused = false;
        Debug.Log($"[Flashlight_Act2] Linterna {(equipada ? "EQUIPADA" : "GUARDADA")}.");
    }

    void HandleInput()
    {
        isFocused = Input.GetMouseButton(1) && battery > 0;
    }

    void HandleBattery()
    {
        if (isFocused)
        {
            battery -= consumptionRate * Time.deltaTime;
            battery = Mathf.Clamp(battery, 0f, maxBattery);

            // Evento de batería vacía (una sola vez)
            if (battery <= 0f && !batteryEmptyEventFired)
            {
                batteryEmptyEventFired = true;
                OnBatteryEmpty?.Invoke();
                Act2Manager.Instance?.MostrarDialogo("Lucas: ¡La linterna! ¡Se acabó la batería!");
            }
        }
        else if (rechargeRate > 0f && battery < maxBattery)
        {
            // Recarga solo si rechargeRate > 0 (desactivado por defecto en Acto 2)
            battery += rechargeRate * Time.deltaTime;
            battery = Mathf.Clamp(battery, 0f, maxBattery);
            batteryEmptyEventFired = false;
        }
    }

    void HandleCombat()
    {
        if (!isFocused || firePoint == null) return;

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 rayOrigin = firePoint.position;
        Vector3 rayDirection = mainCam.transform.forward;

        Debug.DrawRay(rayOrigin, rayDirection * range, Color.yellow);

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, range, enemyLayer))
        {
            Debug.DrawLine(rayOrigin, hit.point, Color.red);
            EnemyCore enemy = hit.collider.GetComponent<EnemyCore>();
            if (enemy != null) enemy.TakeDamage(damagePerSecond);
        }
    }

    void UpdateLightVisuals()
    {
        float targetAngle = isFocused ? focusedAngle : explorationAngle;
        float targetIntensity = isFocused ? focusedIntensity : explorationIntensity;

        flashlightLight.spotAngle = Mathf.Lerp(flashlightLight.spotAngle, targetAngle, Time.deltaTime * 10f);
        flashlightLight.intensity = Mathf.Lerp(flashlightLight.intensity, targetIntensity, Time.deltaTime * 10f);

        // Parpadeo al agotar la batería
        if (battery <= 0f && isFocused)
        {
            flashlightLight.intensity = Mathf.Sin(Time.time * 30f) > 0f ? 0.3f : 0f;
        }
    }

    void UpdateBatteryUI()
    {
        if (batteryBarUI != null)
            batteryBarUI.fillAmount = battery / maxBattery;
    }

    public void Recharge(float amount)
    {
        battery = Mathf.Clamp(battery + amount, 0f, maxBattery);
        batteryEmptyEventFired = false;
    }

    public float GetBatteryPercent() => battery / maxBattery;
}
