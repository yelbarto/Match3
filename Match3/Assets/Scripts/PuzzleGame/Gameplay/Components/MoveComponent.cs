using TMPro;
using UnityEngine;

namespace PuzzleGame.Gameplay.Components
{
    public class MoveComponent : MonoBehaviour
    {
        [SerializeField] private TMP_Text moveText;
        private int _currentMoveCount;

        public void SetMoveCount(int moveCount)
        {
            _currentMoveCount = moveCount;
            moveText.text = moveCount.ToString();
        }

        public void OnMoveHappened()
        {
            _currentMoveCount--;
            moveText.text = _currentMoveCount.ToString();
        }
    }
}