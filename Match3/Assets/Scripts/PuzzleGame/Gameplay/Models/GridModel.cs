using System;
using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Gameplay.Models.Strategy;
using UnityEngine;

namespace PuzzleGame.Gameplay.Models
{
    public abstract class GridModel
    {
        private readonly BreakableModelStrategy _breakableModelStrategy;
        public GridType GridType { get; }
        public bool IsObstacle { get; protected set; }
        public bool IsSpecialItem { get; protected set; }
        public bool IsInteractable { get; protected set; }
        public int Id { get; private set; }
        public int Health { get; private set; }
        public int CreationHeightOffset { get; set; }
        public int ExplodeOffset { get; set; }
        public int BelowGridExplodeOffset { get; set; }
        public Vector2Int Position { get; private set; }

        public event Action<GridType> OnMatchEffectedGrid;
        public event Action OnDropGrid;

        protected GridModel(GridType gridType, int id, BreakableModelStrategy breakableModelStrategy, int health)
        {
            GridType = gridType;
            Id = id;
            Health = health;
            _breakableModelStrategy = breakableModelStrategy;
        }
        
        public void SetPosition(Vector2Int position)
        {
            Position = position;
        }

        public void DropGrid(Vector2Int position)
        {
            SetPosition(position);
            OnDropGrid?.Invoke();
        }

        public bool MatchEffectedGrid(bool isSpecialMatch, GridType otherGridType)
        {
            var canBreak = _breakableModelStrategy.TryBreak(isSpecialMatch);
            if (canBreak)
            {
                Health--;
                OnMatchEffectedGrid?.Invoke(otherGridType);
            }

            return Health == 0;
        }

        public abstract bool CanFall();

        public void Dispose()
        {
            
        }
    }
}