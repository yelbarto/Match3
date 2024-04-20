using System.Threading;
using Cysharp.Threading.Tasks;
using PuzzleGame.Gameplay.Components;
using PuzzleGame.Gameplay.Context;
using PuzzleGame.Gameplay.DataStructures;
using PuzzleGame.Gameplay.Helpers;
using UnityEngine;

namespace PuzzleGame.Gameplay.Views.Strategy
{
    public class RocketViewStrategy : SpecialItemViewStrategy
    {
        private readonly Vector3 _topRightPosition;
        private readonly Vector3 _bottomLeftPosition;

        public RocketViewStrategy(GridType gridType) : base(gridType)
        {
            _topRightPosition = GameplayVariables.Instance.TopRightRocketPositioner.position;
            _bottomLeftPosition = GameplayVariables.Instance.BottomLeftRocketPositioner.position;
        }

        public override async UniTask PlayBreakAnimation(Transform zeroPointPosition, Vector3 position)
        {
            var particleSystem = GridParticleProvider.Instance.GetRandomizedParticleSystem(GridType, GridColor.Default);
            var rocketComponent = particleSystem.GetComponent<RocketAnimationComponent>();
            await rocketComponent.PlayAnimationAsync(zeroPointPosition.position, _topRightPosition, 
                _bottomLeftPosition);
        }

        public override async UniTask PlaySuperBreakAnimation(Transform zeroPointPosition, Vector3 position,
            GridViewStrategy otherBrokenGrid)
        {
            if (otherBrokenGrid.GridType is GridType.Tnt)
            {
                await otherBrokenGrid.PlaySuperBreakAnimation(zeroPointPosition, position, this);
            }
            else
            {
                await PlayBreakAnimation(zeroPointPosition, position);
                await otherBrokenGrid.PlayBreakAnimation(zeroPointPosition, position);
            }
        }
    }
}