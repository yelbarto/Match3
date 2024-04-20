using System;
using System.Linq;
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
        
        public override Vector2Int[] UseSpecialItem(Vector2Int position)
        {
            return UseRocket(position, _direction);
        }

        private Vector2Int[] UseRocket(Vector2Int position, Direction direction)
        {
            var index = 0;
            Vector2Int[] effectedPositions;
            switch (direction)
            {
                case Direction.Vertical:
                    effectedPositions = new Vector2Int[Borders.y];
                    for (var y = 0; y < Borders.y; y++)
                    {
                        effectedPositions[index] = new Vector2Int(position.x, y);
                        index++;
                    }

                    return effectedPositions;
                case Direction.Horizontal:
                    effectedPositions = new Vector2Int[Borders.x];
                    for (var x = 0; x < Borders.x; x++)
                    {
                        effectedPositions[index] = new Vector2Int(x, position.y);
                        index++;
                    }

                    return effectedPositions;
                default:
                    throw new Exception("Invalid direction type. " + _direction);
            }
        }

        public override Vector2Int[] UseCombinedSpecialItem(Vector2Int position,
            SpecialItemModelStrategy otherSpecialItemModelStrategy)
        {
            return otherSpecialItemModelStrategy.GetType() == GetType() 
                ? UseRocketCombination(position) 
                : otherSpecialItemModelStrategy.UseCombinedSpecialItem(position, this);
        }

        private Vector2Int[] UseRocketCombination(Vector2Int position)
        {
            var effectedPositions = UseRocket(position, Direction.Horizontal).ToList();
            effectedPositions.AddRange(UseRocket(position, Direction.Vertical));
            effectedPositions.Remove(position);
            return effectedPositions.ToArray();
        }
    }
}