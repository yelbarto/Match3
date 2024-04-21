using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.Gameplay.Models.Strategy
{
    public abstract class SpecialItemModelStrategy
    {
        protected Vector2Int Borders;

        public void SetBorders(Vector2Int borders)
        {
            Borders = borders;
        }
        
        public abstract Dictionary<Vector2Int, int> UseSpecialItem(Vector2Int position);
        public abstract Dictionary<Vector2Int, int> UseCombinedSpecialItem(Vector2Int position, 
            SpecialItemModelStrategy otherSpecialItemModelStrategy);
    }
}