using System.Collections.Generic;
using System.Linq;
using PuzzleGame.Gameplay.Helpers;
using UnityEngine;

namespace PuzzleGame.Gameplay.Models.Strategy
{
    public class TntModelStrategy : SpecialItemModelStrategy
    {
        private readonly RocketModelStrategy _horizontalRocketStrategy;
        private readonly RocketModelStrategy _verticalRocketStrategy;

        public TntModelStrategy(RocketModelStrategy verticalRocketStrategy,
            RocketModelStrategy horizontalRocketStrategy)
        {
            _verticalRocketStrategy = verticalRocketStrategy;
            _horizontalRocketStrategy = horizontalRocketStrategy;
        }
        
        public override Vector2Int[] UseSpecialItem(Vector2Int position)
        {
            return CalculatedTntEffectedArea(position, 2);
        }

        public override Vector2Int[] UseCombinedSpecialItem(Vector2Int position,
            SpecialItemModelStrategy otherSpecialItemModelStrategy)
        {
            return otherSpecialItemModelStrategy.GetType() == GetType() 
                ? UseTntCombination(position) 
                : UseRocketCombination(position);
        }

        private Vector2Int[] UseTntCombination(Vector2Int position)
        {
            return CalculatedTntEffectedArea(position, 3);
        }

        public Vector2Int[] UseRocketCombination(Vector2Int position)
        {
            var tntEffectArea = CalculatedTntEffectedArea(position, 1);
            var rocketAreas = new List<Vector2Int>();
            foreach (var effectedPosition in tntEffectArea)
            {
                rocketAreas.AddRange(_verticalRocketStrategy.UseSpecialItem(effectedPosition));
                rocketAreas.AddRange(_horizontalRocketStrategy.UseSpecialItem(effectedPosition));
            }
            return rocketAreas.Distinct().ToArray();
        }

        private Vector2Int[] CalculatedTntEffectedArea(Vector2Int position, int range)
        {
            return EffectedAreaCalculator.CalculatedTntEffectedArea(Borders, position, range);
        }
    }
}