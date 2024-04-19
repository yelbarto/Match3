using PuzzleGame.Gameplay.Context;
using PuzzleGame.Gameplay.DataStructures;
using UnityEngine;

namespace PuzzleGame.Gameplay.Views.Strategy
{
    public class CubeViewStrategy : GridViewStrategy
    {
        private GridColor _gridColor;

        public void SetStrategyState(GridColor gridColor)
        {
            _gridColor = gridColor;
        }
        
        public override Sprite GetSprite(int health, GridState gridState)
        {
            return GameplayAssetProvider.Instance.GetGridSprite(_gridColor, gridState);
        }
    }
}