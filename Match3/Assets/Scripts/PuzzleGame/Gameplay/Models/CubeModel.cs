using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Gameplay.Helpers;
using PuzzleGame.Gameplay.Models.Strategy;

namespace PuzzleGame.Gameplay.Models
{
    public class CubeModel : GridModel
    {
        private List<CubeModel> _adjacentCubes;
        private CancellationTokenSource _updateStateToken;
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

        public override void Dropped()
        {
            base.Dropped();
            if (_adjacentCubes == null) return;
            UpdateAdjacentCubesAsync().Forget();
        }

        private async UniTask UpdateAdjacentCubesAsync()
        {
            _updateStateToken?.Cancel();
            _updateStateToken = new CancellationTokenSource();
            await UniTask.Delay(TimeSpan.FromSeconds(GameplayVariables.Instance.CubeStateChangeWaitDuration),
                cancellationToken: _updateStateToken.Token);
            foreach (var cube in _adjacentCubes)
            {
                cube.OnAdjacentCubesUpdated(true);
            }
        }

        private void OnAdjacentCubesUpdated(bool shouldInvokeCallbacks)
        {
            var count = _adjacentCubes?.Count(c => !c.IsMoving) ?? 0;
            var state = count >= 5 ? GridState.Tnt :
                count >= 3 ? GridState.Rocket : 
                count == 2 ? GridState.Interactable : GridState.NonInteractable;
            State = state;
            IsInteractable = state != GridState.NonInteractable;
            if (shouldInvokeCallbacks)
                OnStateChanged?.Invoke();
        }

        public void SetDefaultState()
        {
            State = GridState.Default;
        }

        public void SetState(List<CubeModel> adjacentCubes, bool shouldInvokeCallbacks)
        {
            _updateStateToken?.Cancel();
            _adjacentCubes = adjacentCubes;
            OnAdjacentCubesUpdated(shouldInvokeCallbacks);
        }

        public override bool CanFall()
        {
            return true;
        }
    }
}