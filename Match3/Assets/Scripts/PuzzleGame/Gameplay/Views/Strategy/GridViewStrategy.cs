using PuzzleGame.Gameplay.DataStructures;
using UnityEngine;

namespace PuzzleGame.Gameplay.Views.Strategy
{
    public abstract class GridViewStrategy
    {
        public abstract Sprite GetSprite(int health, GridState gridState);
    }
}