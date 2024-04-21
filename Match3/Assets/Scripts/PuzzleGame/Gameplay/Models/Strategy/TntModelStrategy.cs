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
        
        public override Dictionary<Vector2Int, int> UseSpecialItem(Vector2Int position)
        {
            return CalculatedTntEffectedArea(position, 2);
        }

        public override Dictionary<Vector2Int, int> UseCombinedSpecialItem(Vector2Int position,
            SpecialItemModelStrategy otherSpecialItemModelStrategy)
        {
            return otherSpecialItemModelStrategy.GetType() == GetType() 
                ? UseTntCombination(position) 
                : UseRocketCombination(position);
        }

        private Dictionary<Vector2Int, int> UseTntCombination(Vector2Int position)
        {
            return CalculatedTntEffectedArea(position, 3);
        }

        private Dictionary<Vector2Int, int> UseRocketCombination(Vector2Int position)
        {
            var tntEffectArea = CalculatedTntEffectedArea(position, 1);
            var rocketAreas = new List<Vector2Int>();
            var rocketDictionary = new Dictionary<Vector2Int, int>();
            foreach (var effectedPosition in tntEffectArea)
            {
                rocketAreas.AddRange(_verticalRocketStrategy.UseSpecialItem(effectedPosition.Key).Keys);
                rocketAreas.AddRange(_horizontalRocketStrategy.UseSpecialItem(effectedPosition.Key).Keys);
            }
            var distinctAreas = rocketAreas.Distinct();
            var minTntArea = new Vector2Int(int.MaxValue, int.MaxValue);
            var maxTntArea = new Vector2Int(int.MinValue, int.MinValue);
            foreach (var tntArea in tntEffectArea)
            {
                minTntArea.x = Mathf.Min(minTntArea.x, tntArea.Key.x);
                minTntArea.y = Mathf.Min(minTntArea.y, tntArea.Key.y);
                maxTntArea.x = Mathf.Max(maxTntArea.x, tntArea.Key.x);
                maxTntArea.y = Mathf.Max(maxTntArea.y, tntArea.Key.y);
            }
            foreach (var area in distinctAreas)
            {
                var xDiff = Mathf.Abs(area.x > maxTntArea.x 
                    ? area.x - maxTntArea.x 
                    : area.x < maxTntArea.x 
                        ? area.x - minTntArea.x
                        : 0);
                var yDiff = Mathf.Abs(area.y > maxTntArea.y 
                    ? area.y - maxTntArea.y 
                    : area.y < maxTntArea.y
                        ? area.y - minTntArea.y
                        : 0);
                rocketDictionary.Add(area, Mathf.Max(xDiff, yDiff));
            }
            return rocketDictionary;
        }

        private Dictionary<Vector2Int, int> CalculatedTntEffectedArea(Vector2Int position, int range)
        {
            return EffectedAreaCalculator.CalculatedTntEffectedArea(Borders, position, range)
                .ToDictionary(v => v, _ => 0);
        }
    }
}