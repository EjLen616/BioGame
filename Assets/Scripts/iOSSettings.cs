using UnityEngine;
using UnityEngine.iOS;

public class iOSSettings : MonoBehaviour
{
    [Header("iOS Settings")]
    public bool enableHapticFeedback = true;
    public bool preventScreenSleep = true;

    void Start()
    {
#if UNITY_IOS && !UNITY_EDITOR
        SetupiOS();
#endif
    }

    void SetupiOS()
    {
        // Prevent screen from sleeping during gameplay
        if (preventScreenSleep)
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        // Set preferred orientation
        Screen.orientation = ScreenOrientation.Portrait;

        // Enable haptic feedback for touch interactions
        if (enableHapticFeedback)
        {
            // Note: iOS haptic feedback requires specific implementation
            // Will be handled by iOS native plugin or Unity's built-in
        }

        Debug.Log("iOS settings applied");
    }

    // Call this method to trigger haptic feedback
    public static void TriggerHapticFeedback()
    {
#if UNITY_IOS && !UNITY_EDITOR
        // Light impact feedback
        // This requires the iOS Native plugin or Unity's built-in haptic
        // For now, we'll use a simple approach
        Handheld.Vibrate(); // Only works on iOS with proper setup
#endif
    }
}