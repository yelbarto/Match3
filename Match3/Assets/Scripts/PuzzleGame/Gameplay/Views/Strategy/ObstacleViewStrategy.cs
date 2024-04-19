using System;
using PuzzleGame.Gameplay.Context;
using PuzzleGame.Gameplay.DataStructures;
using UnityEngine;

namespace PuzzleGame.Gameplay.Views.Strategy
{
    public class ObstacleViewStrategy : GridViewStrategy
    {
        private readonly GridType _gridType;

        public ObstacleViewStrategy(GridType gridType)
        {
            _gridType = gridType;
        }
        
        public override Sprite GetSprite(int health, GridState state)
        {
            return GameplayAssetProvider.Instance.GetGridSprite(_gridType, health - 1);
        }
    }
}