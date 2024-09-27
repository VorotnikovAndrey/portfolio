#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEditor.SceneManagement;

public static class ScenesUnits
{
#if UNITY_EDITOR
    [MenuItem("Scenes/▶️ Play", false, 0)]
    private static void PlayGameFromScene()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/Scenes/" + "Main.unity");
            EditorApplication.isPlaying = true;
        }
    }
    
    [MenuItem("Scenes/Main", false, 1)]
    private static void OpenMain()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/Scenes/" + "Main.unity");
        }
    }
    
    [MenuItem("Scenes/Lobby", false, 2)]
    private static void OpenLobby()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/Scenes/" + "Lobby.unity");
        }
    }   
    
    [MenuItem("Scenes/Locations/Location1", false, 3)]
    private static void OpenLocation1()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Locations/" + "Location1.unity");
        }
    }
#endif
}