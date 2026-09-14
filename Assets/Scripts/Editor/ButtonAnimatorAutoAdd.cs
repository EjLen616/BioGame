using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ButtonAnimatorAutoAdd : EditorWindow
{
    [MenuItem("Tools/Add Button Animator To All Buttons In Scene")]
    public static void AddToAllButtons()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        int count = 0;

        foreach (Button button in buttons)
        {
            if (button.GetComponent<ButtonAnimator>() == null)
            {
                Undo.AddComponent<ButtonAnimator>(button.gameObject);
                count++;
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"Added ButtonAnimator to {count} buttons!");
    }
}