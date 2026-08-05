using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager Instance { get; private set; }

    [Header("Platform Detection")]
    public RuntimePlatform currentPlatform;
    public bool isMobile = false;
    public bool isPC = false;
    public bool isWebGL = false;

    [Header("Platform-Specific Settings")]
    public bool useTouchControls = false;
    public bool useMouseControls = true;
    public bool useKeyboardControls = true;

    [Header("Performance Settings")]
    public int targetFrameratePC = 144;
    public int targetFramerateMobile = 60;
    public int targetFramerateWebGL = 60;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        DetectPlatform();
        ApplyPlatformSettings();
    }

    private void DetectPlatform()
    {
        currentPlatform = Application.platform;

        // Check if mobile
        isMobile = currentPlatform == RuntimePlatform.IPhonePlayer ||
                   currentPlatform == RuntimePlatform.Android ||
                   currentPlatform == RuntimePlatform.WindowsPlayer && SystemInfo.deviceType == DeviceType.Handheld;

        // Check if PC
        isPC = currentPlatform == RuntimePlatform.WindowsPlayer ||
               currentPlatform == RuntimePlatform.WindowsEditor ||
               currentPlatform == RuntimePlatform.OSXPlayer ||
               currentPlatform == RuntimePlatform.OSXEditor ||
               currentPlatform == RuntimePlatform.LinuxPlayer ||
               currentPlatform == RuntimePlatform.LinuxEditor;

        // Check if WebGL
        isWebGL = currentPlatform == RuntimePlatform.WebGLPlayer;

        // Set input modes
        useTouchControls = isMobile || isWebGL;
        useMouseControls = isPC || isWebGL || (!isMobile && Application.isEditor);
        useKeyboardControls = isPC || isWebGL || (!isMobile && Application.isEditor);

        Debug.Log($"Platform detected: {currentPlatform}");
        Debug.Log($"Is Mobile: {isMobile}, Is PC: {isPC}, Is WebGL: {isWebGL}");
        Debug.Log($"Controls: Touch={useTouchControls}, Mouse={useMouseControls}, Keyboard={useKeyboardControls}");
    }

    private void ApplyPlatformSettings()
    {
        // Set target framerate
        if (isMobile || isWebGL)
        {
            Application.targetFrameRate = targetFramerateMobile;
        }
        else
        {
            Application.targetFrameRate = targetFrameratePC;
        }

        // Quality settings
        if (isMobile)
        {
            QualitySettings.vSyncCount = 0;
            QualitySettings.shadowCascades = 0;
            QualitySettings.antiAliasing = 0;
            QualitySettings.pixelLightCount = 0;
            QualitySettings.resolutionScalingFixedDPIFactor = 0.8f;
        }
        else
        {
            QualitySettings.vSyncCount = 1;
            QualitySettings.shadowCascades = 2;
            QualitySettings.antiAliasing = 2;
        }
    }

    public bool UseTouchControls()
    {
        return useTouchControls;
    }

    public bool UseMouseControls()
    {
        return useMouseControls;
    }

    public bool UseKeyboardControls()
    {
        return useKeyboardControls;
    }

    public bool IsMobile()
    {
        return isMobile;
    }

    public bool IsPC()
    {
        return isPC;
    }
}