using PuzzleGame.Gameplay.Context;
using PuzzleGame.Gameplay.DataStructures;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.Gameplay.Components
{
    public class SingleGoalComponent : MonoBehaviour
    {
        [SerializeField] private Image goalImage;
        [SerializeField] private TMP_Text remainingGoalCountText;
        [SerializeField] private GameObject goalCompletedImage;
        
        public GridType GoalType { get; private set; }

        private int _remainingGoalCount;

        public void SetGoalData(GridType goalType, int remainingGoalCount)
        {
            GoalType = goalType;
            _remainingGoalCount = remainingGoalCount;
            goalImage.sprite = GameplayAssetProvider.Instance.GetGridSprite(goalType);
            remainingGoalCountText.text = remainingGoalCount.ToString();
            goalCompletedImage.SetActive(false);
        }

        public void GoalFound(int foundGoalCount)
        {
            _remainingGoalCount -= foundGoalCount;
            if (_remainingGoalCount == 0)
            {
                goalCompletedImage.SetActive(true);
                remainingGoalCountText.gameObject.SetActive(false);
            }
            else
            {
                remainingGoalCountText.text = _remainingGoalCount.ToString();
            }
        }
        
    }
}