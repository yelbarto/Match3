using UnityEngine;

namespace PuzzleGame.Gameplay.Helpers
{
    public class EffectedAreaCalculator
    {
        public static Vector2Int[] CalculatedTntEffectedArea(Vector2Int borders, Vector2Int position, int range)
        {
            var minX = Mathf.Max(0, position.x - range);
            var minY = Mathf.Max(0, position.y - range);
            var maxX = Mathf.Min(borders.x - 1, position.x + range);
            var maxY = Mathf.Min(borders.y - 1, position.y + range);
            var effectedPositions = new Vector2Int[(maxX - minX + 1) * (maxY - minY + 1)];
            var index = 0;
            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    effectedPositions[index] = new Vector2Int(x, y);
                    index++;
                }
            }

            return effectedPositions;
        }
    }
}