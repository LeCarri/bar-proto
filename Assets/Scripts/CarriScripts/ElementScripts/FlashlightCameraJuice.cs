using System.Collections;
using UnityEngine;

public class FlashlightCameraJuice : MonoBehaviour
{
    [Header("Referencias de Cámara")]
    public Camera mainCamera;
    public Transform cameraShakePivot; // Asigná acá el 'CameraPivot' o un objeto padre de la cámara

    [Header("Ajustes de Zoom (FOV)")]
    public float defaultFOV = 60f;
    public float attackFOV = 50f;
    public float fovSpeed = 12f;

    [Header("Ajustes de Shake (Sacudida)")]
    public float shakeIntensity = 0.18f;
    public float shakeDuration = 0.15f;

    private bool isShaking = false;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null && defaultFOV <= 0) defaultFOV = mainCamera.fieldOfView;
        
        // Si no asignaste un pivot separado, usa el transform de la cámara directamente
        if (cameraShakePivot == null && mainCamera != null) 
            cameraShakePivot = mainCamera.transform;
    }

    public void TriggerShake()
    {
        if (!isShaking && cameraShakePivot != null)
        {
            StartCoroutine(ShakeRoutine());
        }
    }

    public void ApplyZoom(bool isAttacking)
    {
        if (mainCamera == null) return;

        float targetFOV = isAttacking ? attackFOV : defaultFOV;
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * fovSpeed);
    }

    private IEnumerator ShakeRoutine()
    {
        isShaking = true;
        Vector3 originalPos = cameraShakePivot.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

            cameraShakePivot.localPosition = originalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraShakePivot.localPosition = originalPos;
        isShaking = false;
    }
}