using System;
using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Gameplay.Models.Strategy;

namespace PuzzleGame.Gameplay.Models
{
    public class CubeModel : GridModel
    {
        public GridColor Color { get; }
        public GridState State { get; private set; }
        public event Action OnStateChanged;

        public CubeModel(GridColor color, int id, PlayableBreakableModelStrategy strategy) : base(GridType.Cube, id, 
            strategy, 1)
        {
            Color = color;
        }

        public virtual bool IsSameColor(GridColor color)
        {
            return color == Color;
        }
        
        public void SetState(GridState state, bool shouldInvokeCallbacks)
        {
            State = state;
            IsInteractable = state != GridState.NonInteractable;
            if (shouldInvokeCallbacks)
                OnStateChanged?.Invoke();
        }

        public override bool CanFall()
        {
            return true;
        }
    }
}