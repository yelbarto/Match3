using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PuzzleGame.Data.DataStructures;
using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Gameplay.Models.Strategy;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PuzzleGame.Gameplay.Models
{
    public class LevelModel
    {
        public List<GridModel> GridDataList => _gridData.Cast<GridModel>().ToList();
        public event Action<List<GridModel>, bool> OnNewGridsCreated; 
        public int MoveCount { get; private set; }
        public Vector2Int BoardSize => _boardSize;

        private readonly TntModelStrategy _tntModelStrategy;
        private readonly RocketModelStrategy _horizontalRocketModelStrategy;
        private readonly RocketModelStrategy _verticalRocketModelStrategy;
        private readonly ObstacleBreakableModelStrategy _onlyBySpecialItemBreakableModelStrategy;
        private readonly ObstacleBreakableModelStrategy _obstacleBreakableModelStrategy;
        private readonly PlayableBreakableModelStrategy _playableBreakableModelStrategy;

        private GridModel[,] _gridData;
        private Vector2Int _boardSize;
        private int _currentLevel;
        private int _currentGridCount;

        public LevelModel()
        {
            _horizontalRocketModelStrategy = new RocketModelStrategy(Direction.Horizontal);
            _verticalRocketModelStrategy = new RocketModelStrategy(Direction.Vertical);
            _tntModelStrategy = new TntModelStrategy(_verticalRocketModelStrategy, _horizontalRocketModelStrategy);
            _onlyBySpecialItemBreakableModelStrategy = new ObstacleBreakableModelStrategy(true);
            _obstacleBreakableModelStrategy = new ObstacleBreakableModelStrategy(false);
            _playableBreakableModelStrategy = new PlayableBreakableModelStrategy();
        }

        public void Initialize(LevelData levelData)
        {
            CleanUpGrids();
            _boardSize = new Vector2Int(levelData.GridWidth, levelData.GridHeight);
            _gridData = new GridModel[_boardSize.x, _boardSize.y];
            _currentLevel = levelData.LevelNumber;
            MoveCount = levelData.MoveCount;
            var gridList = levelData.GridData;
            InitializeStrategies();
            CreateGridData(gridList);
        }

        private void InitializeStrategies()
        {
            _horizontalRocketModelStrategy.SetBorders(_boardSize);
            _verticalRocketModelStrategy.SetBorders(_boardSize);
            _tntModelStrategy.SetBorders(_boardSize);
        }

        private void CleanUpGrids()
        {
            if (_gridData == null) return;
            foreach (var gridModel in _gridData)
            {
                if (gridModel == null) continue;
                gridModel.Dispose();
            }
        }

        private void CreateGridData(string[] gridList)
        {
            for (var y = 0; y < _boardSize.y; y++)
            {
                for (var x = 0; x < _boardSize.x; x++)
                {
                    var gridData = gridList[x + _boardSize.x * y];
                    var gridModel = CreateGridModel(gridData);
                    gridModel.SetPosition(new Vector2Int(x, y));
                    _gridData[x, y] = gridModel;
                }
            }

            CalculateInteractableGrids();
        }

        public Dictionary<GridType, int> GetGoals()
        {
            var goals = new Dictionary<GridType, int>();
            foreach (var gridModel in _gridData)
            {
                if (gridModel == null) continue;
                if (!gridModel.IsObstacle) continue;
                if (!goals.TryAdd(gridModel.GridType, 1))
                {
                    goals[gridModel.GridType]++;
                }
            }

            return goals;
        }

        public void CalculateGridsAfterMatch()
        {
            foreach (var model in _gridData)
            {
                if (model is CubeModel cubeModel)
                    cubeModel.SetState(GridState.Default, false);
            }

            CalculateInteractableGrids();
        }

        private SpecialItemModel GetAdjacentStrategy(Vector2Int position)
        {
            var x = position.x;
            var y = position.y;
            SpecialItemModel gridModel = null;
            if (x + 1 < _boardSize.x)
            {
                if (_gridData[x + 1, y] is SpecialItemModel nextGrid)
                {
                    gridModel = nextGrid;
                    if (nextGrid.GridType is GridType.Tnt)
                        return nextGrid;
                }
            }

            if (y + 1 < _boardSize.y)
            {
                if (_gridData[x, y + 1] is SpecialItemModel nextGrid)
                {
                    gridModel = nextGrid;
                    if (nextGrid.GridType is GridType.Tnt)
                        return nextGrid;
                }
            }

            if (x > 0)
            {
                if (_gridData[x - 1, y] is SpecialItemModel nextGrid)
                {
                    gridModel = nextGrid;
                    if (nextGrid.GridType is GridType.Tnt)
                        return nextGrid;
                }
            }

            if (y > 0)
            {
                if (_gridData[x, y - 1] is SpecialItemModel nextGrid)
                {
                    gridModel = nextGrid;
                    if (nextGrid.GridType is GridType.Tnt)
                        return nextGrid;
                }
            }

            return gridModel;
        }

        private (List<CubeModel> adjacentCubes, List<ObstacleModel> adjacentObstacles) GetAdjacentGrids(int x, int y,
            GridColor color, List<CubeModel> cubeModels, List<ObstacleModel> adjacentObstacles)
        {
            if (x > 0)
            {
                var nextGrid = _gridData[x - 1, y];
                if (nextGrid is CubeModel cubeModel && cubeModel.IsSameColor(color)
                                                    && cubeModels.All(c => c.Id != cubeModel.Id))
                {
                    cubeModels.Add(cubeModel);
                    (cubeModels, adjacentObstacles) = GetAdjacentGrids(x - 1, y, color, cubeModels, adjacentObstacles);
                }
                else if (nextGrid is ObstacleModel obstacleModel
                         && adjacentObstacles.All(o => o.Id != nextGrid.Id))
                {
                    adjacentObstacles.Add(obstacleModel);
                }
            }

            if (y > 0)
            {
                var nextGrid = _gridData[x, y - 1];
                if (nextGrid is CubeModel cubeModel && cubeModel.IsSameColor(color)
                                                    && cubeModels.All(c => c.Id != cubeModel.Id))
                {
                    cubeModels.Add(cubeModel);
                    (cubeModels, adjacentObstacles) = GetAdjacentGrids(x, y - 1, color, cubeModels, adjacentObstacles);
                }
                else if (nextGrid is ObstacleModel obstacleModel
                         && adjacentObstacles.All(o => o.Id != nextGrid.Id))
                {
                    adjacentObstacles.Add(obstacleModel);
                }
            }

            if (x < _boardSize.x - 1)
            {
                var nextGrid = _gridData[x + 1, y];
                if (nextGrid is CubeModel cubeModel && cubeModel.IsSameColor(color)
                                                    && cubeModels.All(c => c.Id != cubeModel.Id))
                {
                    cubeModels.Add(cubeModel);
                    (cubeModels, adjacentObstacles) = GetAdjacentGrids(x + 1, y, color, cubeModels, adjacentObstacles);
                }
                else if (nextGrid is ObstacleModel obstacleModel
                         && adjacentObstacles.All(o => o.Id != nextGrid.Id))
                {
                    adjacentObstacles.Add(obstacleModel);
                }
            }

            if (y < _boardSize.y - 1)
            {
                var nextGrid = _gridData[x, y + 1];
                if (nextGrid is CubeModel cubeModel && cubeModel.IsSameColor(color)
                                                    && cubeModels.All(c => c.Id != cubeModel.Id))
                {
                    cubeModels.Add(cubeModel);
                    (cubeModels, adjacentObstacles) = GetAdjacentGrids(x, y + 1, color, cubeModels, adjacentObstacles);
                }
                else if (nextGrid is ObstacleModel obstacleModel
                         && adjacentObstacles.All(o => o.Id != nextGrid.Id))
                {
                    adjacentObstacles.Add(obstacleModel);
                }
            }

            return (cubeModels, adjacentObstacles);
        }

        public void SpendMove()
        {
            MoveCount--;
        }
        
        public bool IsLevelFailed()
        {
            return MoveCount == 0;
        }

        private readonly Queue<int> _activeMatchingQueue = new();
        private bool _matchingActive;
        private int _matchCount;

        public async UniTask<Dictionary<GridType, int>> MatchGrids(GridModel gridModel)
        {
            var currentMatchCount = _matchCount;
            _matchCount++;
            _activeMatchingQueue.Enqueue(currentMatchCount);
            if (_matchingActive)
            {
                await UniTask.WaitUntil(() =>
                    _matchingActive == false && _activeMatchingQueue.Peek() == currentMatchCount);
            }

            _matchingActive = true;
            _activeMatchingQueue.Dequeue();
            var brokenObstacles = new Dictionary<GridType, int>();
            GridModel createdSpecialItem = null;
            if (gridModel.IsSpecialItem)
            {
                var specialItem = (SpecialItemModel)gridModel;
                if (specialItem.IsUsed) return brokenObstacles;
                var adjacentGrid = GetAdjacentStrategy(gridModel.Position);
                var effectedGrids = specialItem.UseSpecialItem(adjacentGrid?.UseAsCombinedSpecialItem());
                foreach (var gridPosition in effectedGrids)
                {
                    var effectedGrid = _gridData[gridPosition.x, gridPosition.y];
                    if (effectedGrid is null) continue;
                    if (!effectedGrid.MatchEffectedGrid(true)) continue;
                    _gridData[effectedGrid.Position.x, effectedGrid.Position.y] = null;
                    if (!effectedGrid.IsObstacle) continue;
                    if (!brokenObstacles.TryAdd(effectedGrid.GridType, 1))
                    {
                        brokenObstacles[effectedGrid.GridType]++;
                    }
                }
            }
            else
            {
                var cubeModel = (CubeModel)gridModel;
                var cubeList = new List<CubeModel> { cubeModel };
                var obstacleList = new List<ObstacleModel>();
                (cubeList, obstacleList) = GetAdjacentGrids(cubeModel.Position.x, cubeModel.Position.y, cubeModel.Color,
                    cubeList,
                    obstacleList);
                foreach (var cube in cubeList)
                {
                    cube.MatchEffectedGrid(false);
                    _gridData[cube.Position.x, cube.Position.y] = null;
                }

                foreach (var obstacleModel in obstacleList)
                {
                    var isDestroyed = obstacleModel.MatchEffectedGrid(false);
                    if (!isDestroyed) continue;
                    _gridData[obstacleModel.Position.x, obstacleModel.Position.y] = null;
                    if (!brokenObstacles.TryAdd(obstacleModel.GridType, 1))
                    {
                        brokenObstacles[obstacleModel.GridType]++;
                    }
                }
                createdSpecialItem = CreateSpecialItem(gridModel);
                if (createdSpecialItem != null)
                    OnNewGridsCreated?.Invoke(new List<GridModel> {createdSpecialItem}, true);
            }
            
            if (_activeMatchingQueue.Count == 0)
                DropAndCreateGrids(createdSpecialItem);
            _matchingActive = false;
            return brokenObstacles;
        }

        private void DropAndCreateGrids(GridModel createdSpecialItem)
        {
            var newGrids = new List<GridModel>();
            var dropGridDictionary = new Dictionary<GridModel, Vector2Int>();
            for (var x = 0; x < _boardSize.x; x++)
            {
                var creationHeightOffset = 0;
                for (var y = 0; y < _boardSize.y; y++)
                {
                    if (_gridData[x, y] != null) continue;
                    var skipEmptySpace = false;
                    for (var newY = y; newY < _boardSize.y; newY++)
                    {
                        if (_gridData[x, newY] == null) continue;
                        var dropTile = _gridData[x, newY];
                        if (!dropTile.CanFall())
                        {
                            skipEmptySpace = true;
                            y = newY;
                            break;
                        }
                        _gridData[x, y] = dropTile;
                        dropGridDictionary.Add(dropTile, new Vector2Int(x, y));
                        _gridData[x, newY] = null;
                        skipEmptySpace = true;
                        break;
                    }
                    if (skipEmptySpace) continue;
                    var gridModel = CreateGridModel("rand");
                    gridModel.SetPosition(new Vector2Int(x, y));

                    gridModel.CreationHeightOffset = creationHeightOffset;
                    creationHeightOffset++;
                    _gridData[x, y] = gridModel;
                    dropGridDictionary.Add(gridModel, new Vector2Int(x, y));
                    newGrids.Add(gridModel);
                }
            }
            // for (var y = 0; y < _boardSize.y; y++)
            // {
            //     for (var x = 0; x < _boardSize.x; x++)
            //     {
            //         if (_gridData[x, y] != null) continue;
            //         var skipEmptySpace = false;
            //         for (var newY = y; newY < _boardSize.y; newY++)
            //         {
            //             if (_gridData[x, newY] == null) continue;
            //             var dropTile = _gridData[x, newY];
            //             if (!dropTile.CanFall())
            //             {
            //                 skipEmptySpace = true;
            //                 break;
            //             }
            //             _gridData[x, y] = dropTile;
            //             dropGridDictionary.Add(dropTile, new Vector2Int(x, y));
            //             _gridData[x, newY] = null;
            //             skipEmptySpace = true;
            //             break;
            //         }
            //
            //         if (skipEmptySpace) continue;
            //         var gridModel = CreateGridModel("rand");
            //         gridModel.SetPosition(new Vector2Int(x, y));
            //         var offset = 0;
            //         for (var newY = y; newY < _boardSize.y; newY++)
            //         {
            //             offset++;
            //         }
            //
            //         gridModel.CreationHeightOffset = offset;
            //         _gridData[x, y] = gridModel;
            //         dropGridDictionary.Add(gridModel, new Vector2Int(x, y));
            //         newGrids.Add(gridModel);
            //     }
            // }
            CalculateGridsAfterMatch();
            OnNewGridsCreated?.Invoke(newGrids, false);
            createdSpecialItem?.DropGrid(createdSpecialItem.Position);
            foreach (var gridPair in dropGridDictionary)
            {
                gridPair.Key.DropGrid(gridPair.Value);
            }
        }

        private GridModel CreateSpecialItem(GridModel gridModel)
        {
            var state = ((CubeModel)gridModel).State;
            var position = gridModel.Position;
            if (state is GridState.Tnt)
            {
                _currentGridCount++;
                var specialItem = new SpecialItemModel(_tntModelStrategy, GridType.Tnt, _currentGridCount,
                    _playableBreakableModelStrategy);
                specialItem.SetPosition(position);
                _gridData[position.x, position.y] = specialItem;
                return specialItem;
            }

            if (state is GridState.Rocket)
            {
                return CreateRocket(position);
            }

            return null;
        }

        private GridModel CreateRocket(Vector2Int position)
        {
            _currentGridCount++;
            SpecialItemModel specialItem;
            if (Random.Range(0, 2) == 0)
            {
                specialItem = new SpecialItemModel(_horizontalRocketModelStrategy, GridType.HorizontalRocket,
                    _currentGridCount, _playableBreakableModelStrategy);
            }
            else
            {
                specialItem = new SpecialItemModel(_verticalRocketModelStrategy, GridType.VerticalRocket,
                    _currentGridCount, _playableBreakableModelStrategy);
            }

            specialItem.SetPosition(position);
            _gridData[position.x, position.y] = specialItem;
            return specialItem;
        }

        private void CalculateInteractableGrids()
        {
            for (var y = 0; y < _boardSize.y; y++)
            {
                for (var x = 0; x < _boardSize.x; x++)
                {
                    var gridModel = _gridData[x, y];
                    var cubeModel = gridModel as CubeModel;
                    if (cubeModel == null) continue;
                    if (cubeModel.State is not GridState.Default) continue;
                    var gridList = new List<GridModel> { gridModel };
                    gridList = CheckAndSetIfAdjacentGridIsTheSame(x, y, cubeModel.Color, gridList);
                    if (gridList.Count > 1)
                    {
                        var cubeState = gridList.Count >= 5 ? GridState.Tnt :
                            gridList.Count >= 3 ? GridState.Rocket : GridState.Interactable;
                        foreach (var selectedGrids in gridList)
                        {
                            ((CubeModel)selectedGrids).SetState(cubeState, true);
                        }
                    }
                    else
                    {
                        ((CubeModel)gridModel).SetState(GridState.NonInteractable, true);
                    }
                }
            }
        }

        private List<GridModel> CheckAndSetIfAdjacentGridIsTheSame(int x, int y, GridColor color,
            List<GridModel> interactableGrids)
        {
            if (x + 1 < _boardSize.x)
            {
                if (_gridData[x + 1, y] is CubeModel nextGrid)
                {
                    if (nextGrid.IsSameColor(color) && interactableGrids.All(g => g.Id != nextGrid.Id))
                    {
                        interactableGrids.Add(nextGrid);
                        interactableGrids = CheckAndSetIfAdjacentGridIsTheSame(x + 1, y, color,
                            interactableGrids);
                    }
                }
            }

            if (x > 0)
            {
                if (_gridData[x - 1, y] is CubeModel nextGrid)
                {
                    if (nextGrid.IsSameColor(color) && interactableGrids.All(g => g.Id != nextGrid.Id))
                    {
                        interactableGrids.Add(nextGrid);
                        interactableGrids = CheckAndSetIfAdjacentGridIsTheSame(x - 1, y, color,
                            interactableGrids);
                    }
                }
            }

            if (y + 1 < _boardSize.y)
            {
                if (_gridData[x, y + 1] is CubeModel nextGrid)
                {
                    if (nextGrid.IsSameColor(color) && interactableGrids.All(g => g.Id != nextGrid.Id))
                    {
                        interactableGrids.Add(nextGrid);
                        interactableGrids = CheckAndSetIfAdjacentGridIsTheSame(x, y + 1, color,
                            interactableGrids);
                    }
                }
            }

            if (y > 0)
            {
                if (_gridData[x, y - 1] is CubeModel nextGrid)
                {
                    if (nextGrid.IsSameColor(color) && interactableGrids.All(g => g.Id != nextGrid.Id))
                    {
                        interactableGrids.Add(nextGrid);
                        interactableGrids = CheckAndSetIfAdjacentGridIsTheSame(x, y - 1, color,
                            interactableGrids);
                    }
                }
            }

            return interactableGrids;
        }

        private GridModel CreateGridModel(string gridType)
        {
            _currentGridCount++;
            switch (gridType)
            {
                case "r":
                    return new CubeModel(GridColor.Red, _currentGridCount, _playableBreakableModelStrategy);
                case "g":
                    return new CubeModel(GridColor.Green, _currentGridCount, _playableBreakableModelStrategy);
                case "y":
                    return new CubeModel(GridColor.Yellow, _currentGridCount, _playableBreakableModelStrategy);
                case "b":
                    return new CubeModel(GridColor.Blue, _currentGridCount, _playableBreakableModelStrategy);
                case "rand":
                    var random = Random.Range(0, 4);
                    return new CubeModel((GridColor)random, _currentGridCount, _playableBreakableModelStrategy);
                case "t":
                    return new SpecialItemModel(_tntModelStrategy, GridType.Tnt, _currentGridCount,
                        _playableBreakableModelStrategy);
                case "rov":
                    return new SpecialItemModel(_verticalRocketModelStrategy, GridType.VerticalRocket,
                        _currentGridCount, _playableBreakableModelStrategy);
                case "roh":
                    return new SpecialItemModel(_horizontalRocketModelStrategy, GridType.HorizontalRocket,
                        _currentGridCount, _playableBreakableModelStrategy);
                case "bo":
                    return new ObstacleModel(GridType.Box, _currentGridCount, 
                        _obstacleBreakableModelStrategy, 1);
                case "s":
                    return new ObstacleModel(GridType.Stone, _currentGridCount,
                        _onlyBySpecialItemBreakableModelStrategy, 1);
                case "v":
                    return new ObstacleModel(GridType.Vase, _currentGridCount,
                        _obstacleBreakableModelStrategy, 2);
                default:
                    throw new Exception("Invalid grid type! " + gridType);
            }
        }
    }
}