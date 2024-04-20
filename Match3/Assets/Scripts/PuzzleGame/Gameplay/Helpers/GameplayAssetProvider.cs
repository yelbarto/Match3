using System;
using System.Collections.Generic;
using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Util;
using PuzzleGame.Util.Pool;
using UnityEngine;
using UnityEngine.Serialization;

namespace PuzzleGame.Gameplay.Context
{
    [DefaultExecutionOrder(-1000)]
    public class GameplayAssetProvider : MonoBehaviour
    {
        [SerializeField] private List<GridNonCubeAssetData> gridNonCubeAssetDataList;
        [SerializeField] private List<GridCubeAssetData> gridCubeAssetDataList;
        
        public static GameplayAssetProvider Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            // Otherwise, set this instance as the singleton instance.
            Instance = this;
        }
        
        public Sprite GetGridSprite(GridType gridType)
        {
            foreach (var gridAssetData in gridNonCubeAssetDataList)
            {
                if (gridAssetData.GridType == gridType)
                {
                    return gridAssetData.GridDefaultSprite[0];
                }
            }

            Debug.LogError("Can't find grid sprite for the given grid type. " + gridType);
            return null;
        }
        
        public Sprite GetGridSprite(GridType gridType, int health)
        {
            foreach (var gridAssetData in gridNonCubeAssetDataList)
            {
                if (gridAssetData.GridType == gridType)
                {
                    return gridAssetData.GridDefaultSprite[health];
                }
            }

            Debug.LogError("Can't find grid sprite for the given grid type. " + gridType);
            return null;
        }

        public GameObject GetGridPrefab(GridType gridType)
        {
            foreach (var gridAssetData in gridNonCubeAssetDataList)
            {
                if (gridAssetData.GridType == gridType)
                {
                    return gridAssetData.GridPrefab;
                }
            }

            Debug.LogError("Can't find grid prefab for the given grid type. " + gridType);
            return null;
        }
        
        public Sprite GetGridSprite(GridColor gridColor, GridState gridState)
        {
            foreach (var gridAssetData in gridCubeAssetDataList)
            {
                if (gridAssetData.GridColor == gridColor)
                {
                    switch (gridState)
                    {
                        case GridState.NonInteractable:
                        case GridState.Interactable:
                            return gridAssetData.CubeDefaultSprite;
                        case GridState.Rocket:
                            return gridAssetData.CubeRocketSprite;
                        case GridState.Tnt:
                            return gridAssetData.CubeTntSprite;
                    }
                }
            }

            Debug.LogError("Can't find grid sprite for the given grid state and color. " + gridColor + " " + gridState);
            return null;
        }
    }

    [Serializable]
    public struct GridNonCubeAssetData
    {
        [SerializeField] private GridType gridType;
        [SerializeField] private Sprite[] gridDefaultSprite;
        [SerializeField] private GameObject gridPrefab;
        
        public GridType GridType => gridType;
        public Sprite[] GridDefaultSprite => gridDefaultSprite;
        public GameObject GridPrefab => gridPrefab;
    }

    [Serializable]
    public struct GridCubeAssetData
    {
        [SerializeField] private GridColor gridColor;
        [SerializeField] private Sprite cubeDefaultSprite;
        [SerializeField] private Sprite cubeRocketSprite;
        [SerializeField] private Sprite cubeTntSprite;
        
        public GridColor GridColor => gridColor;
        public Sprite CubeDefaultSprite => cubeDefaultSprite;
        public Sprite CubeRocketSprite => cubeRocketSprite;
        public Sprite CubeTntSprite => cubeTntSprite;
    }
}