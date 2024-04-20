using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Gameplay.Models.Strategy;
using UnityEngine;

namespace PuzzleGame.Gameplay.Models
{
    public class SpecialItemModel : GridModel
    {
        public bool IsUsed { get; private set; }
        public bool UsedInCombination { get; private set; }
        
        private readonly SpecialItemModelStrategy _strategy;

        public SpecialItemModel(SpecialItemModelStrategy strategy, GridType gridType, int id,
            PlayableBreakableModelStrategy playableBreakableModelStrategy) : base(gridType, id, 
            playableBreakableModelStrategy, 1)
        {
            _strategy = strategy;
            IsSpecialItem = true;
            IsInteractable = true;
        }

        public SpecialItemModelStrategy UseAsCombinedSpecialItem()
        {
            UsedInCombination = true;
            IsUsed = true;
            return _strategy;
        }

        public Vector2Int[] UseSpecialItem(SpecialItemModelStrategy otherSpecialItemModeStrategy)
        {
            IsUsed = true;
            return otherSpecialItemModeStrategy == null
                ? _strategy.UseSpecialItem(Position)
                : _strategy.UseCombinedSpecialItem(Position, otherSpecialItemModeStrategy);
        }

        public override bool CanFall()
        {
            return true;
        }
    }
}