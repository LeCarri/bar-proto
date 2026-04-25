using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    private const string VolumePrefsKey = "MasterVolume";
    private const string SensitivityPrefsKey = "CameraSensitivity";
    private const string PlayerTag = "Player";
    private const float MinMixerVolume = -80f;

    public static SettingsManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private string masterVolumeParameter = "MasterVolume";
    [SerializeField] [Range(0f, 1f)] private float defaultVolume = 1f;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumePercentLabel;

    [Header("Sensibilidad")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private float defaultSensitivity = 2f;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityPercentLabel;

    private float currentSensitivity;
    private readonly List<CinemachineInputAxisController> cameraInputControllers = new List<CinemachineInputAxisController>();
    private readonly Dictionary<CinemachineInputAxisController, float[]> baseInputGains = new Dictionary<CinemachineInputAxisController, float[]>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SetMasterVolume(float sliderValue)
    {
        float clampedValue = Mathf.Clamp01(sliderValue);
        PlayerPrefs.SetFloat(VolumePrefsKey, clampedValue);
        PlayerPrefs.Save();

        ApplyMasterVolume(clampedValue);
        UpdateSliderValue(volumeSlider, clampedValue);
        UpdatePercentLabel(volumePercentLabel, clampedValue);
    }

    public void SetCameraSensitivity(float sliderValue)
    {
        currentSensitivity = sliderValue;
        PlayerPrefs.SetFloat(SensitivityPrefsKey, currentSensitivity);
        PlayerPrefs.Save();

        ApplyCameraSensitivity();
        UpdateSliderValue(sensitivitySlider, currentSensitivity);
        UpdatePercentLabel(sensitivityPercentLabel, GetSliderNormalizedValue(sensitivitySlider, currentSensitivity));
    }

    private void LoadSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefsKey, defaultVolume);
        currentSensitivity = PlayerPrefs.GetFloat(SensitivityPrefsKey, defaultSensitivity);

        ApplyMasterVolume(savedVolume);
        FindPlayerControllerIfNeeded();
        ApplyCameraSensitivity();

        UpdateSliderValue(volumeSlider, savedVolume);
        UpdateSliderValue(sensitivitySlider, currentSensitivity);
        UpdatePercentLabel(volumePercentLabel, savedVolume);
        UpdatePercentLabel(sensitivityPercentLabel, GetSliderNormalizedValue(sensitivitySlider, currentSensitivity));
    }

    private void ApplyMasterVolume(float sliderValue)
    {
        if (mainMixer == null)
        {
            Debug.LogWarning("SettingsManager: Falta asignar MainMixer.");
            return;
        }

        float mixerVolume = sliderValue > 0f ? Mathf.Log10(sliderValue) * 20f : MinMixerVolume;
        if (!mainMixer.SetFloat(masterVolumeParameter, mixerVolume))
        {
            Debug.LogWarning($"SettingsManager: No se pudo modificar el parametro '{masterVolumeParameter}'. Revisar que este expuesto en MainMixer.");
        }
    }

    private void ApplyCameraSensitivity()
    {
        bool appliedSensitivity = false;

        if (FindPlayerControllerIfNeeded())
        {
            AssignPlayerCameraIfNeeded();
            playerController.lookSpeed = currentSensitivity;
            appliedSensitivity = true;
        }

        if (FindCameraInputControllersIfNeeded())
        {
            ApplyCinemachineSensitivity();
            appliedSensitivity = true;
        }

        if (!appliedSensitivity && GameObject.FindGameObjectWithTag(PlayerTag) != null)
        {
            Debug.LogWarning("SettingsManager: No se encontro el Player con tag 'Player' ni su controlador de camara.");
        }
    }

    private bool FindPlayerControllerIfNeeded()
    {
        if (playerController != null)
        {
            return true;
        }

        GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
        if (player == null)
        {
            return false;
        }

        playerController = player.GetComponent<PlayerController>();
        if (playerController == null)
        {
            playerController = player.GetComponentInChildren<PlayerController>(true);
        }

        return playerController != null;
    }

    private void AssignPlayerCameraIfNeeded()
    {
        if (playerController == null || playerController.playerCamera != null)
        {
            return;
        }

        playerController.playerCamera = playerController.GetComponentInChildren<Camera>(true);
    }

    private bool FindCameraInputControllersIfNeeded()
    {
        cameraInputControllers.RemoveAll(controller => controller == null);
        if (cameraInputControllers.Count > 0)
        {
            return true;
        }

        GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
        if (player == null)
        {
            return false;
        }

        CinemachineInputAxisController[] playerChildrenControllers = player.GetComponentsInChildren<CinemachineInputAxisController>(true);
        AddCameraInputControllers(playerChildrenControllers);

        CinemachineInputAxisController[] allControllers = FindObjectsOfType<CinemachineInputAxisController>(true);
        foreach (CinemachineInputAxisController controller in allControllers)
        {
            if (IsControllerTrackingPlayer(controller, player.transform))
            {
                AddCameraInputController(controller);
            }
        }

        if (cameraInputControllers.Count == 0 && allControllers.Length == 1)
        {
            AddCameraInputController(allControllers[0]);
        }

        return cameraInputControllers.Count > 0;
    }

    private void AddCameraInputControllers(CinemachineInputAxisController[] controllers)
    {
        foreach (CinemachineInputAxisController controller in controllers)
        {
            AddCameraInputController(controller);
        }
    }

    private void AddCameraInputController(CinemachineInputAxisController controller)
    {
        if (controller == null || cameraInputControllers.Contains(controller))
        {
            return;
        }

        cameraInputControllers.Add(controller);
        CaptureBaseInputGains(controller);
    }

    private bool IsControllerTrackingPlayer(CinemachineInputAxisController controller, Transform playerTransform)
    {
        if (controller == null || playerTransform == null)
        {
            return false;
        }

        CinemachineCamera cinemachineCamera = controller.GetComponent<CinemachineCamera>();
        if (cinemachineCamera == null)
        {
            return false;
        }

        return IsPlayerTransform(cinemachineCamera.Target.TrackingTarget, playerTransform)
            || IsPlayerTransform(cinemachineCamera.Target.LookAtTarget, playerTransform);
    }

    private bool IsPlayerTransform(Transform target, Transform playerTransform)
    {
        return target != null && (target == playerTransform || target.IsChildOf(playerTransform));
    }

    private void CaptureBaseInputGains(CinemachineInputAxisController controller)
    {
        if (controller == null || baseInputGains.ContainsKey(controller))
        {
            return;
        }

        controller.SynchronizeControllers();

        float[] gains = new float[controller.Controllers.Count];
        for (int i = 0; i < controller.Controllers.Count; i++)
        {
            gains[i] = controller.Controllers[i].Input.Gain;
        }

        baseInputGains.Add(controller, gains);
    }

    private void ApplyCinemachineSensitivity()
    {
        float sensitivityMultiplier = defaultSensitivity > 0f ? currentSensitivity / defaultSensitivity : currentSensitivity;
        sensitivityMultiplier = Mathf.Max(0f, sensitivityMultiplier);

        foreach (CinemachineInputAxisController controller in cameraInputControllers)
        {
            if (controller == null)
            {
                continue;
            }

            CaptureBaseInputGains(controller);
            if (!baseInputGains.TryGetValue(controller, out float[] gains))
            {
                continue;
            }

            for (int i = 0; i < controller.Controllers.Count && i < gains.Length; i++)
            {
                controller.Controllers[i].Input.Gain = gains[i] * sensitivityMultiplier;
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerController = null;
        cameraInputControllers.Clear();
        baseInputGains.Clear();
        FindPlayerControllerIfNeeded();
        ApplyCameraSensitivity();
    }

    private void UpdateSliderValue(Slider slider, float value)
    {
        if (slider == null)
        {
            return;
        }

        slider.SetValueWithoutNotify(value);
    }

    private float GetSliderNormalizedValue(Slider slider, float value)
    {
        if (slider == null)
        {
            return Mathf.Clamp01(value);
        }

        return Mathf.InverseLerp(slider.minValue, slider.maxValue, value);
    }

    private void UpdatePercentLabel(TextMeshProUGUI label, float normalizedValue)
    {
        if (label == null)
        {
            return;
        }

        label.text = $"{Mathf.RoundToInt(Mathf.Clamp01(normalizedValue) * 100f)}%";
    }
}
