using System;
using System.Collections.Generic;
using PuzzleGame.Gameplay.Components;
using PuzzleGame.Gameplay.DataStructures;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PuzzleGame.Gameplay.Views
{
    public class LevelView : MonoBehaviour
    {
        [SerializeField] private GoalsComponent goalsComponent;
        [SerializeField] private MoveComponent moveComponent;
        [SerializeField] private BoardComponent boardComponent;
        [SerializeField] private Transform gridParent;
        [SerializeField] private LevelCompletedComponent levelCompletedComponent;
        private Vector3 _gridLocalScale;
        
        public Action DebugSetCubeStateAction;
        
        [Button]
        public void DebugSetCubeState()
        {
            DebugSetCubeStateAction?.Invoke();
        }

        public void SetUpLevel(int moveCount, Dictionary<GridType, int> goals, Vector2Int boardSize)
        {
            moveComponent.SetMoveCount(moveCount);
            goalsComponent.SetGoalsData(goals);
            boardComponent.SetBoardBackgroundBorders(boardSize);
            _gridLocalScale = gridParent.localScale;
            gridParent.localPosition = new Vector3(-((boardSize.x - 1) / 2f) * _gridLocalScale.x, 
                -((boardSize.y - 1) / 2f) * _gridLocalScale.y, 0);
        }
        
        public void OnGoalFound(GridType goalType, int foundGoalCount)
        {
            goalsComponent.OnGoalFound(goalType, foundGoalCount);
        }
        
        public void OnMoveHappened()
        {
            moveComponent.OnMoveHappened();
        }
        
        public void OnLevelCompleted(Action closeAction)
        {
            levelCompletedComponent.Open(closeAction);
        }
    }
}