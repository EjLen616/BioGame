using UnityEngine;

public class ScreenOrientationManager : MonoBehaviour
{
    [Header("Orientation Settings")]
    public bool allowPortrait = true;
    public bool allowPortraitUpsideDown = false;
    public bool allowLandscapeLeft = false;
    public bool allowLandscapeRight = false;

    [Header("Current Orientation")]
    public ScreenOrientation currentOrientation;

    private ScreenOrientation lastOrientation;

    void Start()
    {
        // Set initial orientation
        SetAllowedOrientations();

        // Get current orientation
        currentOrientation = Screen.orientation;
        lastOrientation = currentOrientation;
    }

    void Update()
    {
        // Check if orientation changed
        currentOrientation = Screen.orientation;

        if (currentOrientation != lastOrientation)
        {
            OnOrientationChanged(currentOrientation);
            lastOrientation = currentOrientation;
        }
    }

    void SetAllowedOrientations()
    {
        Screen.orientation = ScreenOrientation.AutoRotation;

        // Enable allowed orientations
        Screen.autorotateToPortrait = allowPortrait;
        Screen.autorotateToPortraitUpsideDown = allowPortraitUpsideDown;
        Screen.autorotateToLandscapeLeft = allowLandscapeLeft;
        Screen.autorotateToLandscapeRight = allowLandscapeRight;
    }

    void OnOrientationChanged(ScreenOrientation newOrientation)
    {
        Debug.Log($"Orientation changed to: {newOrientation}");

        // Notify other systems
        MobileInputHelper inputHelper = FindObjectOfType<MobileInputHelper>();
        if (inputHelper != null)
        {
            inputHelper.OnOrientationChanged(newOrientation);
        }

        // Adjust camera for new orientation if needed
        AdjustCameraForOrientation(newOrientation);
    }

    void AdjustCameraForOrientation(ScreenOrientation orientation)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Adjust camera viewport or orthographic size for different orientations
        if (orientation == ScreenOrientation.Portrait || orientation == ScreenOrientation.PortraitUpsideDown)
        {
            // Portrait mode - adjust camera for vertical gameplay
            mainCamera.orthographicSize = 5f;
        }
        else if (orientation == ScreenOrientation.LandscapeLeft || orientation == ScreenOrientation.LandscapeRight)
        {
            // Landscape mode - adjust camera for horizontal gameplay
            mainCamera.orthographicSize = 3.5f;
        }
    }
}