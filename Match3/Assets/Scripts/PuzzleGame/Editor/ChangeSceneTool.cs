using UnityEditor;
using UnityEditor.SceneManagement;

namespace PuzzleGame.Editor
{
    public static class ChangeScenesTool
    {
        private const string MENU_NAME = "Tools/Change Scene/";
        
        [MenuItem(MENU_NAME + "Main Scene")]
        static void OpenMainScene()
        {
            OpenScene("MainScene");
        }
        
        [MenuItem(MENU_NAME + "Level Scene")]
        static void OpenLevelScene()
        {
            OpenScene("LevelScene");
        }
        
        private static void OpenScene(string sceneName)
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }
            EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity");
        }

    }
}