using System;
using Cysharp.Threading.Tasks;
using PuzzleGame.Player.Abstractions;
using PuzzleGame.SceneManagement;

namespace PuzzleGame.MainScene.Presenters
{
    public class MainScenePresenter : IDisposable
    {
        private readonly MainSceneView _mainSceneView;
        private readonly IPlayerRepositoryService _playerRepositoryService;

        public MainScenePresenter(MainSceneView mainSceneView, IPlayerRepositoryService playerRepositoryService)
        {
            _playerRepositoryService = playerRepositoryService;
            _mainSceneView = mainSceneView;
        }

        public void Initialize()
        {
            _mainSceneView.OpenLevelDelegate += OnOpenLevelCalled;
            _mainSceneView.SetUp(_playerRepositoryService.CurrentLevel);
        }

        private void OnOpenLevelCalled()
        {
            PuzzleSceneManager.ChangeSceneAsync().Forget();
        }

        public void Dispose()
        {
            _mainSceneView.OpenLevelDelegate -= OnOpenLevelCalled;
        }
    }
}