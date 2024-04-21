using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleGame.Gameplay.Context;
using PuzzleGame.Gameplay.DataStructures;
using UnityEngine;

namespace PuzzleGame.Gameplay.Models.Strategy
{
    public class RocketModelStrategy : SpecialItemModelStrategy
    {
        private readonly Direction _direction;

        public RocketModelStrategy(Direction direction)
        {
            _direction = direction;
        }

        public override Dictionary<Vector2Int, int> UseSpecialItem(Vector2Int position)
        {
            return UseRocket(position, _direction);
        }

        private Dictionary<Vector2Int, int> UseRocket(Vector2Int position, Direction direction)
        {
            var effectedPositions = new Dictionary<Vector2Int, int>();
            switch (direction)
            {
                case Direction.Vertical:
                    for (var y = 0; y < Borders.y; y++)
                    {
                        effectedPositions.Add(new Vector2Int(position.x, y),
                            Mathf.Max(0,
                                Math.Abs(position.y - y) - GameplayVariables.Instance.RocketInitialExplodeDistance));
                    }

                    return effectedPositions;
                case Direction.Horizontal:
                    for (var x = 0; x < Borders.x; x++)
                    {
                        effectedPositions.Add(new Vector2Int(x, position.y),
                            Mathf.Max(0,
                                Math.Abs(position.x - x) - GameplayVariables.Instance.RocketInitialExplodeDistance));
                    }

                    return effectedPositions;
                default:
                    throw new Exception("Invalid direction type. " + _direction);
            }
        }

        public override Dictionary<Vector2Int, int> UseCombinedSpecialItem(Vector2Int position,
            SpecialItemModelStrategy otherSpecialItemModelStrategy)
        {
            return otherSpecialItemModelStrategy.GetType() == GetType()
                ? UseRocketCombination(position)
                : otherSpecialItemModelStrategy.UseCombinedSpecialItem(position, this);
        }

        private Dictionary<Vector2Int, int> UseRocketCombination(Vector2Int position)
        {
            var effectedPositions = UseRocket(position, Direction.Horizontal);
            var verticalRocketEffectedPositions = UseRocket(position, Direction.Vertical);
            verticalRocketEffectedPositions.Remove(position);
            effectedPositions = effectedPositions.Concat(verticalRocketEffectedPositions)
                .ToDictionary(e => e.Key, e => e.Value);
            return effectedPositions;
        }
    }
}