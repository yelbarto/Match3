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
        
        public abstract Vector2Int[] UseSpecialItem(Vector2Int position);
        public abstract Vector2Int[] UseCombinedSpecialItem(Vector2Int position, 
            SpecialItemModelStrategy otherSpecialItemModelStrategy);
    }
}