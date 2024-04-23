using System;
using System.Collections.Generic;
using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Util;
using PuzzleGame.Util.Pool;
using UnityEngine;

namespace PuzzleGame.Gameplay.Helpers
{
    public class GridParticleProvider : MonoBehaviour
    {
        [SerializeField] private List<BreakableParticleData> gridParticleDataList;
        [SerializeField] private Transform particleParent;
        [SerializeField] private RandomizedParticleSystem specialItemCreationParticle;
        [SerializeField] private int specialItemCreationParticleWarmUpAmount = 2;

        public static GridParticleProvider Instance { get; private set; }

        private readonly Dictionary<GridType, Dictionary<GridColor, GameObjectPool<RandomizedParticleSystem>>>
            _particlePoolDictionary = new();

        private GameObjectPool<RandomizedParticleSystem> _specialItemCreationParticlePool;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            // Otherwise, set this instance as the singleton instance.
            Instance = this;

            foreach (var particleData in gridParticleDataList)
            {
                var pool = new GameObjectPool<RandomizedParticleSystem>(particleData.ParticleSystem, particleParent);
                pool.LoadPrefab(particleData.WarmUpAmount);
                if (!_particlePoolDictionary.ContainsKey(particleData.GridType))
                    _particlePoolDictionary.Add(particleData.GridType,
                        new Dictionary<GridColor, GameObjectPool<RandomizedParticleSystem>>());
                _particlePoolDictionary[particleData.GridType].Add(particleData.GridColor, pool);
            }
            _specialItemCreationParticlePool = new GameObjectPool<RandomizedParticleSystem>(specialItemCreationParticle,
                particleParent);
            _specialItemCreationParticlePool.LoadPrefab(specialItemCreationParticleWarmUpAmount);
        }

        public RandomizedParticleSystem GetCreationParticleSystem()
        {
            return _specialItemCreationParticlePool.Get(particleParent);
        }
        
        public void ReturnCreationParticleSystem(RandomizedParticleSystem randomizedParticleSystem)
        {
            _specialItemCreationParticlePool.Return(randomizedParticleSystem);
        }

        public RandomizedParticleSystem GetRandomizedParticleSystem(GridType gridType, GridColor gridColor)
        {
            if (_particlePoolDictionary.TryGetValue(gridType, out var colorDictionary))
            {
                if (colorDictionary.TryGetValue(gridColor, out var pool))
                {
                    return pool.Get(particleParent);
                }
            }

            Debug.LogError(
                "Can't find particle system for the given grid type and color. " + gridType + " " + gridColor);
            return null;
        }

        public void ReturnRandomizedParticleSystem(GridType gridType, GridColor gridColor,
            RandomizedParticleSystem randomizedParticleSystem)
        {
            if (_particlePoolDictionary.TryGetValue(gridType, out var colorDictionary))
            {
                if (colorDictionary.TryGetValue(gridColor, out var pool))
                {
                    pool.Return(randomizedParticleSystem);
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }

    [Serializable]
    public struct BreakableParticleData
    {
        [SerializeField] private GridType gridType;
        [SerializeField] private GridColor gridColor;
        [SerializeField] private RandomizedParticleSystem particleSystem;
        [SerializeField] private int warmUpAmount;

        public GridType GridType => gridType;
        public GridColor GridColor => gridColor;
        public RandomizedParticleSystem ParticleSystem => particleSystem;
        public int WarmUpAmount => warmUpAmount;
    }
}