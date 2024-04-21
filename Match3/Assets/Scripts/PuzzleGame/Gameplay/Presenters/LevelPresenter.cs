using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PuzzleGame.Data.Services;
using PuzzleGame.Gameplay.Models;
using PuzzleGame.Gameplay.Views;
using PuzzleGame.Player.Abstractions;
using PuzzleGame.SceneManagement;
using PuzzleGame.Util.UI;
using UnityEngine;

namespace PuzzleGame.Gameplay.Presenters
{
    public class LevelPresenter
    {
        private readonly IPlayerRepositoryService _playerRepositoryService;
        private readonly LevelRepositoryService _levelRepositoryService;
        private readonly GridPresenterFactory _gridPresenterFactory;
        private readonly LevelFailView _levelFailView;
        private readonly LevelView _levelView;
        private readonly DebugView _debugView;

        private readonly CancellationTokenSource _lifetimeCts = new ();
        private readonly List<GridPresenter> _gridPresenters = new();
        private readonly List<int> _activeCrackAnimationIds = new();
        
        private LevelModel _levelModel;

        public LevelPresenter(IPlayerRepositoryService playerRepositoryService, LevelView levelView,
            Transform gridParent, GridView gridPrefab, LevelFailView levelFailView, DebugView debugView)
        {
            _levelRepositoryService = new LevelRepositoryService();
            _gridPresenterFactory = new GridPresenterFactory(gridParent, gridPrefab);
            _playerRepositoryService = playerRepositoryService;
            _levelFailView = levelFailView;
            _levelView = levelView;
            _debugView = debugView;
            SetActions();
        }
        
        private void SetActions()
        {
            _levelView.DebugSetCubeStateAction += DebugSetCubeState;
            _debugView.ChangeLevelAction += ChangeLevel;
            _debugView.RestartLevelAction += ResetLevel;
            _debugView.CompleteLevelAction += OnLevelCompleted;
            _debugView.FailLevelAction += OnLevelFailed;
        }

        private void ChangeLevel(int level)
        {
            if (_playerRepositoryService.ChangeLevel(level))
                ResetLevel();
        }
        
        private void DebugSetCubeState()
        {
            _levelModel.CalculateGridsAfterMatch();
        }

        public void Initialize()
        {
            OpenLevel(true);
        }

        private void OpenLevel(bool isSceneFirstLevel)
        {
            var levelData = _levelRepositoryService.LoadLevel(_playerRepositoryService.CurrentLevel);
            if (isSceneFirstLevel)
            {
                _levelModel = new LevelModel();
                _levelModel.OnNewGridsCreated += CreateGrids;
            }

            _levelModel.Initialize(levelData);
            _levelView.SetUpLevel(_levelModel.MoveCount, _levelModel.GetGoals(), _levelModel.BoardSize);
            CreateInitialGrids();
        }

        private void CreateInitialGrids()
        {
            CreateGrids(_levelModel.GridDataList, true);
        }

        private void CreateGrids(List<GridModel> gridModels, bool shouldCreateAtModelLocation)
        {
            foreach (var gridModel in gridModels)
            {
                CreateGrid(gridModel, shouldCreateAtModelLocation);
            }
        }

        private void CreateGrid(GridModel gridModel, bool shouldCreateAtModelLocation)
        {
            var gridPresenter = _gridPresenterFactory.Create();
            gridPresenter.SetUp(gridModel, shouldCreateAtModelLocation, _levelModel.BoardSize);
            gridPresenter.OnGridMatched += MatchGrid;
            gridPresenter.OnGridDestroyed += OnGridDestroyed;
            gridPresenter.OnCrackAnimationStateChange += OnCrackAnimationStateChanged;
            _gridPresenters.Add(gridPresenter);
        }

        private void OnGridDestroyed(GridPresenter gridPresenter)
        {
            _gridPresenters.Remove(gridPresenter);
            gridPresenter.OnGridDestroyed -= OnGridDestroyed;
            gridPresenter.OnGridMatched -= MatchGrid;
            gridPresenter.OnCrackAnimationStateChange -= OnCrackAnimationStateChanged;
            gridPresenter.Dispose();
        }
        
        private void MatchGrid(GridModel gridModel, bool isMove)
        {
            MatchGridAsync(gridModel, isMove).Forget();
        }
        
        private async UniTask MatchGridAsync(GridModel gridModel, bool isMove)
        {
            if (isMove)
            {
                _levelModel.SpendMove();
                _levelView.OnMoveHappened();
            }
            var brokenObstacles = await _levelModel.MatchGrids(gridModel);
            foreach (var obstacleData in brokenObstacles)
            {
                _levelView.OnGoalFound(obstacleData.Key, obstacleData.Value);
            }
            
            if (_levelModel.GetGoals().Count == 0)
            {
                await WaitForAnimationsAsync();
                OnLevelCompleted();
            }
            else if (_levelModel.IsLevelFailed())
            {
                await WaitForAnimationsAsync();
                OnLevelFailed();
            }
        }

        private async Task WaitForAnimationsAsync()
        {
            InputBlockerComponent.Instance.ChangeInteractable(true);
            await UniTask.WaitUntil(() => _activeCrackAnimationIds.Count == 0,
                cancellationToken: _lifetimeCts.Token);
            InputBlockerComponent.Instance.ChangeInteractable(false);
        }

        private void CleanUp()
        {
            foreach (var gridPresenter in _gridPresenters)
            {
                gridPresenter.OnGridMatched -= MatchGrid;
                gridPresenter.OnCrackAnimationStateChange -= OnCrackAnimationStateChanged;
                gridPresenter.OnGridDestroyed -= OnGridDestroyed;
                gridPresenter.Dispose();
            }
            _activeCrackAnimationIds.Clear();
            _gridPresenters.Clear();
        }

        private void OnCrackAnimationStateChanged(int id, bool state)
        {
            if (state)
                _activeCrackAnimationIds.Add(id);
            else
                _activeCrackAnimationIds.Remove(id);
        }

        private void OnLevelFailed()
        {
            _levelFailView.Open(isRetry =>
            {
                if (isRetry)
                    ResetLevel();
                else
                    CloseScene();
            });
        }

        private void ResetLevel()
        {
            CleanUp();
            OpenLevel(false);
        }

        private void OnLevelCompleted()
        {
            _levelView.OnLevelCompleted(() =>
            {
                _playerRepositoryService.LevelCompleted();
                CloseScene();
            });
        }

        private void CloseScene()
        {
            PuzzleSceneManager.ChangeScene();
        }

        public void Dispose()
        {
            _levelModel.OnNewGridsCreated -= CreateGrids;
            _debugView.ChangeLevelAction -= ChangeLevel;
            _debugView.RestartLevelAction -= ResetLevel;
            _debugView.CompleteLevelAction -= OnLevelCompleted;
            _debugView.FailLevelAction -= OnLevelFailed;
            _lifetimeCts?.Cancel();
        }
    }
}