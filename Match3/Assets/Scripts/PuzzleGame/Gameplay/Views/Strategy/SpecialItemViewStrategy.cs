using System.Threading;
using Cysharp.Threading.Tasks;
using PuzzleGame.Gameplay.Context;
using PuzzleGame.Gameplay.DataStructures;
using UnityEngine;

namespace PuzzleGame.Gameplay.Views.Strategy
{
    public abstract class SpecialItemViewStrategy : GridViewStrategy
    {

        public SpecialItemViewStrategy(GridType gridType) : base(gridType, GridColor.Default)
        {
        }

        public override Sprite GetSprite(int health, GridState state)
        {
            return GameplayAssetProvider.Instance.GetGridSprite(GridType);
        }
    }
}