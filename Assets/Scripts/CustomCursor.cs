using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomCursor : MonoBehaviour
{
    [Header("Cursor Settings")]
    public Texture2D cursorTexture;
    public Vector2 cursorHotspot = Vector2.zero;

    [Header("Optional")]
    public bool hideSystemCursor = false;

    private static CustomCursor instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            ApplyCursor();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            ApplyCursor();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyCursor();
    }

    void ApplyCursor()
    {
        if (cursorTexture != null)
        {
            // ForceSoftware = stable cursor that doesn't disappear
            Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.ForceSoftware);
        }

        Cursor.visible = !hideSystemCursor;
    }

    public void SetCursor(Texture2D newCursor, Vector2 hotspot)
    {
        cursorTexture = newCursor;
        cursorHotspot = hotspot;
        ApplyCursor();
    }

    public void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.visible = true;
    }
}