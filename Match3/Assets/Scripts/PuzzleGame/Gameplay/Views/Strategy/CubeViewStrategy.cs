using System.Threading;
using Cysharp.Threading.Tasks;
using PuzzleGame.Gameplay.Context;
using PuzzleGame.Gameplay.DataStructures;
using UnityEngine;

namespace PuzzleGame.Gameplay.Views.Strategy
{
    public class CubeViewStrategy : GridViewStrategy
    {

        public CubeViewStrategy(GridColor gridColor) : base(GridType.Cube, gridColor)
        {
        }
        
        public override Sprite GetSprite(int health, GridState gridState)
        {
            return GameplayAssetProvider.Instance.GetGridSprite(GridColor, gridState);
        }

        public override async UniTask PlaySuperBreakAnimation(Transform zeroPoint, Vector3 position,
            GridViewStrategy otherBrokenGrid)
        {
            await PlayBreakAnimation(zeroPoint, position);
        }
    }
}