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
        public event Action<List<GridModel>, bool, bool> OnNewGridsCreated;
        public int MoveCount { get; private set; }
        public Vector2Int BoardSize => _boardSize;

        private readonly TntModelStrategy _tntModelStrategy;
        private readonly RocketModelStrategy _horizontalRocketModelStrategy;
        private readonly RocketModelStrategy _verticalRocketModelStrategy;
        private readonly ObstacleBreakableModelStrategy _onlyBySpecialItemBreakableModelStrategy;
        private readonly ObstacleBreakableModelStrategy _obstacleBreakableModelStrategy;
        private readonly PlayableBreakableModelStrategy _playableBreakableModelStrategy;

        private readonly Dictionary<Vector2Int, int> _destroyedGrids = new();
        private readonly Queue<int> _activeMatchingQueue = new();

        private bool _matchingActive;
        private int _matchCount;
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
                {
                    cubeModel.SetState(null, false);
                    cubeModel.SetDefaultState();
                }
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
                if (GetAdjacentSpecialGrid(x + 1, y, ref gridModel)) 
                    return gridModel;
            }

            if (y + 1 < _boardSize.y)
            {
                if (GetAdjacentSpecialGrid(x, y + 1, ref gridModel)) 
                    return gridModel;
            }

            if (x > 0)
            {
                if (GetAdjacentSpecialGrid(x - 1, y, ref gridModel)) 
                    return gridModel;
            }

            if (y > 0)
            {
                if (GetAdjacentSpecialGrid(x, y - 1, ref gridModel)) 
                    return gridModel;
            }

            return gridModel;
        }

        private bool GetAdjacentSpecialGrid(int x, int y, ref SpecialItemModel gridModel)
        {
            if (_gridData[x, y] is SpecialItemModel nextGrid)
            {
                gridModel = nextGrid;
                if (nextGrid.GridType is GridType.Tnt)
                {
                    return true;
                }
            }

            return false;
        }

        private (HashSet<CubeModel> adjacentCubes, HashSet<ObstacleModel> adjacentObstacles) GetAdjacentGrids(int x, int y,
            GridColor color, HashSet<CubeModel> cubeModels, HashSet<ObstacleModel> adjacentObstacles)
        {
            if (x > 0)
            {
                cubeModels = GetAdjacentSameColoredCubesAndObstacles(x - 1, y, color, cubeModels, ref adjacentObstacles);
            }

            if (y > 0)
            {
                cubeModels = GetAdjacentSameColoredCubesAndObstacles(x, y - 1, color, cubeModels, ref adjacentObstacles);
            }

            if (x < _boardSize.x - 1)
            {
                cubeModels = GetAdjacentSameColoredCubesAndObstacles(x + 1, y, color, cubeModels, ref adjacentObstacles);
            }

            if (y < _boardSize.y - 1)
            {
                cubeModels = GetAdjacentSameColoredCubesAndObstacles(x, y + 1, color, cubeModels, ref adjacentObstacles);
            }

            return (cubeModels, adjacentObstacles);
        }

        private HashSet<CubeModel> GetAdjacentSameColoredCubesAndObstacles(int x, int y, GridColor color, HashSet<CubeModel> cubeModels,
            ref HashSet<ObstacleModel> adjacentObstacles)
        {
            var nextGrid = _gridData[x, y];
            if (nextGrid is CubeModel cubeModel && cubeModel.IsSameColor(color))
            {
                var isNewCube = cubeModels.Add(cubeModel);
                if (isNewCube)
                {
                    (cubeModels, adjacentObstacles) = GetAdjacentGrids(x, y, color, cubeModels, adjacentObstacles);
                    return cubeModels;
                }
            }
            if (nextGrid is ObstacleModel obstacleModel)
            {
                adjacentObstacles.Add(obstacleModel);
            }

            return cubeModels;
        }

        public void SpendMove(int spentMoveCount)
        {
            MoveCount -= spentMoveCount;
        }

        public bool IsLevelFailed()
        {
            return MoveCount == 0;
        }

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
            if (gridModel.IsSpecialItem)
            {
                var specialItem = (SpecialItemModel)gridModel;
                if (specialItem.IsUsed) return brokenObstacles;
                var adjacentGrid = GetAdjacentStrategy(gridModel.Position);
                var effectedGrids = specialItem.UseSpecialItem(adjacentGrid?.UseAsCombinedSpecialItem());
                foreach (var gridPositionPair in effectedGrids)
                {
                    var gridPosition = gridPositionPair.Key;
                    var effectedGrid = _gridData[gridPosition.x, gridPosition.y];
                    if (effectedGrid is null) continue;
                    var otherGridType = GridType.Default;
                    if (effectedGrid == gridModel && adjacentGrid != null)
                        otherGridType = adjacentGrid.GridType;
                    effectedGrid.ExplodeOffset = gridPositionPair.Value + specialItem.ExplodeOffset;
                    if (!effectedGrid.MatchEffectedGrid(true, otherGridType)) continue;
                    _gridData[effectedGrid.Position.x, effectedGrid.Position.y] = null;
                    _destroyedGrids.Add(gridPosition, effectedGrid.ExplodeOffset);
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
                var cubeList = new HashSet<CubeModel> { cubeModel };
                var obstacleList = new HashSet<ObstacleModel>();
                (cubeList, obstacleList) = GetAdjacentGrids(cubeModel.Position.x, cubeModel.Position.y, cubeModel.Color,
                    cubeList,
                    obstacleList);
                foreach (var cube in cubeList)
                {
                    cube.MatchEffectedGrid(false, GridType.Default);
                    _gridData[cube.Position.x, cube.Position.y] = null;
                    _destroyedGrids.Add(cube.Position, cube.ExplodeOffset);
                }

                foreach (var obstacleModel in obstacleList)
                {
                    var isDestroyed = obstacleModel.MatchEffectedGrid(false, GridType.Default);
                    if (!isDestroyed) continue;
                    _destroyedGrids.Add(obstacleModel.Position, obstacleModel.ExplodeOffset);
                    _gridData[obstacleModel.Position.x, obstacleModel.Position.y] = null;
                    if (!brokenObstacles.TryAdd(obstacleModel.GridType, 1))
                    {
                        brokenObstacles[obstacleModel.GridType]++;
                    }
                }

                var createdSpecialItem = CreateSpecialItem(gridModel);
                if (createdSpecialItem != null)
                    OnNewGridsCreated?.Invoke(new List<GridModel> { createdSpecialItem }, true, true);
            }

            if (_activeMatchingQueue.Count == 0)
            {
                DropAndCreateGrids();
                _destroyedGrids.Clear();
            }

            _matchingActive = false;
            return brokenObstacles;
        }

        private void DropAndCreateGrids()
        {
            var newGrids = new List<GridModel>();
            var dropGridDictionary = new Dictionary<GridModel, Vector2Int>();
            for (var x = 0; x < _boardSize.x; x++)
            {
                var dropHeightOffset = 0;
                var currentColumnDestroyedTiles = _destroyedGrids
                    .Where(g => g.Key.x == x)
                    .OrderByDescending(g => g.Key.y)
                    .ToDictionary(g => g.Key, g => g.Value);
                for (var y = 0; y < _boardSize.y; y++)
                {
                    if (_gridData[x, y] != null)
                    {
                        dropHeightOffset = 0;
                        continue;
                    }

                    var skipEmptySpace = false;
                    for (var newY = y + 1; newY < _boardSize.y; newY++)
                    {
                        if (_gridData[x, newY] == null) continue;
                        DropGrid(x, newY, currentColumnDestroyedTiles, dropGridDictionary,
                            ref y, ref dropHeightOffset);
                        skipEmptySpace = true;
                        break;
                    }

                    if (skipEmptySpace) continue;
                    dropHeightOffset = CreateRandomGridModel(currentColumnDestroyedTiles, x, y, dropHeightOffset, 
                        dropGridDictionary, newGrids);
                }
            }

            CalculateGridsAfterMatch();
            OnNewGridsCreated?.Invoke(newGrids, false, true);
            foreach (var gridPair in dropGridDictionary)
            {
                gridPair.Key.DropGrid(gridPair.Value);
            }
        }

        private void DropGrid(int x, int newY, Dictionary<Vector2Int, int> currentColumnDestroyedTiles,
            Dictionary<GridModel, Vector2Int> dropGridDictionary, ref int y,
            ref int dropHeightOffset)
        {
            var dropTile = _gridData[x, newY];
            if (!dropTile.CanFall())
            {
                y = newY;
                return;
            }

            _gridData[x, y] = dropTile;
            dropTile.BelowGridExplodeOffset = currentColumnDestroyedTiles
                .First(v => v.Key.y < newY).Value;
            dropTile.IsMoving = true;
            dropTile.DropHeightOffset = dropHeightOffset;
            dropHeightOffset++;
            dropGridDictionary.Add(dropTile, new Vector2Int(x, y));
            _gridData[x, newY] = null;
        }

        private int CreateRandomGridModel(Dictionary<Vector2Int, int> currentColumnDestroyedTiles, int x, int y,
            int dropHeightOffset, Dictionary<GridModel, Vector2Int> dropGridDictionary, List<GridModel> newGrids)
        {
            var gridModel = CreateGridModel("rand");
            var maxOffset = currentColumnDestroyedTiles.Values.Max();
            var topOffset = currentColumnDestroyedTiles.First().Value;
            var belowGridExplodeOffset = maxOffset - topOffset > currentColumnDestroyedTiles.Count
                ? maxOffset
                : topOffset;
            gridModel.BelowGridExplodeOffset = belowGridExplodeOffset;

            gridModel.SetPosition(new Vector2Int(x, y));

            gridModel.DropHeightOffset = dropHeightOffset;
            dropHeightOffset++;
            _gridData[x, y] = gridModel;
            gridModel.IsMoving = true;
            dropGridDictionary.Add(gridModel, new Vector2Int(x, y));
            newGrids.Add(gridModel);
            return dropHeightOffset;
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
                    if (cubeModel?.State is not GridState.Default) continue;
                    var cubeList = new HashSet<CubeModel> { cubeModel };
                    cubeList = GetSameColoredAdjacentCubes(x, y, cubeModel.Color, cubeList);
                    if (cubeList.Count > 1)
                    {
                        foreach (var selectedGrids in cubeList)
                        {
                            selectedGrids.SetState(cubeList, true);
                        }
                    }
                    else
                    {
                        ((CubeModel)gridModel).SetState(null, true);
                    }
                }
            }
        }

        private HashSet<CubeModel> GetSameColoredAdjacentCubes(int x, int y, GridColor color,
            HashSet<CubeModel> interactableGrids)
        {
            if (x + 1 < _boardSize.x)
            {
                interactableGrids = CheckSameColoredAdjacentCubes(x + 1, y, color, interactableGrids);
            }

            if (x > 0)
            {
                interactableGrids = CheckSameColoredAdjacentCubes(x - 1, y, color, interactableGrids);
            }

            if (y + 1 < _boardSize.y)
            {
                interactableGrids = CheckSameColoredAdjacentCubes(x, y + 1, color, interactableGrids);

            }

            if (y > 0)
            {
                interactableGrids = CheckSameColoredAdjacentCubes(x, y - 1, color, interactableGrids);
            }

            return interactableGrids;
        }

        private HashSet<CubeModel> CheckSameColoredAdjacentCubes(int x, int y, GridColor color, HashSet<CubeModel> interactableGrids)
        {
            if (_gridData[x, y] is not CubeModel nextGrid) return interactableGrids;
            if (!nextGrid.IsSameColor(color)) return interactableGrids;
            var isNewGrid = interactableGrids.Add(nextGrid);
            if (isNewGrid)
                interactableGrids = GetSameColoredAdjacentCubes(x, y, color,
                    interactableGrids);

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
                    return new VaseModel(GridType.Vase, _currentGridCount,
                        _obstacleBreakableModelStrategy, 2);
                default:
                    throw new Exception("Invalid grid type! " + gridType);
            }
        }
    }
}