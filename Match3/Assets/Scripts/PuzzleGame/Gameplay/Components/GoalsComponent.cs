using System;
using System.Collections.Generic;
using PuzzleGame.Gameplay.DataStructures;
using UnityEngine;

namespace PuzzleGame.Gameplay.Components
{
    public class GoalsComponent : MonoBehaviour
    {
        [SerializeField] private List<GoalsUiData> goalsUiDataList;
        private GoalsUiData _currentGoalsUiData;

        public void SetGoalsData(Dictionary<GridType, int> goals)
        {
            var currentGoalsCount = goals.Count;
            foreach (var goalsUiData in goalsUiDataList)
            {
                if (goalsUiData.GoalsCount != currentGoalsCount)
                {
                    goalsUiData.GoalsParent.SetActive(false);
                    continue;
                }

                goalsUiData.GoalsParent.SetActive(true);
                _currentGoalsUiData = goalsUiData;
                var goalsEnumerator = goals.GetEnumerator();
                for (var i = 0; i < currentGoalsCount; i++)
                {
                    goalsEnumerator.MoveNext();
                    var currentGoal = goalsEnumerator.Current;
                    _currentGoalsUiData.SingleGoalComponents[i].SetGoalData(currentGoal.Key, currentGoal.Value);
                }
                goalsEnumerator.Dispose();
            }
        }

        public void OnGoalFound(GridType gridType, int foundGoalCount)
        {
            foreach (var singleGoalComponent in _currentGoalsUiData.SingleGoalComponents)
            {
                if (singleGoalComponent.GoalType == gridType)
                {
                    singleGoalComponent.GoalFound(foundGoalCount);
                    return;
                }
            }
        }
    }

    [Serializable]
    public struct GoalsUiData
    {
        [SerializeField] private int goalsCount;
        [SerializeField] private SingleGoalComponent[] singleGoalComponents;
        [SerializeField] private GameObject goalsParent;
        
        public int GoalsCount => goalsCount;
        public SingleGoalComponent[] SingleGoalComponents => singleGoalComponents;
        public GameObject GoalsParent => goalsParent;
    }
}