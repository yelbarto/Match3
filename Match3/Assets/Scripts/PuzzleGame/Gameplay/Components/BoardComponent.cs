using Sirenix.OdinInspector;
using UnityEngine;

namespace PuzzleGame.Gameplay.Components
{
    public class BoardComponent : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer boardBackgroundSprite;
        [SerializeField] private Vector2 singleCellSize = new(1.42f, 1.62f);
        [SerializeField] private Vector2 boardBorderOffset = new(0.28f, 0.28f);
        
        [Button]
        public void SetBoardBackgroundBorders(Vector2Int boardSize)
        {
            boardBackgroundSprite.size = new Vector2(boardSize.x * singleCellSize.x + boardBorderOffset.x, 
                boardSize.y * singleCellSize.y + boardBorderOffset.y);
        }
    }
}