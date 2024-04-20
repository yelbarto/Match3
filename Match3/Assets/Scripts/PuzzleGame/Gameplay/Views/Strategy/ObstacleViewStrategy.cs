using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PuzzleGame.Gameplay.Context;
using PuzzleGame.Gameplay.DataStructures;
using UnityEngine;

namespace PuzzleGame.Gameplay.Views.Strategy
{
    public class ObstacleViewStrategy : GridViewStrategy
    {
        private readonly GridType _gridType;

        public ObstacleViewStrategy(GridType gridType) : base(gridType, GridColor.Default)
        {
            _gridType = gridType;
        }

        public override Sprite GetSprite(int health, GridState state)
        {
            return GameplayAssetProvider.Instance.GetGridSprite(_gridType, health - 1);
        }

        public override async UniTask PlaySuperBreakAnimation(Transform zeroPointPosition, Vector3 position,
            GridViewStrategy otherBrokenGrid)
        {
            await PlayBreakAnimation(zeroPointPosition, position);
        }
    }
}