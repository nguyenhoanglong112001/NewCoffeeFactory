#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [MenuItem("ChangeScene/LoaderScene #1")]
    public static void Open_Loading()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/LoaderScene.unity");
        }
    }
    [MenuItem("ChangeScene/MainMenu #2")]
    public static void OpenMain()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/MainMenu.unity");
        }
    }
    [MenuItem("ChangeScene/MainGamePlay #3")]
    public static void OpenLevelDesign()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/MainGamePlay.unity");
        }
    }
    
}
#endif