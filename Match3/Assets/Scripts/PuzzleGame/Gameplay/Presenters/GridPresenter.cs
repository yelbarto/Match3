using System;
using PuzzleGame.Gameplay.Context;
using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Gameplay.Models;
using PuzzleGame.Gameplay.Views;
using PuzzleGame.Gameplay.Views.Strategy;
using PuzzleGame.Util.Pool;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PuzzleGame.Gameplay.Presenters
{
    public class GridPresenter
    {
        public event Action<GridModel, bool> OnGridMatched;
        public event Action<GridPresenter> OnGridDestroyed;

        private readonly GameObjectPool<GridView> _gridViewPool;
        private readonly ViewPrefabContainer _viewPrefabContainer;

        private GridView _gridView;
        private GridType _gridType;
        private GridModel _gridModel;
        private int _boardHeight;

        public GridPresenter(GameObjectPool<GridView> gridViewPool, ViewPrefabContainer viewPrefabContainer)
        {
            _gridViewPool = gridViewPool;
            _viewPrefabContainer = viewPrefabContainer;
        }

        public void SetUp(GridModel gridModel, bool shouldCreateAtModelLocation, int boardHeight)
        {
            _gridType = gridModel.GridType;
            _boardHeight = boardHeight;
            _gridModel = gridModel;
            CreateGridView(shouldCreateAtModelLocation);
            _gridView.OnGridClicked += OnGridClicked;
            _gridModel.OnMatchEffectedGrid += OnMatchEffected;
            _gridModel.OnDropGrid += OnDropGrid;
        }

        private void OnGridClicked()
        {
            OnGridMatched?.Invoke(_gridModel, true);
        }

        private void CreateGridView(bool shouldCreateAtModelLocation)
        {
            Vector2Int position;
            var offset = 0;
            if (!shouldCreateAtModelLocation)
            {
                position = new Vector2Int(_gridModel.Position.x, _boardHeight + _gridModel.CreationHeightOffset + 
                                                                 GameplayVariables.Instance.CreationHeightOffset);
                offset = _gridModel.CreationHeightOffset;
            }
            else
            {
                position = _gridModel.Position;
            }
            if (_gridType is GridType.Cube)
            {
                var cubeView = _gridViewPool.Get();
                var cubeModel = (CubeModel)_gridModel;

                _gridView = cubeView;
                cubeModel.OnStateChanged += OnStateChanged;
                var cubeViewStrategy = new CubeViewStrategy();
                cubeViewStrategy.SetStrategyState(cubeModel.Color);
                _gridView.SetUp(_gridType, _gridModel.IsInteractable, position, cubeViewStrategy, offset);
                _gridView.SetSprite(_gridModel.Health, cubeModel.State);
            }
            else
            {
                _gridView = Object.Instantiate(_viewPrefabContainer.GetGridViewPrefab(_gridType),
                    _gridViewPool.PoolParent);
                GridViewStrategy strategy;
                switch (_gridType)
                {
                    case GridType.HorizontalRocket:
                    case GridType.VerticalRocket:
                    case GridType.Tnt:
                        strategy = new SpecialItemViewStrategy(_gridModel.GridType);
                        break;
                    case GridType.Box:
                        strategy = new ObstacleViewStrategy(_gridModel.GridType);
                        break;
                    case GridType.Stone:
                        strategy = new ObstacleViewStrategy(_gridModel.GridType);
                        break;
                    case GridType.Vase:
                        strategy = new ObstacleViewStrategy(_gridModel.GridType);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _gridView.SetUp(_gridType, _gridModel.IsInteractable, position, strategy, offset);
                _gridView.SetSprite(_gridModel.Health, GridState.Default);
            }
        }

        private void OnDropGrid()
        {
            _gridView.DropGridSpeedBase(_gridModel.Position);
        }

        private void OnMatchEffected()
        {
            if (_gridModel is SpecialItemModel { IsUsed: false })
            {
                OnGridMatched?.Invoke(_gridModel, false);
            }

            if (_gridModel.Health == 0)
            {
                OnGridDestroyed?.Invoke(this);
            }
            else
            {
                ChangeGridSprite();
            }
        }

        public void OnStateChanged()
        {
            ChangeGridSprite();
            _gridView.SetInteractable(_gridModel.IsInteractable);
        }

        private void ChangeGridSprite()
        {
            var state = _gridModel is CubeModel cubeModel ? cubeModel.State : GridState.Default;
            _gridView.SetSprite(_gridModel.Health, state);
        }

        public void Dispose()
        {
            _gridView.OnGridClicked -= OnGridClicked;
            _gridModel.OnMatchEffectedGrid -= OnMatchEffected;
            _gridModel.OnDropGrid -= OnDropGrid;
            if (_gridModel.GridType is GridType.Cube || _gridModel.IsSpecialItem)
                _gridViewPool.Return(_gridView);
            else
                Object.Destroy(_gridView.gameObject);
            _gridView = null;
        }
    }
}