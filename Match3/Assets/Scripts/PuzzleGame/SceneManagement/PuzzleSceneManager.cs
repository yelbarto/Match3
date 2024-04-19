using System;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace PuzzleGame.SceneManagement
{
    public class PuzzleSceneManager : SceneManager
    { 
        public static SceneState CurrentSceneState { get; private set; } = SceneState.MainScene;
        public static event Action OnSceneChanged;

        private static bool _isLevelSceneLoaded;

        public static async UniTask ChangeSceneAsync()
        {
            switch (CurrentSceneState)
            {
                case SceneState.MainScene:
                    
                    await LoadSceneAsync(1);
                    CurrentSceneState = SceneState.LevelScene;
                    break;
                case SceneState.LevelScene:
                    await LoadSceneAsync(0);
                    CurrentSceneState = SceneState.MainScene;
                    break;
            }
            OnSceneChanged?.Invoke();
        }

        public static void ChangeScene()
        {
            switch (CurrentSceneState)
            {
                case SceneState.MainScene:
                    
                    LoadScene(1);
                    CurrentSceneState = SceneState.LevelScene;
                    break;
                case SceneState.LevelScene:
                    LoadScene(0);
                    CurrentSceneState = SceneState.MainScene;
                    break;
            }
            OnSceneChanged?.Invoke();
        }
    }
}