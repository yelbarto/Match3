using System;
using Cysharp.Threading.Tasks;
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
        private Vector2Int _boardBorders;

        public GridPresenter(GameObjectPool<GridView> gridViewPool, ViewPrefabContainer viewPrefabContainer)
        {
            _gridViewPool = gridViewPool;
            _viewPrefabContainer = viewPrefabContainer;
        }

        public void SetUp(GridModel gridModel, bool shouldCreateAtModelLocation, Vector2Int boardBorders)
        {
            _gridType = gridModel.GridType;
            _boardBorders = boardBorders;
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
                position = new Vector2Int(_gridModel.Position.x, _boardBorders.y + _gridModel.CreationHeightOffset +
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
                var cubeViewStrategy = new CubeViewStrategy(cubeModel.Color);
                _gridView.SetUp(_gridType, _gridModel.IsInteractable, position, cubeViewStrategy, offset,
                    cubeModel.Color);
                _gridView.SetSprite(_gridModel.Health, cubeModel.State);
            }
            else if (_gridModel.IsSpecialItem)
            {
                _gridView = _gridViewPool.Get();
                GridViewStrategy strategy;
                switch (_gridType)
                {
                    case GridType.HorizontalRocket:
                    case GridType.VerticalRocket:
                        strategy = new RocketViewStrategy(_gridType);
                        break;
                    case GridType.Tnt:
                        var verticalRocketStrategy = new RocketViewStrategy(GridType.VerticalRocket);
                        var horizontalRocketStrategy = new RocketViewStrategy(GridType.HorizontalRocket);
                        strategy = new TntViewStrategy(_boardBorders, verticalRocketStrategy, horizontalRocketStrategy);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                _gridView.SetUp(_gridType, _gridModel.IsInteractable, position, strategy, offset, GridColor.Default);
                _gridView.SetSprite(_gridModel.Health, GridState.Default);
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
                        strategy = new RocketViewStrategy(_gridType);
                        break;
                    case GridType.Tnt:
                        var verticalRocketStrategy = new RocketViewStrategy(GridType.VerticalRocket);
                        var horizontalRocketStrategy = new RocketViewStrategy(GridType.HorizontalRocket);
                        strategy = new TntViewStrategy(_boardBorders, verticalRocketStrategy, horizontalRocketStrategy);
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

                _gridView.SetUp(_gridType, _gridModel.IsInteractable, position, strategy, offset, GridColor.Default);
                _gridView.SetSprite(_gridModel.Health, GridState.Default);
            }
        }

        private void OnDropGrid()
        {
            _gridView.DropGrid(_gridModel.Position);
        }

        private void OnMatchEffected(GridType otherEffectedType)
        {
            var playCrackAnimation = true;
            if (_gridModel is SpecialItemModel specialItemModel)
            {
                if (!specialItemModel.IsUsed)
                    OnGridMatched?.Invoke(_gridModel, false);
                playCrackAnimation = !specialItemModel.UsedInCombination;
            }

            if (_gridModel.Health == 0)
            {
                if (playCrackAnimation)
                {
                    GridViewStrategy strategy = null;
                    switch (otherEffectedType)
                    {
                        case GridType.Default:
                        case GridType.Cube:
                        case GridType.Vase:
                        case GridType.Stone:
                        case GridType.Box:
                            break;
                        case GridType.VerticalRocket:
                        case GridType.HorizontalRocket:
                            strategy = _gridType is GridType.VerticalRocket
                                ? new RocketViewStrategy(GridType.HorizontalRocket)
                                : new RocketViewStrategy(GridType.VerticalRocket);
                            break;
                        case GridType.Tnt:
                            strategy = new TntViewStrategy(_boardBorders,
                                new RocketViewStrategy(GridType.VerticalRocket),
                                new RocketViewStrategy(GridType.HorizontalRocket));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(otherEffectedType), otherEffectedType, null);
                    }

                    _gridView.CrackGrid(strategy);   
                }
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